using System;

namespace ColumbusWeighing.Models
{
    /// <summary>
    /// 로그인 계정 1건(허브 DB dbo.APP_USER). 비밀번호 해시/솔트는 화면에 표시할 필요가 없어
    /// 이 모델에 담지 않는다(SqlUserRepository도 조회 SQL에서부터 아예 선택하지 않는다).
    /// </summary>
    public class UserAccount
    {
        public int Id { get; set; }

        /// <summary>소속 지점 코드('A'/'B'/'C'). NULL이면 지점에 속하지 않는 전사 공용 계정
        /// (예: admin)이다. 유일성은 LOGIN_ID 단독이 아니라 (BranchCode, LoginId) 조합 기준이다.</summary>
        public string BranchCode { get; set; }

        public string LoginId { get; set; }

        public string DisplayName { get; set; }

        public string Phone { get; set; }

        public string Remark { get; set; }

        public bool CanPrint { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public bool IsAdmin { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedAt { get; set; }
    }
}
