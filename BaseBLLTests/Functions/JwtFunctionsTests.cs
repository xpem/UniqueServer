using Microsoft.VisualStudio.TestTools.UnitTesting;
using BaseBLL.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBLL.Functions.Tests
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

                var jwt = JwtFunctions.GenerateToken(uid, name, DateTime.UtcNow.AddDays(5));

                var validatedUid = JwtFunctions.GetUidFromToken(jwt);

                Assert.AreEqual(uid, validatedUid);
            }catch(Exception ex) { throw ex; }
        }
    }
}