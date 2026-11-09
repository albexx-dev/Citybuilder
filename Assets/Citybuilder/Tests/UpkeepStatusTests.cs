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
        [TestCase("PowerStation", 8)]
        [TestCase("WaterTower", 6)]
        [TestCase("WasteDepot", 5)]
        [TestCase("FireStation", 6)]
        [TestCase("PoliceStation", 6)]
        public void AssetUpkeep_MatchesEpicValues(string name, int expected)
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

        // Девять зданий со значениями EPIC-04: 0+0+0 +1+1 +2 +8 +6 +5 = 23 $/ч и строка говорит 23.
        [Test]
        public void CityOfEpicBuildings_LineShowsRealSum()
        {
            List<BuildingDefinition> placed = new List<BuildingDefinition>
            {
                Load("House"), Load("House"), Load("House"),
                Load("Shop"), Load("Shop"),
                Load("Factory"), Load("PowerStation"), Load("WaterTower"), Load("WasteDepot")
            };
            int upkeep = UpkeepPolicy.UpkeepPerHour(placed);
            Assert.AreEqual(23, upkeep);
            Assert.AreEqual("Расход: 23 $/ч", UpkeepStatusPolicy.Text(upkeep));
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
