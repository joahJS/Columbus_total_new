namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 로그인 인증 서비스 추상화. 실제 운영은 dbo.APP_USER를 조회하는 SqlAuthenticationService를
    /// 쓰고, VS 디자이너 전용 생성자에서만 고정 계정(FixedAuthenticationService)을 쓴다.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>지점 코드(BranchCode, 공용 계정이면 null/빈 문자열)와 아이디/비밀번호가
        /// 유효하면 true 를 반환하고 화면에 표시할 사용자명을 돌려준다. B/C지점처럼 서로 다른
        /// 지점에 같은 아이디가 있을 수 있어 지점을 함께 확인해야 한다.</summary>
        bool TryLogin(string branchCode, string userId, string password, out string displayName);
    }
}
