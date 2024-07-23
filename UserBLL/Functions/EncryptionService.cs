using System.Security.Cryptography;
using System.Text;

namespace UserManagementBLL.Functions
{
    public interface IEncryptionService
    {
        string Decrypt(string cipherText);
        string Encrypt(string plainText);
    }

    public class EncryptionService(string Key32, string IV16) : IEncryptionService
    {
        // Chave e vetor de inicialização (IV) devem ter 32 bytes e 16 bytes respectivamente
        private readonly byte[] Key = Encoding.UTF8.GetBytes(Key32);
        private readonly byte[] IV = Encoding.UTF8.GetBytes(IV16);

        public string Encrypt(string plainText)
        {
            string encrypted = "";

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msEncrypt = new();
                using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                encrypted = Convert.ToBase64String(msEncrypt.ToArray());
            }

            return encrypted;
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using Aes aesAlg = Aes.Create();
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msDecrypt = new(cipherBytes);
                using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new(csDecrypt);
                string plainText = srDecrypt.ReadToEnd();
                return plainText;
            }
            catch (Exception /*ex*/) { throw; }
        }

        //public string Encrypt(string password)
        //{
        //    byte[] plainTextBytes = Encoding.UTF8.GetBytes(password);
        //    byte[] keyBytes = new Rfc2898DeriveBytes(PrivateKeys.EncryptionKeys.PASSWORDHASH, Encoding.ASCII.GetBytes(PrivateKeys.EncryptionKeys.SALTKEY)).GetBytes(256 / 8);
        //    byte[] cipherTextBytes;

        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Mode = CipherMode.CBC;
        //        aes.Padding = PaddingMode.Zeros;

        //        ICryptoTransform encryptor = aes.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(PrivateKeys.EncryptionKeys.VIKEY));

        //        using MemoryStream memoryStream = new();
        //        using (CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write))
        //        {
        //            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
        //            cryptoStream.FlushFinalBlock();
        //            cipherTextBytes = memoryStream.ToArray();
        //            cryptoStream.Close();
        //        }
        //        memoryStream.Close();
        //    }
        //    return Convert.ToBase64String(cipherTextBytes);
        //}

        //public string Decrypt(string senha)
        //{
        //    byte[] cipherTextBytes = Convert.FromBase64String(senha);
        //    byte[] keyBytes = new Rfc2898DeriveBytes(PrivateKeys.EncryptionKeys.PASSWORDHASH, Encoding.ASCII.GetBytes(PrivateKeys.EncryptionKeys.SALTKEY)).GetBytes(256 / 8);

        //    using Aes aes = Aes.Create();
        //    aes.Mode = CipherMode.CBC;
        //    aes.Padding = PaddingMode.Zeros;

        //    ICryptoTransform decryptor = aes.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(PrivateKeys.EncryptionKeys.VIKEY));

        //    using MemoryStream memoryStream = new(cipherTextBytes);
        //    using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        //    byte[] plainTextBytes = new byte[cipherTextBytes.Length];

        //    int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
        //    memoryStream.Close();
        //    cryptoStream.Close();

        //    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        //}
    }
}
