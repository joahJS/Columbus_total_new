using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 통합 허브 DB(COLUMBUS_WEIGH_HUB)의 dbo.APP_USER 테이블을 조회/추가/수정/삭제하는 실제
    /// 구현체. 비밀번호는 PasswordHasher로 해시해서만 저장하고, 조회 SQL 자체에서 해시/솔트
    /// 컬럼은 제외해 화면으로 나가지 않게 한다. 관리자 계정이 하나도 안 남는 상황(마지막
    /// 관리자 삭제/권한 해제)은 로그인 자체가 막히므로 Update/Delete에서 막는다.
    /// </summary>
    public sealed class SqlUserRepository : IUserRepository
    {
        private const string SelectSql = @"
SELECT USER_ID, LOGIN_ID, DISPLAY_NAME, PHONE, REMARK, CAN_PRINT, CAN_EDIT, CAN_DELETE, IS_ADMIN,
       MODIFIED_BY, MODIFIED_AT
FROM dbo.APP_USER
WHERE (@Search = '' OR LOGIN_ID LIKE '%' + @Search + '%' OR DISPLAY_NAME LIKE '%' + @Search + '%')
ORDER BY USER_ID";

        public BindingList<UserAccount> Records { get; } = new BindingList<UserAccount>();

        public SqlUserRepository()
        {
            // VS 디자이너가 폼을 디자인 타임에 로드할 때도 이 생성자가 호출되는데, 그 시점에
            // 실제 DB 접속을 시도하면 디자이너가 멈추거나 오류가 날 수 있어 건너뛴다.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            Refresh(string.Empty);
        }

        public void Refresh(string searchText)
        {
            DataTable table;
            try
            {
                table = DBConn.GetDataTable(
                    SelectSql,
                    new List<Parameter>
                    {
                        new Parameter("Search", searchText ?? string.Empty),
                    },
                    CommandType.Text);
            }
            catch (SqlException ex)
            {
                ComnFunc.gp_PrintMessage(
                    "통합 허브 DB 조회에 실패했습니다. 네트워크/DB 접속 정보를 확인해주세요.\r\n" + ex.Message,
                    "DB 조회 오류", MessageType.오류);
                return;
            }

            Records.RaiseListChangedEvents = false;
            Records.Clear();
            foreach (DataRow row in table.Rows)
            {
                Records.Add(ToRecord(row));
            }

            Records.RaiseListChangedEvents = true;
            Records.ResetBindings();
        }

        private static UserAccount ToRecord(DataRow row)
        {
            return new UserAccount
            {
                Id = Convert.ToInt32(row["USER_ID"]),
                LoginId = AsString(row, "LOGIN_ID"),
                DisplayName = AsString(row, "DISPLAY_NAME"),
                Phone = AsString(row, "PHONE"),
                Remark = AsString(row, "REMARK"),
                CanPrint = Convert.ToBoolean(row["CAN_PRINT"]),
                CanEdit = Convert.ToBoolean(row["CAN_EDIT"]),
                CanDelete = Convert.ToBoolean(row["CAN_DELETE"]),
                IsAdmin = Convert.ToBoolean(row["IS_ADMIN"]),
                ModifiedBy = AsString(row, "MODIFIED_BY"),
                ModifiedAt = Convert.ToDateTime(row["MODIFIED_AT"]),
            };
        }

        private static string AsString(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? null : row[column].ToString();
        }

        public void Add(UserAccount account, string password, string modifiedBy)
        {
            string hash, salt;
            PasswordHasher.CreateHash(password, out hash, out salt);

            const string sql = @"
INSERT INTO dbo.APP_USER
    (LOGIN_ID, DISPLAY_NAME, PHONE, REMARK, CAN_PRINT, CAN_EDIT, CAN_DELETE, IS_ADMIN,
     PASSWORD_HASH, PASSWORD_SALT, MODIFIED_BY, MODIFIED_AT)
VALUES
    (@LoginId, @DisplayName, @Phone, @Remark, @CanPrint, @CanEdit, @CanDelete, @IsAdmin,
     @PasswordHash, @PasswordSalt, @ModifiedBy, SYSDATETIME())";

            var parameters = BuildAccountParameters(account, modifiedBy);
            parameters.Add(new Parameter("PasswordHash", hash));
            parameters.Add(new Parameter("PasswordSalt", salt));

            ExecuteWrite(sql, parameters, account.LoginId);
        }

        public void Update(UserAccount account, string newPassword, string modifiedBy)
        {
            if (!account.IsAdmin && IsLastAdmin(account.Id))
            {
                throw new InvalidOperationException("마지막 관리자 계정의 관리자 권한은 해제할 수 없습니다.");
            }

            var parameters = BuildAccountParameters(account, modifiedBy);
            parameters.Add(new Parameter("UserId", account.Id, SqlDbType.Int));

            var setPasswordSql = string.Empty;
            if (!string.IsNullOrEmpty(newPassword))
            {
                string hash, salt;
                PasswordHasher.CreateHash(newPassword, out hash, out salt);
                setPasswordSql = ", PASSWORD_HASH = @PasswordHash, PASSWORD_SALT = @PasswordSalt";
                parameters.Add(new Parameter("PasswordHash", hash));
                parameters.Add(new Parameter("PasswordSalt", salt));
            }

            var sql = string.Format(@"
UPDATE dbo.APP_USER
SET LOGIN_ID = @LoginId, DISPLAY_NAME = @DisplayName, PHONE = @Phone, REMARK = @Remark,
    CAN_PRINT = @CanPrint, CAN_EDIT = @CanEdit, CAN_DELETE = @CanDelete, IS_ADMIN = @IsAdmin,
    MODIFIED_BY = @ModifiedBy, MODIFIED_AT = SYSDATETIME(){0}
WHERE USER_ID = @UserId", setPasswordSql);

            ExecuteWrite(sql, parameters, account.LoginId);
        }

        public void Delete(int userId)
        {
            if (IsLastAdmin(userId))
            {
                throw new InvalidOperationException("마지막 관리자 계정은 삭제할 수 없습니다.");
            }

            const string sql = "DELETE FROM dbo.APP_USER WHERE USER_ID = @UserId";
            DBConn.ExecuteNonQuery(sql, new List<Parameter> { new Parameter("UserId", userId, SqlDbType.Int) }, CommandType.Text);
        }

        private static List<Parameter> BuildAccountParameters(UserAccount account, string modifiedBy)
        {
            return new List<Parameter>
            {
                new Parameter("LoginId", account.LoginId),
                new Parameter("DisplayName", account.DisplayName),
                new Parameter("Phone", account.Phone),
                new Parameter("Remark", account.Remark),
                new Parameter("CanPrint", account.CanPrint, SqlDbType.Bit),
                new Parameter("CanEdit", account.CanEdit, SqlDbType.Bit),
                new Parameter("CanDelete", account.CanDelete, SqlDbType.Bit),
                new Parameter("IsAdmin", account.IsAdmin, SqlDbType.Bit),
                new Parameter("ModifiedBy", modifiedBy),
            };
        }

        private static void ExecuteWrite(string sql, List<Parameter> parameters, string loginId)
        {
            try
            {
                DBConn.ExecuteNonQuery(sql, parameters, CommandType.Text);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                throw new DuplicateLoginIdException(loginId);
            }
        }

        /// <summary>userId가 관리자이면서, 그 계정을 뺀 나머지 중에는 관리자가 하나도 없는지 확인한다
        /// (Update에서 관리자 권한을 해제하는 경우/Delete 모두 이 기준으로 마지막 관리자를 지킨다).</summary>
        private static bool IsLastAdmin(int userId)
        {
            const string sql = @"
SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.APP_USER WHERE USER_ID = @UserId AND IS_ADMIN = 1)
             AND NOT EXISTS (SELECT 1 FROM dbo.APP_USER WHERE USER_ID <> @UserId AND IS_ADMIN = 1)
        THEN 1 ELSE 0 END";

            var table = DBConn.GetDataTable(
                sql,
                new List<Parameter> { new Parameter("UserId", userId, SqlDbType.Int) },
                CommandType.Text);

            return table.Rows.Count > 0 && Convert.ToInt32(table.Rows[0][0]) == 1;
        }
    }
}
