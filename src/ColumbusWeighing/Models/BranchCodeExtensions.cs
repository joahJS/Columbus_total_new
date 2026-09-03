namespace ColumbusWeighing.Models
{
    /// <summary>
    /// 지점 코드(BRANCH_CODE)를 화면에 표시할 이름으로 바꾼다. 통합 허브 DB의 BRANCH 테이블은
    /// "A지점/B지점/C지점"이라는 일반 이름을 쓰지만, 실제 현장에서는 지역명으로 부르므로
    /// 조회 화면에서는 이 이름을 보여준다.
    /// </summary>
    public static class BranchCodeExtensions
    {
        public static string ToDisplayString(this string branchCode)
        {
            switch (branchCode)
            {
                case "A": return "영천";
                case "B": return "생곡";
                case "C": return "녹산";
                default: return branchCode;
            }
        }
    }
}
