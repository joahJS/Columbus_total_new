using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

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

        /// <summary>TS2020 로그인 계정 전체 조회 (TB_USER). B/C지점은 같은 프로그램을 쓰기 때문에
        /// "1", "admin" 같은 단순한 ID가 두 지점 mdb에 똑같이 존재할 수 있다 — 지점별로 유일한
        /// 계정이 되도록, 이 값을 허브에 올릴 때는 반드시 지점 코드를 함께 키로 써야 한다
        /// (HubWriter.UpsertUser 참고).</summary>
        public List<RawUserRow> GetUsers()
        {
            const string sql = "SELECT [ID1], [USER1], [PASS1], [TEL], [REM1], [C_PRINT], [C_MODIFY], [C_DELETE], [C_ADMINISTRATOR] FROM [TB_USER]";

            var table = OleDbHelper.GetDataTable(_connectionString, sql, new OleDbParam[0]);

            var list = new List<RawUserRow>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(new RawUserRow
                {
                    LoginId = AsString(row, "ID1"),
                    DisplayName = AsString(row, "USER1"),
                    Password = AsString(row, "PASS1"),
                    Phone = AsString(row, "TEL"),
                    Remark = AsString(row, "REM1"),
                    CanPrint = AsBoolean(row, "C_PRINT"),
                    CanEdit = AsBoolean(row, "C_MODIFY"),
                    CanDelete = AsBoolean(row, "C_DELETE"),
                    IsAdmin = AsBoolean(row, "C_ADMINISTRATOR"),
                });
            }

            return list;
        }

        private const string WeighColumns =
            "[SENO], [DATE1], [DATE2], [BUNHO], [TIME1], [TIME2], [CARNO], [CUSTONO], [CUSTONM], " +
            "[PUMNO], [PUMNM], [ONEWT], [TWOWT], [LOSSWT], [DANKA], [DAMDANG], [REM1], [WEIGH_STS], [IMGFILE], [WORK_DATE]";

        /// <summary>
        /// 지정한 기간의 계근 데이터 조회 (TB_WEIGH). DATE1(1차계량일, Text 10)이 "yyyy-mm-dd"
        /// 형식으로 저장되어 있어 문자열 범위 비교로도 날짜순 비교가 정확히 맞는다.
        /// OLE DB는 "?" 자리표시자를 SQL문에 나오는 순서대로 채우므로, 파라미터 배열 순서가
        /// 곧 자리표시자 순서다.
        ///
        /// 최초 백필(워터마크가 아직 없을 때)에만 쓴다 - 이후의 매 주기 동기화는
        /// GetWeighRecordsModifiedSince()로 WORK_DATE 기준 증분 조회를 한다
        /// (SyncOrchestrator 참고). 일단 받은 뒤에는 수정 여부와 상관없이 전부 돌려준다.
        /// </summary>
        public List<RawWeighRow> GetWeighRecords(DateTime fromDate, DateTime toDate)
        {
            var sql = string.Format(@"
SELECT {0}
FROM [TB_WEIGH]
WHERE [DATE1] >= ? AND [DATE1] <= ?
ORDER BY [DATE1], [BUNHO]", WeighColumns);

            var table = OleDbHelper.GetDataTable(_connectionString, sql, new[]
            {
                new OleDbParam("FromDate", fromDate.ToString("yyyy-MM-dd")),
                new OleDbParam("ToDate", toDate.ToString("yyyy-MM-dd")),
            });

            return MapWeighRows(table, fromDate);
        }

        /// <summary>
        /// workDateWatermark(직전 동기화에서 본 가장 최신 WORK_DATE)보다 WORK_DATE가 더
        /// 큰(=그 이후에 저장/수정된) 행만 조회한다. DATE1(계량일)과 무관하게 걸리므로,
        /// 1년도 더 지난 오래된 계근건이 뒤늦게 수정되는 경우도 놓치지 않는다 - 매 주기
        /// 전체 기간을 다시 훑지 않고도 "수정된 내역이 있는 데이터만" 수집하기 위한 핵심
        /// 메서드다.
        /// </summary>
        public List<RawWeighRow> GetWeighRecordsModifiedSince(string workDateWatermark)
        {
            var sql = string.Format(@"
SELECT {0}
FROM [TB_WEIGH]
WHERE [WORK_DATE] > ?
ORDER BY [WORK_DATE]", WeighColumns);

            var table = OleDbHelper.GetDataTable(_connectionString, sql, new[]
            {
                new OleDbParam("WorkDateWatermark", workDateWatermark),
            });

            return MapWeighRows(table, DateTime.Today);
        }

        /// <summary>이 mdb에 TB_IMAGE 테이블이 있는지 확인한다. TS2020은 버전/설정에 따라
        /// 계근 사진을 로컬 IMAGE 폴더의 파일로 저장하기도 하고(B지점처럼), mdb 안의
        /// TB_IMAGE 테이블에 바이너리로 직접 저장하기도 한다(녹산/C지점에서 발견됨) - 같은
        /// 실행파일(TS2020.exe)에 두 방식이 다 들어있고, 실제로 어느 쪽을 쓰는지는 mdb
        /// 스키마를 보기 전에는 알 수 없다. B/C지점이 같은 프로그램(ColumbusSync.BranchBC)을
        /// 쓰기 때문에, 이 메서드로 지점마다 자동으로 알맞은 방식을 골라 쓴다(SyncOrchestrator
        /// 참고) - 설정을 따로 둘 필요가 없다.</summary>
        public bool HasImageTable()
        {
            try
            {
                OleDbHelper.GetDataTable(_connectionString, "SELECT TOP 1 [IMGFILE] FROM [TB_IMAGE]", new OleDbParam[0]);
                return true;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is OleDbException)
                {
                    // TB_IMAGE 테이블 자체가 없는 mdb(로컬 파일 방식인 경우) - 정상적인 경우다.
                    return false;
                }

                throw;
            }
        }

        /// <summary>TB_IMAGE에서 IMGFILE 하나에 연결된 1차/2차 사진(각 최대 4장)을 모두
        /// 읽어온다. 이 테이블은 IMAGE1_1~IMAGE1_4(1차), IMAGE2_1~IMAGE2_4(2차) 컬럼에
        /// 사진 바이너리를 직접 담고 있고, 1차/2차가 IMGFILE 하나로 같은 행에 묶여 있어서
        /// 로컬 파일 방식(PhotoFileReader)처럼 2차 사진을 촬영 시각으로 추정할 필요가 없다 -
        /// IMGFILE로 조회만 하면 둘 다 정확하게 나온다.</summary>
        public List<WeighPhoto> GetImagesByImgFile(string imgFile)
        {
            var result = new List<WeighPhoto>();
            if (string.IsNullOrEmpty(imgFile))
            {
                return result;
            }

            const string sql = "SELECT [IMAGE1_1],[IMAGE1_2],[IMAGE1_3],[IMAGE1_4],[IMAGE2_1],[IMAGE2_2],[IMAGE2_3],[IMAGE2_4] FROM [TB_IMAGE] WHERE [IMGFILE] = ?";
            var table = OleDbHelper.GetDataTable(_connectionString, sql, new[] { new OleDbParam("ImgFile", imgFile) });
            if (table.Rows.Count == 0)
            {
                return result;
            }

            var row = table.Rows[0];
            AddImageIfPresent(result, row, "IMAGE1_1", "1", 1);
            AddImageIfPresent(result, row, "IMAGE1_2", "1", 2);
            AddImageIfPresent(result, row, "IMAGE1_3", "1", 3);
            AddImageIfPresent(result, row, "IMAGE1_4", "1", 4);
            AddImageIfPresent(result, row, "IMAGE2_1", "2", 1);
            AddImageIfPresent(result, row, "IMAGE2_2", "2", 2);
            AddImageIfPresent(result, row, "IMAGE2_3", "2", 3);
            AddImageIfPresent(result, row, "IMAGE2_4", "2", 4);
            return result;
        }

        private static void AddImageIfPresent(List<WeighPhoto> result, DataRow row, string column, string stage, int seq)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
            {
                return;
            }

            var bytes = row[column] as byte[];
            if (bytes != null && bytes.Length > 0)
            {
                result.Add(new WeighPhoto { Stage = stage, Seq = seq, Data = bytes });
            }
        }

        private static List<RawWeighRow> MapWeighRows(DataTable table, DateTime fallbackDate)
        {
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
                    WeighDate = ParseDate(date1) ?? fallbackDate,
                    Bunho = AsString(row, "BUNHO"),
                    CarNo = AsString(row, "CARNO"),
                    CustoNo = AsString(row, "CUSTONO"),
                    CustoNm = AsString(row, "CUSTONM"),
                    PumNo = AsString(row, "PUMNO"),
                    PumNm = AsString(row, "PUMNM"),
                    FirstDateTime = CombineDateTime(date1, time1),
                    FirstWeight = AsDecimal(row, "ONEWT"),
                    SecondDateTime = CombineDateTime(date2, time2),
                    SecondWeight = AsDecimal(row, "TWOWT"),
                    LossWeight = AsDecimal(row, "LOSSWT"),
                    UnitPrice = AsDecimal(row, "DANKA"),
                    WeigherName = AsString(row, "DAMDANG"),
                    Remark = AsString(row, "REM1"),
                    WeighStatus = AsString(row, "WEIGH_STS"),
                    ImgFile = AsString(row, "IMGFILE"),
                    WorkDate = AsString(row, "WORK_DATE"),
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

        private static bool AsBoolean(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value && Convert.ToBoolean(row[column]);
        }
    }
}
