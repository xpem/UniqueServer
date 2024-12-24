using Microsoft.VisualStudio.TestTools.UnitTesting;
using UserManagementService.Functions;

namespace UserManagementRepoTests.Functions
{
    [TestClass()]
    public class EncryptionTests
    {
        public static EncryptionService BuildEncryptionService()
        {
            string key32 = "qJEJjz9rat0JyGpbtsA9Wc7zZE4yk9cX";
            string IV16 = "XtZqKFa6OQyPKRGf";

            return new EncryptionService(key32, IV16);
        }

        [TestMethod()]
        public void CompareEncryptionTest()
        {
            string password = "131313";

            var encryptionService = BuildEncryptionService();

            string encryptedString = encryptionService.Encrypt(password);

            string decryptedString = encryptionService.Decrypt(encryptedString);

            Assert.AreEqual(password, decryptedString);
        }

        [TestMethod()]
        public void Compare_Diferent_EncryptionTest()
        {
            string passworda = "123456";

            var encryptionService = BuildEncryptionService();

            string encryptedaString = encryptionService.Encrypt(passworda);

            string passwordb = "654321";

            string encryptedbString = encryptionService.Encrypt(passwordb);

            Assert.AreNotEqual(encryptedaString, encryptedbString);
        }

        [TestMethod()]
        public void DecrypTest()
        {
            string encrypted = "pQhhuP6H8BAZCjFYVfHalA==";
            string decrypted = "131313";
            var encryptionService = BuildEncryptionService();
            string DecryptedString = encryptionService.Decrypt(encrypted);

            Assert.AreEqual(DecryptedString, decrypted);
        }

        [TestMethod()]
        public void EncrypTest()
        {
            string decrypted = "131313";
            string encrypted = "pQhhuP6H8BAZCjFYVfHalA==";
            var encryptionService = BuildEncryptionService();
            string EncryptedString = encryptionService.Encrypt(decrypted);

            Assert.AreEqual(EncryptedString, encrypted);
        }
    }
}