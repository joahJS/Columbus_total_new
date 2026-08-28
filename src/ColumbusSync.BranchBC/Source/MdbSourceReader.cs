using System;
using System.Collections.Generic;
using System.Data;

namespace ColumbusSync.BranchBC.Source
{
    /// <summary>
    /// B/C지점 TSDB.mdb(Access)에서 계근/거래처/차량/품목 데이터를 읽어오는 소스 리더.
    /// 컬럼명은 첨부받은 TSDB.mdb 스키마(mdb-schema로 직접 확인)를 기준으로 했다 — A지점
    /// MES 저장프로시저처럼 화면 소스를 거쳐 추정한 게 아니라 mdb 파일 스키마 자체를 읽은
    /// 것이라 신뢰도가 높다.
    ///
    /// 읽기 전용이다: TS2020 프로그램이 쓰는 mdb를 이 프로그램이 건드리는 일은 없다.
    /// </summary>
    public class MdbSourceReader
    {
        private readonly string _connectionString;

        public MdbSourceReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>거래처 마스터 전체 조회 (TB_CUSTO).</summary>
        public List<RawCustomerRow> GetCustomers()
        {
            const string sql = "SELECT [CUSTONO], [CUSTONM], [CEO], [DAMDANG], [TEL], [FAX], [ADD1], [BUSSNO], [UPTAE], [JONGMOK], [Rem1] FROM [TB_CUSTO]";

            var table = OleDbHelper.GetDataTable(_connectionString, sql, new OleDbParam[0]);

            var list = new List<RawCustomerRow>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(new RawCustomerRow
                {
                    CustoNo = AsString(row, "CUSTONO"),
                    CustoNm = AsString(row, "CUSTONM"),
                    Ceo = AsString(row, "CEO"),
                    Damdang = AsString(row, "DAMDANG"),
                    Tel = AsString(row, "TEL"),
                    Fax = AsString(row, "FAX"),
                    Addr = AsString(row, "ADD1"),
                    BussNo = AsString(row, "BUSSNO"),
                    Uptae = AsString(row, "UPTAE"),
                    Jongmok = AsString(row, "JONGMOK"),
                    Remark = AsString(row, "Rem1"),
                });
            }

            return list;
        }

        /// <summary>차량 마스터 전체 조회 (TB_CAR).</summary>
        public List<RawVehicleRow> GetVehicles()
        {
            const string sql = "SELECT [CARNO], [CARSONM], [CARTYPE], [DRIVER], [EMPTWT], [REM1] FROM [TB_CAR]";

            var table = OleDbHelper.GetDataTable(_connectionString, sql, new OleDbParam[0]);

            var list = new List<RawVehicleRow>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(new RawVehicleRow
                {
                    CarNo = AsString(row, "CARNO"),
                    CarrierName = AsString(row, "CARSONM"),
                    VehicleType = AsString(row, "CARTYPE"),
                    DriverName = AsString(row, "DRIVER"),
                    TareWeight = AsDecimal(row, "EMPTWT"),
                    Remark = AsString(row, "REM1"),
                });
            }

            return list;
        }

        /// <summary>제품 마스터 전체 조회 (TB_PUM).</summary>
        public List<RawProductRow> GetProducts()
        {
            const string sql = "SELECT [PUMNO], [PUMNM], [UNIT1], [DANKA], [LossWt], [LossPro], [Rem1] FROM [TB_PUM]";

            var table = OleDbHelper.GetDataTable(_connectionString, sql, new OleDbParam[0]);

            var list = new List<RawProductRow>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(new RawProductRow
                {
                    PumNo = AsString(row, "PUMNO"),
                    PumNm = AsString(row, "PUMNM"),
                    Unit = AsString(row, "UNIT1"),
                    UnitPrice = AsDecimal(row, "DANKA"),
                    LossWeight = AsDecimal(row, "LossWt"),
                    LossRate = AsDecimal(row, "LossPro"),
                    Remark = AsString(row, "Rem1"),
                });
            }

            return list;
        }

        /// <summary>
        /// 지정한 기간의 계근 데이터 조회 (TB_WEIGH). DATE1(1차계량일, Text 10)이 "yyyy-mm-dd"
        /// 형식으로 저장되어 있어 문자열 범위 비교로도 날짜순 비교가 정확히 맞는다.
        /// OLE DB는 "?" 자리표시자를 SQL문에 나오는 순서대로 채우므로, 파라미터 배열 순서가
        /// 곧 자리표시자 순서다.
        /// </summary>
        public List<RawWeighRow> GetWeighRecords(DateTime fromDate, DateTime toDate)
        {
            const string sql = @"
SELECT [SENO], [DATE1], [DATE2], [BUNHO], [TIME1], [TIME2], [CARNO], [CUSTONO], [CUSTONM],
       [PUMNO], [PUMNM], [OVTOTWT], [OVNETWT], [LOSSWT], [DANKA], [DAMDANG], [REM1], [WEIGH_STS]
FROM [TB_WEIGH]
WHERE [DATE1] >= ? AND [DATE1] <= ?
ORDER BY [DATE1], [BUNHO]";

            var table = OleDbHelper.GetDataTable(_connectionString, sql, new[]
            {
                new OleDbParam("FromDate", fromDate.ToString("yyyy-MM-dd")),
                new OleDbParam("ToDate", toDate.ToString("yyyy-MM-dd")),
            });

            var list = new List<RawWeighRow>();
            foreach (DataRow row in table.Rows)
            {
                var date1 = AsString(row, "DATE1");
                var date2 = AsString(row, "DATE2");
                var time1 = AsString(row, "TIME1");
                var time2 = AsString(row, "TIME2");

                list.Add(new RawWeighRow
                {
                    Seno = AsString(row, "SENO"),
                    WeighDate = ParseDate(date1) ?? fromDate,
                    Bunho = AsString(row, "BUNHO"),
                    CarNo = AsString(row, "CARNO"),
                    CustoNo = AsString(row, "CUSTONO"),
                    CustoNm = AsString(row, "CUSTONM"),
                    PumNo = AsString(row, "PUMNO"),
                    PumNm = AsString(row, "PUMNM"),
                    FirstDateTime = CombineDateTime(date1, time1),
                    FirstWeight = AsDecimal(row, "OVTOTWT"),
                    SecondDateTime = CombineDateTime(date2, time2),
                    SecondWeight = AsDecimal(row, "OVNETWT"),
                    LossWeight = AsDecimal(row, "LOSSWT"),
                    UnitPrice = AsDecimal(row, "DANKA"),
                    WeigherName = AsString(row, "DAMDANG"),
                    Remark = AsString(row, "REM1"),
                    WeighStatus = AsString(row, "WEIGH_STS"),
                });
            }

            return list;
        }

        private static DateTime? CombineDateTime(string datePart, string timePart)
        {
            var date = ParseDate(datePart);
            if (date == null)
            {
                return null;
            }

            TimeSpan time;
            if (!string.IsNullOrWhiteSpace(timePart) && TimeSpan.TryParse(timePart, out time))
            {
                return date.Value.Date + time;
            }

            return date;
        }

        private static DateTime? ParseDate(string text)
        {
            DateTime value;
            return !string.IsNullOrWhiteSpace(text) && DateTime.TryParse(text, out value) ? value : (DateTime?)null;
        }

        private static string AsString(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value ? row[column].ToString() : null;
        }

        private static decimal? AsDecimal(DataRow row, string column)
        {
            var text = AsString(row, column);
            decimal value;
            return text != null && decimal.TryParse(text, out value) ? value : (decimal?)null;
        }
    }
}
