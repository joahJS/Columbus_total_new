using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ColumbusWeighing.ComnLib;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 통합 허브 DB(dbo.APP_USER)에 등록된 계정으로 로그인을 검증한다. 비밀번호는 기본적으로
    /// PBKDF2 해시로 저장되어 있으므로 PasswordHasher로 검증하지만, A지점(MES)에서 동기화된
    /// 계정은 PASSWORD_ALGORITHM='SHA256'으로 표시되어 있어 MES가 쓰는 방식
    /// (HASHBYTES('SHA2_256', 평문))으로 검증한다 - MES는 평문을 어디에도 남기지 않아서
    /// PBKDF2로 재해시할 방법이 없기 때문이다(ColumbusSync.BranchA/Hub/HubWriter.cs 참고).
    /// 평문 비밀번호는 어디에도 남기지 않는다.
    /// 이 테이블/서비스가 생기기 전까지 쓰이던 고정 계정(FixedAuthenticationService)은 이제
    /// VS 디자이너 전용 생성자에서만 쓰인다.
    /// 로그인은 지점과 무관하게 LOGIN_ID 하나로만 판단한다(지점 선택 UI는 일단 숨김 처리 -
    /// LoginForm 참고). branchCode 파라미터는 현재 쓰이지 않지만, 최고관리자가 아이디별로
    /// 조회 가능한 지점 권한을 부여하는 기능이 추가될 때를 대비해 인터페이스에 남겨둔다.
    /// </summary>
    public sealed class SqlAuthenticationService : IAuthenticationService
    {
        private const string SelectSql = @"
SELECT DISPLAY_NAME, PASSWORD_HASH, PASSWORD_SALT, PASSWORD_ALGORITHM
FROM dbo.APP_USER
WHERE LOGIN_ID = @LoginId";

        public bool TryLogin(string branchCode, string userId, string password, out string displayName)
        {
            displayName = null;

            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            DataTable table;
            try
            {
                table = DBConn.GetDataTable(
                    SelectSql,
                    new List<Parameter>
                    {
                        new Parameter("LoginId", userId),
                    },
                    CommandType.Text);
            }
            catch (SqlException ex)
            {
                ComnFunc.gp_PrintMessage(
                    "통합 허브 DB 접속에 실패해 로그인할 수 없습니다.\r\n" + ex.Message,
                    "DB 접속 오류", MessageType.오류);
                return false;
            }

            if (table.Rows.Count == 0)
            {
                return false;
            }

            var row = table.Rows[0];
            var algorithm = row["PASSWORD_ALGORITHM"].ToString();
            var isValid = string.Equals(algorithm, "SHA256", StringComparison.OrdinalIgnoreCase)
                ? VerifyMesSha256Password(password, row["PASSWORD_HASH"].ToString())
                : PasswordHasher.Verify(password, row["PASSWORD_HASH"].ToString(), row["PASSWORD_SALT"].ToString());

            if (!isValid)
            {
                return false;
            }

            displayName = row["DISPLAY_NAME"].ToString();
            return true;
        }

        /// <summary>A지점(MES)에서 동기화된 계정을 MES와 같은 방식으로 검증한다. MES는
        /// SQL Server의 HASHBYTES('SHA2_256', 평문 VARCHAR)로 해시하므로, C#에서 SHA256을
        /// 직접 계산하면 문자 인코딩 차이로 바이트가 어긋날 위험이 있다 - 그래서 같은 허브
        /// DB(SQL Server)에 똑같이 HASHBYTES를 시켜서 비교한다(항상 같은 서버가 계산하므로
        /// 인코딩 불일치 문제가 없다).</summary>
        private static bool VerifyMesSha256Password(string password, string storedHashBase64)
        {
            byte[] storedHash;
            try
            {
                storedHash = Convert.FromBase64String(storedHashBase64);
            }
            catch (FormatException)
            {
                return false;
            }

            const string sql = "SELECT CASE WHEN HASHBYTES('SHA2_256', @Password) = @StoredHash THEN 1 ELSE 0 END AS IsMatch";

            var table = DBConn.GetDataTable(
                sql,
                new List<Parameter>
                {
                    new Parameter("Password", password ?? string.Empty, SqlDbType.VarChar),
                    new Parameter("StoredHash", storedHash, SqlDbType.VarBinary),
                },
                CommandType.Text);

            return table.Rows.Count > 0 && Convert.ToInt32(table.Rows[0]["IsMatch"]) == 1;
        }
    }
}
