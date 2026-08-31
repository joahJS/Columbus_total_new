using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 통합 허브 DB(COLUMBUS_WEIGH_HUB)의 dbo.WEIGH_RECORD 테이블을 조회하는 실제 구현체.
    /// 이 프로그램은 조회/집계 전용이므로 INSERT/UPDATE/DELETE는 하지 않는다(계량 입력은
    /// 각 지점이 지금 쓰는 프로그램에서 그대로 처리하고, 동기화 잡이 이 테이블을 채운다).
    /// </summary>
    public sealed class SqlWeighingRepository : IWeighingRepository
    {
        private const string SelectSql = @"
SELECT WEIGH_ID, WEIGH_SEQ, VEHICLE_NO, CUSTOMER_NAME, PRODUCT_NAME, IN_OUT_TYPE,
       WEIGHER_NAME, UNIT_PRICE, REMARK, WEIGH_DATE, FIRST_DATETIME, FIRST_WEIGHT,
       SECOND_DATETIME, SECOND_WEIGHT, LOSS_WEIGHT
FROM dbo.WEIGH_RECORD
WHERE WEIGH_DATE >= @FromDate AND WEIGH_DATE < @ToDate
ORDER BY FIRST_DATETIME";

        public BindingList<WeighingRecord> Records { get; } = new BindingList<WeighingRecord>();

        /// <summary>
        /// 화면을 처음 열 때는 아무도 Refresh()를 호출해주지 않으므로(2차계량 화면의 조회일자가
        /// 바뀔 때만 Refresh가 호출됨), 생성 시점에 기본 조회 기간(오늘부터 며칠 전까지)으로
        /// 한 번 채워둔다. 며칠 전 것까지 같이 보여주는 이유는 "1차 계량 대기" 목록이 날짜
        /// 조건 없이 이 Records 전체에서 미완료 건을 그대로 뽑아 쓰기 때문 - 전날 넘어와서
        /// 아직 2차 계량 전인 차량도 놓치지 않게 하려는 것이다.
        /// </summary>
        public SqlWeighingRepository()
        {
            // VS 디자이너가 MainForm을 디자인 타임에 로드할 때도 이 생성자가 호출되는데,
            // 그 시점에 실제 DB 접속을 시도하면 디자이너가 멈추거나 오류가 날 수 있어 건너뛴다.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            var toDate = DateTime.Today.AddDays(1);
            var fromDate = DateTime.Today.AddDays(-DefaultLookbackDays);
            Refresh(fromDate, toDate);
        }

        private static int DefaultLookbackDays
        {
            get
            {
                var raw = ConfigurationManager.AppSettings["WeighQueryLookbackDays"];
                int days;
                return int.TryParse(raw, out days) && days > 0 ? days : 3;
            }
        }

        public void Refresh(DateTime fromDate, DateTime toDate)
        {
            DataTable table;
            try
            {
                table = DBConn.GetDataTable(
                    SelectSql,
                    new List<Parameter>
                    {
                        new Parameter("FromDate", fromDate.Date, SqlDbType.Date),
                        new Parameter("ToDate", toDate.Date, SqlDbType.Date),
                    },
                    CommandType.Text);
            }
            catch (SqlException ex)
            {
                // 허브 DB에 일시적으로 접속이 안 되는 경우, 화면 전체를 죽이는 대신 기존에
                // 보여주던 목록은 그대로 두고 오류만 안내한다.
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

        private static WeighingRecord ToRecord(DataRow row)
        {
            var weighDate = (DateTime)row["WEIGH_DATE"];

            return new WeighingRecord
            {
                Id = Convert.ToInt32(row["WEIGH_ID"]),
                WeighSeq = AsNullableInt(row, "WEIGH_SEQ") ?? 0,
                VehicleNo = AsString(row, "VEHICLE_NO"),
                CustomerName = AsString(row, "CUSTOMER_NAME"),
                ProductName = AsString(row, "PRODUCT_NAME"),
                InOutType = AsString(row, "IN_OUT_TYPE") == "I" ? InOutType.In : InOutType.Out,
                WeigherName = AsString(row, "WEIGHER_NAME"),
                UnitPrice = AsNullableDecimal(row, "UNIT_PRICE"),
                Remark = AsString(row, "REMARK"),
                FirstDateTime = AsNullableDateTime(row, "FIRST_DATETIME") ?? weighDate,
                FirstWeight = AsNullableDecimal(row, "FIRST_WEIGHT") ?? 0m,
                SecondDateTime = AsNullableDateTime(row, "SECOND_DATETIME"),
                SecondWeight = AsNullableDecimal(row, "SECOND_WEIGHT"),
                LossWeight = AsNullableDecimal(row, "LOSS_WEIGHT"),
            };
        }

        private static string AsString(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? null : row[column].ToString();
        }

        private static int? AsNullableInt(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? (int?)null : Convert.ToInt32(row[column]);
        }

        private static decimal? AsNullableDecimal(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row[column]);
        }

        private static DateTime? AsNullableDateTime(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row[column]);
        }
    }
}
