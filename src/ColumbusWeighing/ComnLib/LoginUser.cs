namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// 현재 로그인한 사용자 정보를 프로그램 전역에서 참조하기 위한 세션 홀더.
    /// VisionIns 솔루션의 LoginUser와 동일한 역할이며, 로그인 성공 시 LoginForm에서 값을 채운다.
    /// </summary>
    public static class LoginUser
    {
        /// <summary>로그인한 계정의 소속 지점 코드('A'/'B'/'C'). 공용 계정이면 null/빈 문자열이다.
        /// B/C지점처럼 서로 다른 지점에 같은 UserId가 있을 수 있어, 같은 계정인지 비교할 때는
        /// UserId 하나만이 아니라 BranchCode까지 같이 봐야 한다.</summary>
        public static string BranchCode;
        public static string UserId;
        public static string UserName;
    }
}
