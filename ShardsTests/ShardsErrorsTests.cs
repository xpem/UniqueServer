using System.Text.Json;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Model.Res;

namespace ShardsTests
{
    [TestClass]
    public class ShardsErrorsTests
    {
        [TestMethod]
        public void EveryCode_HasMessageAndStatus()
        {
            foreach (ShardsErrorCode code in Enum.GetValues<ShardsErrorCode>())
            {
                string message = ShardsErrors.DefaultMessage(code);

                Assert.IsFalse(string.IsNullOrWhiteSpace(message), $"{code} sem mensagem");
                Assert.AreNotEqual("Erro de regra do jogo.", message, $"{code} caiu na mensagem genérica");
            }
        }

        [TestMethod]
        public void StatusCode_ConcurrencyIs409_RulesAre400()
        {
            Assert.AreEqual(409, ShardsErrors.StatusCodeOf(ShardsErrorCode.ConcurrencyConflict));

            foreach (ShardsErrorCode code in Enum.GetValues<ShardsErrorCode>().Where(c => c != ShardsErrorCode.ConcurrencyConflict))
                Assert.AreEqual(400, ShardsErrors.StatusCodeOf(code), code.ToString());
        }

        [TestMethod]
        public void Exception_BuildsCodeMessageResponse()
        {
            var ex = new ShardsException(ShardsErrorCode.NotEnoughEnergy);

            var res = ex.ToResponse();

            Assert.AreEqual("NotEnoughEnergy", res.Code);
            Assert.AreEqual("Energia insuficiente.", res.Message);
        }

        [TestMethod]
        public void Exception_CustomMessage_OverridesDefault()
        {
            var ex = new ShardsException(ShardsErrorCode.InvalidTimes, "times deve ficar entre 1 e 100.");

            Assert.AreEqual("times deve ficar entre 1 e 100.", ex.ToResponse().Message);
        }

        [TestMethod]
        public void ErrorResponse_SerializesAsCodeAndMessage()
        {
            string json = JsonSerializer.Serialize(new ShardsException(ShardsErrorCode.CartEmpty).ToResponse(),
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            using var doc = JsonDocument.Parse(json);
            Assert.AreEqual("CartEmpty", doc.RootElement.GetProperty("code").GetString());
            Assert.AreEqual("O vagonete está vazio.", doc.RootElement.GetProperty("message").GetString());
        }

        [TestMethod]
        [DataRow("DoubleDrop")]
        [DataRow("doubledrop")]
        [DataRow("  DOUBLEDROP ")]
        public void ParseSkillType_AcceptsNameIgnoringCase(string value)
        {
            Assert.AreEqual(SkillType.DoubleDrop, ShardsErrors.ParseSkillType(value));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("Unknown")]
        [DataRow("1")]
        [DataRow("-1")]
        [DataRow("99")]
        public void ParseSkillType_Invalid_ThrowsInvalidSkill(string? value)
        {
            var ex = Assert.ThrowsExactly<ShardsException>(() => ShardsErrors.ParseSkillType(value));

            Assert.AreEqual(ShardsErrorCode.InvalidSkill, ex.Code);
        }

        [TestMethod]
        public void ParseMissionType_AcceptsName_RejectsNumbersAndUnknown()
        {
            Assert.AreEqual(MissionType.FirstPickaxe, ShardsErrors.ParseMissionType("firstpickaxe"));

            foreach (string? bad in new string?[] { null, "", "Nope", "1", "3", "0" })
            {
                var ex = Assert.ThrowsExactly<ShardsException>(() => ShardsErrors.ParseMissionType(bad));
                Assert.AreEqual(ShardsErrorCode.MissionNotFound, ex.Code);
            }
        }

        [TestMethod]
        public void Enums_SerializeAsNames()
        {
            var res = new OreAmountRes { OreType = OreType.Silver, Quantity = 3 };

            string json = JsonSerializer.Serialize(res, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            using var doc = JsonDocument.Parse(json);
            Assert.AreEqual("Silver", doc.RootElement.GetProperty("oreType").GetString());
        }
    }
}
