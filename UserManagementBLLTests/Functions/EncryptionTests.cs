using Microsoft.VisualStudio.TestTools.UnitTesting;
using UserManagementBLL.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementBLL.Functions.Tests
{
    [TestClass()]
    public class EncryptionTests
    {
        [TestMethod()]
        public void CompareEncryptionTest()
        {
            string password = "131313";

            string encryptedString = Encryption.Encrypt(password);

            string decryptedString = Encryption.Decrypt(encryptedString);

            Assert.AreEqual(password, decryptedString);
        }

        [TestMethod()]
        public void Compare_Diferent_EncryptionTest()
        {
            string passworda = "123456";

            string encryptedaString = Encryption.Encrypt(passworda);

            string passwordb = "654321";

            string encryptedbString = Encryption.Encrypt(passwordb);

            Assert.AreNotEqual(encryptedaString, encryptedbString);
        }

        [TestMethod()]
        public void DecrypTest()
        {
            string encrypted = "eqB8RKRPVewE+8srbICRYw==";
            string decrypted = "131313";

            string DecryptedString = Encryption.Decrypt(encrypted);

            Assert.AreEqual(DecryptedString, decrypted);
        }

        [TestMethod()]
        public void EncrypTest()
        {
            string decrypted = "131313";
            string encrypted = "eqB8RKRPVewE+8srbICRYw==";

            string EncryptedString = Encryption.Encrypt(decrypted);

            Assert.AreEqual(EncryptedString, encrypted);
        }
    }
}