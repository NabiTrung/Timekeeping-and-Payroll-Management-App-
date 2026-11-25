
using System;
using System.Security.Cryptography;

namespace QuanLyChamCong
{
    public class PasswordHelper
    {
        public static string BamMatKhau(string matKhau)
        {
            return BCrypt.Net.BCrypt.HashPassword(
                matKhau,
                workFactor: 12
            );
        }

        public static bool KiemTraMatKhau(
            string matKhau,
            string matKhauDaBam)
        {
            if (string.IsNullOrEmpty(matKhau) ||
                string.IsNullOrEmpty(matKhauDaBam))
            {
                return false;
            }

            if (LaHashBCrypt(matKhauDaBam))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(
                        matKhau,
                        matKhauDaBam
                    );
                }
                catch
                {
                    return false;
                }
            }
            try
            {
                string[] parts = matKhauDaBam.Split(':');

                if (parts.Length != 2)
                    return false;

                byte[] salt =
                    Convert.FromBase64String(parts[0]);

                byte[] hashCu =
                    Convert.FromBase64String(parts[1]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    matKhau,
                    salt,
                    100000,
                    HashAlgorithmName.SHA256))
                {
                    byte[] hashMoi = pbkdf2.GetBytes(32);

                    return CryptographicOperations.FixedTimeEquals(
                        hashCu,
                        hashMoi
                    );
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool LaHashBCrypt(string hash)
        {
            return hash.StartsWith("$2a$") ||
                   hash.StartsWith("$2b$") ||
                   hash.StartsWith("$2y$");
        }

        public static bool CanNangCap(string hash)
        {
            return !LaHashBCrypt(hash);
        }
    }
}
