using System;

namespace ColumbusSync.BranchA.Source
{
    /// <summary>MES(Columbus_total) DP_CM001F00 'CV_Retr' 조회 결과 1행.
    /// DTO 필드명은 통합 허브 스키마 쪽 의미를 따르고, 실제 CVMAST 원본 컬럼명과는 다르다
    /// (예: Ceo는 실제로 OWNAM 컬럼에서 옴). 매핑은 MesSourceReader.GetCustomers() 참고.</summary>
    public class RawCustomerRow
    {
        public string CvCod { get; set; }
        public string CvNam { get; set; }
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

    /// <summary>MES DP_CM009F00 'CAR_RETR' 조회 결과 1행.
    /// 실제 원본(CAR_TEMPLATE)은 "차량번호+거래처+품목 조합 템플릿"에 가까워서 운전자명/공차중량
    /// 컬럼이 아예 없다. DriverName/TareWeight는 통합 허브 VEHICLE 테이블에는 있지만(B/C지점 mdb
    /// TB_CAR에는 실제로 있는 컬럼), A지점에서는 항상 null로 채워진다.</summary>
    public class RawVehicleRow
    {
        public string CarNo { get; set; }
        public string CarrierName { get; set; }
        public string VehicleType { get; set; }
        public string DriverName { get; set; }
        public decimal? TareWeight { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>MES SA015F00 화면이 사용하는 프로시저 DP_SA015F00 'MEASURE_RETR' 조회 결과 1행.
    /// 컬럼명은 SA015F00.cs의 SetData()/삭제 로직에서 실제 바인딩되는 컬럼을 그대로 따랐다.</summary>
    public class RawWeighRow
    {
        public string Slino { get; set; }
        public DateTime TDate { get; set; }
        public string SeqNo { get; set; }
        public string JobGu { get; set; }          // 'I' / 'O'
        public string CarNo { get; set; }
        public string CvCod { get; set; }
        public string CvNam { get; set; }
        public string ItCod { get; set; }
        public string ItNam { get; set; }
        public DateTime? FTime { get; set; }
        public DateTime? STime { get; set; }
        public decimal? FWeit { get; set; }        // 1차중량
        public decimal? SWeit { get; set; }        // 2차중량
        public decimal? EWeit { get; set; }        // 공차중량
        public decimal? RWeit { get; set; }        // 실중량
        public decimal? LWeit { get; set; }        // 감량중량
        public string LosGu { get; set; }          // 감량사유
        public decimal? AWeit { get; set; }        // 순중량
        public decimal? UCost { get; set; }        // 단가
        public string ChkYn { get; set; }          // 검수여부(원본 상태값, 참고용으로만 보존)
        public string PlnNm { get; set; }          // 담당자명
        public string InsRk { get; set; }          // 비고
    }
}
