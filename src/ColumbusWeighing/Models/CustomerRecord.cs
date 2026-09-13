using System;

namespace ColumbusWeighing.Models
{
    /// <summary>
    /// 통합 허브 DB(dbo.CUSTOMER)의 거래처 마스터 1건. 이 프로그램은 조회 전용이므로
    /// 추가/수정/삭제 없이 목록 표시에만 쓰인다.
    /// </summary>
    public class CustomerRecord
    {
        public int Id { get; set; }

        public string BranchCode { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string CeoName { get; set; }

        public string ManagerName { get; set; }

        public string Tel { get; set; }

        public string Fax { get; set; }

        public string Remark { get; set; }

        public DateTime SyncedAt { get; set; }
    }
}
