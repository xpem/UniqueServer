using BaseModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaseBLL.Functions.Tests
{
    [TestClass()]
    public class EncryptionTests
    {
        [TestMethod()]
        public void CompareEncryptionTest()
        {
            string password = "131313";

            var encryptedString = Encryption.Encrypt(password);

            var decryptedString = Encryption.Decrypt(encryptedString);

            Assert.AreEqual(password, decryptedString);
        }

        [TestMethod()]
        public void Compare_Diferent_EncryptionTest()
        {
            string passworda = "123456";

            var encryptedaString = Encryption.Encrypt(passworda);

            string passwordb = "654321";

            var encryptedbString = Encryption.Encrypt(passwordb);

            Assert.AreNotEqual(encryptedaString, encryptedbString);
        }

        [TestMethod()]
        public void DecrypTest()
        {
            string encrypted = "eqB8RKRPVewE+8srbICRYw==";
            string decrypted = "131313";

            var DecryptedString = Encryption.Decrypt(encrypted);

            Assert.AreEqual(DecryptedString, decrypted);
        }

        [TestMethod()]
        public void EncrypTest()
        {
            string decrypted = "131313";
            string encrypted = "eqB8RKRPVewE+8srbICRYw==";

            var EncryptedString = Encryption.Encrypt(decrypted);

            Assert.AreEqual(EncryptedString, encrypted);
        }
    }
}