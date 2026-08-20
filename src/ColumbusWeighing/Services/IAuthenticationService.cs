namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 로그인 인증 서비스 추상화. 지금은 고정 계정으로 검증하는 구현체를 쓰지만,
    /// DB(사용자/권한 테이블) 연동 시 이 인터페이스만 구현한 새 서비스로 교체하면 된다.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>아이디/비밀번호가 유효하면 true 를 반환하고 화면에 표시할 사용자명을 돌려준다.</summary>
        bool TryLogin(string userId, string password, out string displayName);
    }
}
