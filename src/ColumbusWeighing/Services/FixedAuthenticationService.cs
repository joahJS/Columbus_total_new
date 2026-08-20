using System;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// TODO: DB 연동 후 사용자 테이블 조회(및 비밀번호 해시 검증)로 교체할 것.
    /// 그 전까지는 아래 고정 계정으로만 로그인이 허용된다.
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
