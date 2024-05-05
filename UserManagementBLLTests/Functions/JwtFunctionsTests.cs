using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UserManagementBLL.Functions.Tests
{
    [TestClass()]
    public class JwtFunctionsTests
    {
        [TestMethod()]
        public void GenerateValidateTokenTest()
        {
            try
            {
                int uid = 10;
                string name = "emanuel";

                string jwt = JwtFunctions.GenerateToken(uid, name, DateTime.UtcNow.AddDays(5));

                int? validatedUid = JwtFunctions.GetUidFromToken(jwt);

                Assert.AreEqual(uid, validatedUid);
            }
            catch (Exception ex) { throw ex; }
        }
    }
}