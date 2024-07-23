﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

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
                string configName = "JwtKey";

                //https://generate-random.org/encryption-key-generator?count=1&bytes=32&cipher=aes-256-cbc&string=&password=
                string configValue = "iezfxheYc3rduxaqQ+OXQNkbp0MAfZs4jU/8nU+c3isVuvcOdFPV1TzLDIy9X6oe";
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[configName] != null)
                {
                    config.AppSettings.Settings.Remove(configName);
                }
                config.AppSettings.Settings.Add(configName, configValue);
                config.Save(ConfigurationSaveMode.Modified, true);
                ConfigurationManager.RefreshSection("appSettings");

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