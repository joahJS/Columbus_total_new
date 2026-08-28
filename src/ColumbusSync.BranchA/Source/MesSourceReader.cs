using System;
using System.Collections.Generic;
using System.Data;
using ColumbusSync.BranchA.Logging;

namespace ColumbusSync.BranchA.Source
{
    /// <summary>
    /// A지점 MES(Columbus_total)에서 계근/거래처/차량 데이터를 읽어오는 소스 리더.
    ///
    /// 프로시저 라우팅은 Columbus_total 소스코드의 PROCEDURE_ID 상수를 실제로 확인해서 맞췄다
    /// (03SA/SA015F00.cs -> DP_SA015F00, 01CM/CM001F00.cs -> DP_CM001F00,
    ///  01CM/CM009F00.cs -> DP_CM009F00). 처음에는 CV_Retr/CAR_RETR도 DP_SA015F00으로
    /// 잘못 호출해서, 해당 CMD 분기가 없는 프로시저가 "오류 없이 빈 결과"를 돌려주는 바람에
    /// 0건으로 보였다.
    ///
    /// DP_SA015F00/DP_CM001F00/DP_CM009F00 세 프로시저 모두 실제 본문을 받아 컬럼명/필터
    /// 조건을 확인했다(README 참고). 다만 GetProducts()용 품목 프로시저는 아직 미확인 상태다.
    /// </summary>
    public class MesSourceReader
    {
        private const string MeasureProcedureId = "DP_SA015F00";
        private const string CustomerProcedureId = "DP_CM001F00";
        private const string VehicleProcedureId = "DP_CM009F00";
        private readonly string _connectionString;

        public MesSourceReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>거래처 마스터 전체 조회 (DP_CM001F00, CMD=CV_Retr).
        /// 실제 프로시저 본문으로 확인 완료. CVMAST 원본 컬럼명이 짐작과 많이 달랐다
        /// (예: 대표자명은 CEO가 아니라 OWNAM, 담당자명은 DAMDANG이 아니라 조인된 PLNCD_NM,
        /// 전화/팩스는 TEL/FAX가 아니라 TELNO/FAXNO, 사업자번호는 BUSSNO가 아니라 SANO,
        /// 종목은 JONGMOK이 아니라 JONGK, 주소는 이미 CONCAT(ADDR1,ADDR2)된 ADDR로 내려온다).</summary>
        public List<RawCustomerRow> GetCustomers()
        {
            var table = SqlHelper.GetDataTable(_connectionString, CustomerProcedureId, new[]
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
                    Ceo = AsString(row, "OWNAM"),
                    Damdang = AsString(row, "PLNCD_NM"),
                    Tel = AsString(row, "TELNO"),
                    Fax = AsString(row, "FAXNO"),
                    Addr = AsString(row, "ADDR"),
                    BussNo = AsString(row, "SANO"),
                    Uptae = AsString(row, "UPTAE"),
                    Jongmok = AsString(row, "JONGK"),
                    Remark = AsString(row, "RK"),
                });
            }

            return list;
        }

        /// <summary>차량(차량-거래처-품목 조합 템플릿) 전체 조회 (DP_CM009F00, CMD=CAR_RETR).
        /// 실제 프로시저 본문으로 확인 완료. 원본 테이블(CAR_TEMPLATE)은 순수 차량대장이
        /// 아니라 "차량번호+거래처+품목" 조합을 미리 저장해두는 템플릿이라, 운전자명/공차중량
        /// 컬럼이 아예 없다 — 항상 null로 채워진다. VehicleType은 CARML을 매핑했는데,
        /// 실제로 "차종"인지 다른 의미인지는 담당자 확인이 필요하다.</summary>
        public List<RawVehicleRow> GetVehicles()
        {
            var table = SqlHelper.GetDataTable(_connectionString, VehicleProcedureId, new[]
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
                    DriverName = null,   // CAR_TEMPLATE에 없는 컬럼 (B/C지점 mdb에는 있음)
                    TareWeight = null,   // CAR_TEMPLATE에 없는 컬럼 (B/C지점 mdb에는 있음)
                    Remark = AsString(row, "RK"),
                });
            }

            return list;
        }

        /// <summary>
        /// 품목(제품) 마스터 조회. Columbus_total 01CM/CM003~004 폼의 PROCEDURE_ID/CMD 값을
        /// 아직 확인하지 못했다.
        /// </summary>
        public List<object> GetProducts()
        {
            FileLogger.Info("GetProducts()는 아직 미구현 상태입니다. MES 품목관리 화면의 PROCEDURE_ID/CMD 값을 확인한 뒤 구현하세요.");
            return new List<object>();
        }

        /// <summary>
        /// 계근 데이터 조회. DP_SA015F00 실제 본문 기준으로 두 CMD를 모두 호출해서 합친다.
        ///  - FIRST_MEASURE_RETR: 아직 2차 계량 전(SWEIT=0)인 건. 날짜/구분 파라미터가 없다.
        ///  - MEASURE_RETR: 1·2차 모두 완료된(FWEIT&lt;&gt;0 AND SWEIT&lt;&gt;0) 건만 대상이며,
        ///    @COPYN이 'A'/'Y'/'N' 중 하나, @GUBUN이 'A'/'I'/'O' 중 하나가 아니면 WHERE절 전체가
        ///    거짓이 되어 무조건 0건이 나오므로 반드시 채워서 호출해야 한다.
        /// </summary>
        public List<RawWeighRow> GetWeighRecords(DateTime fromDate, DateTime toDate)
        {
            var list = new List<RawWeighRow>();

            var firstTable = SqlHelper.GetDataTable(_connectionString, MeasureProcedureId, new[]
            {
                new SqlParam("CMD", "FIRST_MEASURE_RETR"),
            });
            foreach (DataRow row in firstTable.Rows)
            {
                list.Add(MapWeighRow(row, fromDate));
            }

            var measureTable = SqlHelper.GetDataTable(_connectionString, MeasureProcedureId, new[]
            {
                new SqlParam("CMD", "MEASURE_RETR"),
                new SqlParam("FROM", fromDate.ToString("yyyy-MM-dd")),
                new SqlParam("TO", toDate.ToString("yyyy-MM-dd")),
                new SqlParam("COPYN", "A"),
                new SqlParam("GUBUN", "A"),
                new SqlParam("IDX", "0"),
                new SqlParam("WORD", null),
            });
            foreach (DataRow row in measureTable.Rows)
            {
                list.Add(MapWeighRow(row, fromDate));
            }

            return list;
        }

        private static RawWeighRow MapWeighRow(DataRow row, DateTime fallbackDate)
        {
            return new RawWeighRow
            {
                Slino = AsString(row, "SLINO"),
                TDate = AsDateTime(row, "TDATE") ?? fallbackDate,
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
                // 실제 SELECT에서 LOSGU가 (디코딩값, M.LOSGU 원본값) 순서로 두 번 나와서
                // ADO.NET이 두 번째 컬럼명을 LOSGU1로 자동 변경한다. 원본 코드가 필요하면 LOSGU1을 읽어야 한다.
                LosGu = AsString(row, "LOSGU1") ?? AsString(row, "LOSGU"),
                AWeit = AsDecimal(row, "AWEIT"),
                UCost = AsDecimal(row, "UCOST"),
                ChkYn = AsString(row, "CHKYN"),
                PlnNm = AsString(row, "PLNNM"),
                // INSRK는 검수(inspector) 비고, RK는 일반 비고. 통합 스키마의 REMARK는 RK를 사용한다.
                InsRk = AsString(row, "RK"),
            };
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
