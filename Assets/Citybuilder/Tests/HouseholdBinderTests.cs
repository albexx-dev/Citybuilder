using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Привязка домохозяйств к домам после загрузки: координаты (новые слоты), индекс (старые).
    public class HouseholdBinderTests
    {
        [Test]
        public void Apply_SkippedBuilding_KeepsLaterHouseholds()
        {
            BuildingDefinition def = NewHouseDef();
            BuildingState a = NewHome(def, 0, 0);
            BuildingState b = NewHome(def, 4, 0);
            BuildingState c = NewHome(def, 8, 0);
            PopulationSystem saved = new PopulationSystem();
            saved.MoveIn(a, 2);
            saved.MoveIn(c, 3);
            CitySnapshot snapshot = SaveSnapshot(a, b, c, saved);

            PopulationSystem restored = new PopulationSystem();
            List<BuildingState> homes = new List<BuildingState>();
            homes.Add(a);
            homes.Add(c);
            LoadSystem.ApplyHouseholds(snapshot, restored, homes);

            Assert.AreEqual(5, restored.TotalResidents);
            Assert.AreEqual(2, restored.ResidentsIn(a));
            Assert.AreEqual(3, restored.ResidentsIn(c));
            Object.DestroyImmediate(def);
        }

        [Test]
        public void Apply_LegacySlotWithoutHome_ByIndex()
        {
            BuildingDefinition def = NewHouseDef();
            BuildingState a = NewHome(def, 0, 0);
            PopulationSystem saved = new PopulationSystem();
            saved.MoveIn(a, 2);
            CitySnapshot snapshot = SaveSnapshot(a, saved);
            snapshot.Households[0].HasHome = false;

            PopulationSystem restored = new PopulationSystem();
            List<BuildingState> homes = new List<BuildingState>();
            homes.Add(a);
            LoadSystem.ApplyHouseholds(snapshot, restored, homes);

            Assert.AreEqual(2, restored.ResidentsIn(a));
            Object.DestroyImmediate(def);
        }

        [Test]
        public void Apply_MissingHome_DoesNotAttachToStranger()
        {
            BuildingDefinition def = NewHouseDef();
            BuildingState a = NewHome(def, 0, 0);
            PopulationSystem saved = new PopulationSystem();
            saved.MoveIn(a, 2);
            CitySnapshot snapshot = SaveSnapshot(a, saved);
            snapshot.Households[0].HomeX = 12;
            snapshot.Households[0].HomeY = 12;

            PopulationSystem restored = new PopulationSystem();
            List<BuildingState> homes = new List<BuildingState>();
            homes.Add(a);
            LoadSystem.ApplyHouseholds(snapshot, restored, homes);

            Assert.AreEqual(0, restored.TotalResidents);
            Assert.AreEqual(0, restored.ResidentsIn(a));
            Object.DestroyImmediate(def);
        }

        [Test]
        public void Json_Roundtrip_KeepsHomeCoordinates()
        {
            BuildingDefinition def = NewHouseDef();
            BuildingState a = NewHome(def, 3, 5);
            PopulationSystem saved = new PopulationSystem();
            saved.MoveIn(a, 2);
            CitySnapshot snapshot = SaveSnapshot(a, saved);

            CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(snapshot));

            Assert.IsTrue(restored.Households[0].HasHome);
            Assert.AreEqual(3, restored.Households[0].HomeX);
            Assert.AreEqual(5, restored.Households[0].HomeY);
            Object.DestroyImmediate(def);
        }

        private static CitySnapshot SaveSnapshot(BuildingState home, PopulationSystem population)
        {
            List<BuildingState> homes = new List<BuildingState>();
            homes.Add(home);
            return SaveSystem.BuildSnapshot(homes, population.Households, null, 0, 0, null);
        }

        private static CitySnapshot SaveSnapshot(BuildingState a, BuildingState b,
            BuildingState c, PopulationSystem population)
        {
            List<BuildingState> homes = new List<BuildingState>();
            homes.Add(a);
            homes.Add(b);
            homes.Add(c);
            return SaveSystem.BuildSnapshot(homes, population.Households, null, 0, 0, null);
        }

        private static BuildingState NewHome(BuildingDefinition def, int x, int y)
        {
            GridPosition origin = new GridPosition(x, y);
            return new BuildingState(EntityId.New(), def, origin, def.CoversCells(origin));
        }

        private static BuildingDefinition NewHouseDef()
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house";
            def.AllowedZone = ZoneType.Residential;
            def.ResidentCapacity = 4;
            return def;
        }
    }
}
