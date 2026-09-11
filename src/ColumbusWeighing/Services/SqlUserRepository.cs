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
    /// 통합 허브 DB(COLUMBUS_WEIGH_HUB)의 dbo.APP_USER 테이블을 조회하는 실제 구현체.
    /// 이 프로그램은 조회 전용이므로 INSERT/UPDATE/DELETE는 하지 않는다(계정 추가/수정/비밀번호
    /// 변경은 아직 이 화면에서 다루지 않는다 - 필요해지면 별도로 구현). 비밀번호 해시/솔트는
    /// 화면에 보여줄 필요가 없으므로 SELECT 자체에서 제외한다.
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
    }
}
