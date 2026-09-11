using System;
using System.Security.Cryptography;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// 사용자 계정(dbo.APP_USER) 비밀번호를 PBKDF2로 해시/검증한다. .NET Framework 4.6.1의
    /// Rfc2898DeriveBytes는 HMACSHA1 알고리즘만 지원하므로 그대로 사용한다(사내 소규모
    /// 관리용 앱 기준으로는 충분한 수준). 평문 비밀번호는 어디에도 저장하지 않는다.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static void CreateHash(string password, out string hashBase64, out string saltBase64)
        {
            var salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            saltBase64 = Convert.ToBase64String(salt);
            hashBase64 = ComputeHash(password, salt);
        }

        public static bool Verify(string password, string hashBase64, string saltBase64)
        {
            try
            {
                var salt = Convert.FromBase64String(saltBase64);
                var expected = Convert.FromBase64String(hashBase64);
                var actual = Convert.FromBase64String(ComputeHash(password, salt));
                return SlowEquals(expected, actual);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string ComputeHash(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password ?? string.Empty, salt, Iterations))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(HashSize));
            }
        }

        /// <summary>타이밍 공격을 피하기 위해 항상 두 배열 전체를 비교한 뒤 결과를 돌려준다.</summary>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            var diff = (uint)a.Length ^ (uint)b.Length;
            for (var i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }

            return diff == 0;
        }
    }
}
