namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// 현재 로그인한 사용자 정보를 프로그램 전역에서 참조하기 위한 세션 홀더.
    /// VisionIns 솔루션의 LoginUser와 동일한 역할이며, 로그인 성공 시 LoginForm에서 값을 채운다.
    /// </summary>
    public static class LoginUser
    {
        public static string UserId;
        public static string UserName;
    }
}
