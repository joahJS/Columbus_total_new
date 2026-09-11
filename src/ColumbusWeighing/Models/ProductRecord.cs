using System;

namespace ColumbusWeighing.Models
{
    /// <summary>
    /// 통합 허브 DB(dbo.PRODUCT)의 제품 마스터 1건. 이 프로그램은 조회 전용이므로
    /// 추가/수정/삭제 없이 목록 표시에만 쓰인다.
    /// </summary>
    public class ProductRecord
    {
        public int Id { get; set; }

        public string BranchCode { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string Unit { get; set; }

        public decimal? UnitPrice { get; set; }

        public decimal? LossWeight { get; set; }

        public decimal? LossRate { get; set; }

        public string Remark { get; set; }

        public DateTime SyncedAt { get; set; }
    }
}
