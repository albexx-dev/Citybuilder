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
        [TestCase("Shop", 1)]
        [TestCase("Factory", 2)]
        [TestCase("PowerStation", 4)]
        [TestCase("WaterTower", 3)]
        [TestCase("WasteDepot", 3)]
        [TestCase("FireStation", 3)]
        [TestCase("PoliceStation", 3)]
        [TestCase("Hospital", 5)]
        [TestCase("Park", 1)]
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

        // Девять зданий после ребаланса 171: 0+0+0 +1+1 +2 +4 +3 +3 = 14 $/ч и строка говорит 14.
        [Test]
        public void CityOfRebalancedBuildings_LineShowsRealSum()
        {
            List<BuildingDefinition> placed = new List<BuildingDefinition>
            {
                Load("House"), Load("House"), Load("House"),
                Load("Shop"), Load("Shop"),
                Load("Factory"), Load("PowerStation"), Load("WaterTower"), Load("WasteDepot")
            };
            int upkeep = UpkeepPolicy.UpkeepPerHour(placed);
            Assert.AreEqual(14, upkeep);
            Assert.AreEqual("Расход: 14 $/ч", UpkeepStatusPolicy.Text(upkeep));
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
