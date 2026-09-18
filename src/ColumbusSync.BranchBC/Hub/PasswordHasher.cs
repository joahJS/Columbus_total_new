using System;
using System.Security.Cryptography;

namespace ColumbusSync.BranchBC.Hub
{
    /// <summary>
    /// ColumbusWeighing.ComnLib.PasswordHasher와 완전히 동일한 알고리즘(PBKDF2/HMACSHA1,
    /// 16바이트 솔트, 32바이트 해시, 10000회 반복)을 그대로 복사한 것이다. 이 프로젝트는
    /// ColumbusWeighing을 참조하지 않는(README의 "독립성" 원칙) 완전히 독립된 콘솔 앱이라
    /// 클래스를 공유할 수 없어 부득이하게 중복 구현했다 — 두 파일 중 하나를 고치면 반드시
    /// 다른 하나도 같이 고쳐야 한다.
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

        private static string ComputeHash(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password ?? string.Empty, salt, Iterations))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(HashSize));
            }
        }
    }
}
