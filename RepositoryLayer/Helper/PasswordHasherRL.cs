using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Helper
{
    public class PasswordHasherRL
    {

        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;

        public async Task<string> HashPasswordAsync(string password)
        {
            return await Task.Run(() =>
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    byte[] salt = new byte[SaltSize];
                    rng.GetBytes(salt);

                    using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                    {
                        byte[] hash = pbkdf2.GetBytes(KeySize);
                        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
                    }
                }
            });
        }
        public async Task<bool> VerifyPasswordAsync(string inputPassword, string storedHash)
        {
            return await Task.Run(() =>
            {
                var parts = storedHash.Split(':');
                if (parts.Length != 2) return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedPasswordHash = Convert.FromBase64String(parts[1]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(KeySize);
                    return CryptographicOperations.FixedTimeEquals(computedHash, storedPasswordHash);
                }
            });
        }
    }
}
