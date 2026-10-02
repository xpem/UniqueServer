using System.Security.Cryptography;

namespace UserManagementService.Service
{
    public interface IPasswordHashService
    {
        string Hash(string password);
        bool Verify(string password, string hashedPassword);
    }

    /// <summary>
    /// PBKDF2-HMACSHA256 com formato auto-descritivo "{iterations}.{saltBase64}.{hashBase64}",
    /// permitindo aumentar as iterações no futuro sem invalidar hashes já gravados.
    /// </summary>
    public class PasswordHashService : IPasswordHashService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 210_000; // recomendação OWASP (2023) para PBKDF2-HMAC-SHA256
        private const char Delimiter = '.';

        public string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            return string.Join(Delimiter, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public bool Verify(string password, string hashedPassword)
        {
            string[] parts = hashedPassword.Split(Delimiter);

            if (parts.Length != 3 || !int.TryParse(parts[0], out int iterations))
                return false;

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expectedHash = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
