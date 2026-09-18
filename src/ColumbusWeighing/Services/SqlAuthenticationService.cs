using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ColumbusWeighing.ComnLib;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 통합 허브 DB(dbo.APP_USER)에 등록된 계정으로 로그인을 검증한다. 비밀번호는 PBKDF2
    /// 해시로 저장되어 있으므로 PasswordHasher로 검증하고, 평문 비밀번호는 어디에도 남기지 않는다.
    /// 이 테이블/서비스가 생기기 전까지 쓰이던 고정 계정(FixedAuthenticationService)은 이제
    /// VS 디자이너 전용 생성자에서만 쓰인다.
    /// 로그인은 지점과 무관하게 LOGIN_ID 하나로만 판단한다(지점 선택 UI는 일단 숨김 처리 -
    /// LoginForm 참고). branchCode 파라미터는 현재 쓰이지 않지만, 최고관리자가 아이디별로
    /// 조회 가능한 지점 권한을 부여하는 기능이 추가될 때를 대비해 인터페이스에 남겨둔다.
    /// </summary>
    public sealed class SqlAuthenticationService : IAuthenticationService
    {
        private const string SelectSql = @"
SELECT DISPLAY_NAME, PASSWORD_HASH, PASSWORD_SALT
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
            var isValid = PasswordHasher.Verify(password, row["PASSWORD_HASH"].ToString(), row["PASSWORD_SALT"].ToString());
            if (!isValid)
            {
                return false;
            }

            displayName = row["DISPLAY_NAME"].ToString();
            return true;
        }
    }
}
