using System;

namespace ColumbusSync.BranchBC.Source
{
    /// <summary>TSDB.mdb TB_CUSTO 조회 결과 1행.</summary>
    public class RawCustomerRow
    {
        public string CustoNo { get; set; }
        public string CustoNm { get; set; }
        public string Ceo { get; set; }
        public string Damdang { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Addr { get; set; }
        public string BussNo { get; set; }
        public string Uptae { get; set; }
        public string Jongmok { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>TSDB.mdb TB_CAR 조회 결과 1행. A지점(MES)과 달리 운전자명/공차중량이
    /// 실제로 존재하는 컬럼이다.</summary>
    public class RawVehicleRow
    {
        public string CarNo { get; set; }
        public string CarrierName { get; set; }
        public string VehicleType { get; set; }
        public string DriverName { get; set; }
        public decimal? TareWeight { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>TSDB.mdb TB_PUM 조회 결과 1행. A지점(MES)과 달리 감량중량/감량율이
    /// 실제로 존재하는 컬럼이다.</summary>
    public class RawProductRow
    {
        public string PumNo { get; set; }
        public string PumNm { get; set; }
        public string Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? LossWeight { get; set; }
        public decimal? LossRate { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>TSDB.mdb TB_WEIGH 조회 결과 1행.</summary>
    public class RawWeighRow
    {
        public string Seno { get; set; }
        public DateTime WeighDate { get; set; }      // DATE1 (1차계량일)
        public string Bunho { get; set; }             // 계량순번
        public string CarNo { get; set; }
        public string CustoNo { get; set; }
        public string CustoNm { get; set; }
        public string PumNo { get; set; }
        public string PumNm { get; set; }
        public DateTime? FirstDateTime { get; set; }   // DATE1 + TIME1
        public decimal? FirstWeight { get; set; }       // OVTOTWT
        public DateTime? SecondDateTime { get; set; }   // DATE2 + TIME2
        public decimal? SecondWeight { get; set; }      // OVNETWT
        public decimal? LossWeight { get; set; }        // LOSSWT
        public decimal? UnitPrice { get; set; }          // DANKA
        public string WeigherName { get; set; }          // DAMDANG
        public string Remark { get; set; }                // REM1
        public string WeighStatus { get; set; }            // WEIGH_STS (원본 상태값, 참고용으로만 보존)
    }
}
