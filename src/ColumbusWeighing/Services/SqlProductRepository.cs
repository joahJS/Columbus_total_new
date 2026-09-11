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
    /// 통합 허브 DB(COLUMBUS_WEIGH_HUB)의 dbo.PRODUCT 테이블을 조회하는 실제 구현체.
    /// 이 프로그램은 조회 전용이므로 INSERT/UPDATE/DELETE는 하지 않는다(제품 등록/수정은
    /// 각 지점이 지금 쓰는 프로그램에서 그대로 처리하고, 동기화 잡이 이 테이블을 채운다).
    /// </summary>
    public sealed class SqlProductRepository : IProductRepository
    {
        private const string SelectSql = @"
SELECT PRODUCT_ID, BRANCH_CODE, SOURCE_CODE, PRODUCT_NAME, UNIT, UNIT_PRICE, LOSS_WEIGHT, LOSS_RATE, REMARK, SYNCED_AT
FROM dbo.PRODUCT
WHERE (@Search = '' OR PRODUCT_NAME LIKE '%' + @Search + '%' OR SOURCE_CODE LIKE '%' + @Search + '%')
ORDER BY BRANCH_CODE, SOURCE_CODE";

        public BindingList<ProductRecord> Records { get; } = new BindingList<ProductRecord>();

        public SqlProductRepository()
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

        private static ProductRecord ToRecord(DataRow row)
        {
            return new ProductRecord
            {
                Id = Convert.ToInt32(row["PRODUCT_ID"]),
                BranchCode = AsString(row, "BRANCH_CODE"),
                ProductCode = AsString(row, "SOURCE_CODE"),
                ProductName = AsString(row, "PRODUCT_NAME"),
                Unit = AsString(row, "UNIT"),
                UnitPrice = AsNullableDecimal(row, "UNIT_PRICE"),
                LossWeight = AsNullableDecimal(row, "LOSS_WEIGHT"),
                LossRate = AsNullableDecimal(row, "LOSS_RATE"),
                Remark = AsString(row, "REMARK"),
                SyncedAt = Convert.ToDateTime(row["SYNCED_AT"]),
            };
        }

        private static string AsString(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? null : row[column].ToString();
        }

        private static decimal? AsNullableDecimal(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row[column]);
        }
    }
}
