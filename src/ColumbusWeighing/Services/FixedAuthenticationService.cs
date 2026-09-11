using System;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 실제 로그인은 SqlAuthenticationService(dbo.APP_USER 조회)로 대체되었다. 이 클래스는
    /// MainForm의 VS 디자이너 전용 생성자에서 디자인 타임 로드를 위해서만 남아 있다.
    /// </summary>
    public sealed class FixedAuthenticationService : IAuthenticationService
    {
        private const string FixedUserId = "admin";
        private const string FixedPassword = "1234";
        private const string FixedDisplayName = "관리자";

        public bool TryLogin(string userId, string password, out string displayName)
        {
            var isValid = string.Equals(userId, FixedUserId, StringComparison.Ordinal)
                && string.Equals(password, FixedPassword, StringComparison.Ordinal);

            displayName = isValid ? FixedDisplayName : null;
            return isValid;
        }
    }
}
