using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;

namespace Citybuilder
{
    // Строка расхода: её текст, вид при дефиците и фактические значения содержания в ассетах.
    // UpkeepStatusPolicy чистая и без GUI; ассеты читаются через AssetDatabase, как в SupplyStationAssetTests.
    public class UpkeepStatusTests
    {
        private const string BuildingsFolder = "Assets/Citybuilder/Data/Buildings/";

        // Нулевое содержание строка не прячет и не округляет до «нет расходов».
        [Test]
        public void ZeroUpkeep_LineSaysZero()
        {
            Assert.AreEqual("Расход: 0 $/ч", UpkeepStatusPolicy.Text(0));
        }

        [Test]
        public void UpkeepLine_ShowsRealNumber()
        {
            Assert.AreEqual("Расход: 26 $/ч", UpkeepStatusPolicy.Text(26));
        }

        // Баланс >= 0 — казна не в дефиците, строка обычного вида.
        [Test]
        public void BalancedTreasury_LineLooksNormal()
        {
            Assert.AreEqual(UpkeepStatusPolicy.Look.Normal, UpkeepStatusPolicy.Pick(false));
        }

        // Баланс < 0 — Treasury.IsInDeficit, строка красится в дефицитный цвет.
        [Test]
        public void DeficitTreasury_LineLooksHighlighted()
        {
            Assert.AreEqual(UpkeepStatusPolicy.Look.Deficit, UpkeepStatusPolicy.Pick(true));
        }

        [TestCase("House", 0)]
        [TestCase("Shop", 5)]
        [TestCase("Factory", 7)]
        [TestCase("PowerStation", 3)]
        [TestCase("WaterTower", 3)]
        [TestCase("WasteDepot", 3)]
        [TestCase("FireStation", 6)]
        [TestCase("PoliceStation", 6)]
        [TestCase("Hospital", 12)]
        [TestCase("Park", 3)]
        public void AssetUpkeep_MatchesRebalancedValues(string name, int expected)
        {
            Assert.AreEqual(expected, Load(name).UpkeepPerHour, name);
        }

        // Город без станций: расход нулевой, дефицит по содержанию недостижим.
        [Test]
        public void HousesOnly_CostsNothingPerHour()
        {
            List<BuildingDefinition> placed = new List<BuildingDefinition>
            {
                Load("House"), Load("House"), Load("House")
            };
            Assert.AreEqual(0, UpkeepPolicy.UpkeepPerHour(placed));
        }

        // Девять зданий после баланса v1.2: база 26 $/ч, фактическое списание 26 ÷ 8 = 3 $/ч.
        [Test]
        public void CityOfRebalancedBuildings_LineShowsEffectiveCharge()
        {
            List<BuildingDefinition> placed = new List<BuildingDefinition>
            {
                Load("House"), Load("House"), Load("House"),
                Load("Shop"), Load("Shop"),
                Load("Factory"), Load("PowerStation"), Load("WaterTower"), Load("WasteDepot")
            };
            int baseUpkeep = UpkeepPolicy.UpkeepPerHour(placed);
            int upkeep = EffectiveUpkeepPolicy.Apply(baseUpkeep);
            Assert.AreEqual(26, baseUpkeep);
            Assert.AreEqual(3, upkeep);
            Assert.AreEqual("Расход: 3 $/ч", UpkeepStatusPolicy.Text(upkeep));
        }

        private static BuildingDefinition Load(string name)
        {
            string path = BuildingsFolder + name + ".asset";
            BuildingDefinition definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            Assert.IsNotNull(definition, "Missing asset: " + path);
            return definition;
        }
    }
}
