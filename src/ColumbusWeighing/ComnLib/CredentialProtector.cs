using System;
using System.Security.Cryptography;
using System.Text;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// "접속정보 기억하기"로 저장하는 비밀번호를 이 PC의 현재 Windows 사용자 안에서만 복호화 가능하도록
    /// DPAPI로 암호화/복호화한다. VisionIns 솔루션은 INI 파일에 비밀번호를 평문으로 저장했지만,
    /// 그 방식은 그대로 재현하지 않고 최소한의 보호를 추가했다.
    /// </summary>
    internal static class CredentialProtector
    {
        public static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(plainText);
            var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        public static string Unprotect(string protectedText)
        {
            if (string.IsNullOrEmpty(protectedText))
            {
                return string.Empty;
            }

            try
            {
                var protectedBytes = Convert.FromBase64String(protectedText);
                var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                return string.Empty;
            }
            catch (CryptographicException)
            {
                return string.Empty;
            }
        }
    }
}
