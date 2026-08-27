using System;
using System.Collections.Generic;
using System.Data;
using ColumbusSync.BranchA.Logging;

namespace ColumbusSync.BranchA.Source
{
    /// <summary>
    /// A지점 MES(Columbus_total)에서 계근/거래처/차량 데이터를 읽어오는 소스 리더.
    /// Columbus_total의 03SA/SA015F00.cs, 01CM/CM001F00.cs, 01CM/CM009F00.cs 에서 실제로
    /// 호출하는 저장프로시저/CMD 값을 그대로 사용한다.
    ///
    /// 주의: 여기 있는 CMD 값과 컬럼명은 화면 코드를 근거로 확인한 것이며, 실제 저장프로시저의
    /// 정확한 반환 컬럼 목록은 DBA/MES 담당자에게 한 번 더 확인 후 맞춰야 한다(스켈레톤 단계).
    /// </summary>
    public class MesSourceReader
    {
        private const string MeasureProcedureId = "DP_SA015F00";
        private readonly string _connectionString;

        public MesSourceReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>거래처 마스터 전체 조회 (CM001F00.cs: CMD=CV_Retr).</summary>
        public List<RawCustomerRow> GetCustomers()
        {
            var table = SqlHelper.GetDataTable(_connectionString, MeasureProcedureId, new[]
            {
                new SqlParam("CMD", "CV_Retr"),
            });

            var list = new List<RawCustomerRow>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(new RawCustomerRow
                {
                    CvCod = AsString(row, "CVCOD"),
                    CvNam = AsString(row, "CVNAM"),
                    Ceo = AsString(row, "CEO"),
                    Damdang = AsString(row, "DAMDANG"),
                    Tel = AsString(row, "TEL"),
                    Fax = AsString(row, "FAX"),
                    Addr = AsString(row, "ADD1"),
                    BussNo = AsString(row, "BUSSNO"),
                    Uptae = AsString(row, "UPTAE"),
                    Jongmok = AsString(row, "JONGMOK"),
                    Remark = AsString(row, "RK"),
                });
            }

            return list;
        }

        /// <summary>차량 마스터 전체 조회 (CM009F00.cs: CMD=CAR_RETR).</summary>
        public List<RawVehicleRow> GetVehicles()
        {
            var table = SqlHelper.GetDataTable(_connectionString, MeasureProcedureId, new[]
            {
                new SqlParam("CMD", "CAR_RETR"),
            });

            var list = new List<RawVehicleRow>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(new RawVehicleRow
                {
                    CarNo = AsString(row, "CARNO"),
                    CarrierName = AsString(row, "CVNAM"),
                    VehicleType = AsString(row, "CARML"),
                    DriverName = AsString(row, "DRIVER"),
                    TareWeight = AsDecimal(row, "EMPTWT"),
                    Remark = AsString(row, "RK"),
                });
            }

            return list;
        }

        /// <summary>
        /// 품목(제품) 마스터 조회. Columbus_total 01CM/CM003~004 폼에서 정확한 CMD 이름을
        /// 아직 확인하지 못했다 (SetData 등 값 바인딩 흔적으로 화면 존재만 확인됨).
        /// 실제 연동 전, MES 담당자에게 CM003F00/CM004F00이 호출하는 CMD 값을 확인해서
        /// 이 메서드를 채워 넣어야 한다.
        /// </summary>
        public List<object> GetProducts()
        {
            FileLogger.Info("GetProducts()는 아직 미구현 상태입니다. MES 품목관리 화면의 CMD 값을 확인한 뒤 구현하세요.");
            return new List<object>();
        }

        /// <summary>
        /// 지정한 날짜의 계근 데이터 전체 조회.
        /// SA015F00.cs의 RETR()에서 쓰는 CMD=MEASURE_RETR과 동일한 파라미터 구성을 따른다.
        /// (FIRST_MEASURE_RETR은 "아직 2차 계량 전" 건만 보는 별도 조회이며, 여기서는
        /// 1차/2차 완료 여부를 통합 스키마 쪽에서 재계산하므로 MEASURE_RETR 하나로 충분하다.)
        /// </summary>
        public List<RawWeighRow> GetWeighRecords(DateTime fromDate, DateTime toDate)
        {
            var table = SqlHelper.GetDataTable(_connectionString, MeasureProcedureId, new[]
            {
                new SqlParam("CMD", "MEASURE_RETR"),
                new SqlParam("FROM", fromDate.ToString("yyyy-MM-dd")),
                new SqlParam("TO", toDate.ToString("yyyy-MM-dd")),
                new SqlParam("COPYN", null),
                new SqlParam("GUBUN", null),
                new SqlParam("IDX", "0"),
                new SqlParam("WORD", null),
            });

            var list = new List<RawWeighRow>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(new RawWeighRow
                {
                    Slino = AsString(row, "SLINO"),
                    TDate = AsDateTime(row, "TDATE") ?? fromDate,
                    SeqNo = AsString(row, "SEQNO"),
                    JobGu = AsString(row, "JOBGU"),
                    CarNo = AsString(row, "CARNO"),
                    CvCod = AsString(row, "CVCOD"),
                    CvNam = AsString(row, "CVNAM"),
                    ItCod = AsString(row, "ITCOD"),
                    ItNam = AsString(row, "ITNAM"),
                    FTime = AsDateTime(row, "FTIME"),
                    STime = AsDateTime(row, "STIME"),
                    FWeit = AsDecimal(row, "FWEIT"),
                    SWeit = AsDecimal(row, "SWEIT"),
                    EWeit = AsDecimal(row, "EWEIT"),
                    RWeit = AsDecimal(row, "RWEIT"),
                    LWeit = AsDecimal(row, "LWEIT"),
                    LosGu = AsString(row, "LOSGU"),
                    AWeit = AsDecimal(row, "AWEIT"),
                    UCost = AsDecimal(row, "UCOST"),
                    ChkYn = AsString(row, "CHKYN"),
                    PlnNm = AsString(row, "PLNNM"),
                    InsRk = AsString(row, "INSRK"),
                });
            }

            return list;
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

        private static DateTime? AsDateTime(DataRow row, string column)
        {
            var text = AsString(row, column);
            DateTime value;
            return text != null && DateTime.TryParse(text, out value) ? value : (DateTime?)null;
        }
    }
}
