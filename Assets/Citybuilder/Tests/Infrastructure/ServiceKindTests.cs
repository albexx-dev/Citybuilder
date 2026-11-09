using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Citybuilder
{
    // Проверки типа службы в описании здания и ассетов Hospital/Park (EPIC-02 шаг 1).
    public class ServiceKindTests
    {
        private const string HospitalPath = "Assets/Citybuilder/Data/Buildings/Hospital.asset";
        private const string ParkPath = "Assets/Citybuilder/Data/Buildings/Park.asset";

        [Test]
        public void NewDefinition_ServiceKindIsNone()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            Assert.AreEqual(ServiceKind.None, definition.ServiceKind);
        }

        [Test]
        public void AssignedPoliceHealthLeisure_AreStoredAndSanitized()
        {
            Assert.AreEqual(ServiceKind.None, BuildingDefinition.SanitizeServiceKind((ServiceKind)99));
            Assert.AreEqual(ServiceKind.Fire, BuildingDefinition.SanitizeServiceKind(ServiceKind.Fire));
            Assert.AreEqual(ServiceKind.Police, BuildingDefinition.SanitizeServiceKind(ServiceKind.Police));
            Assert.AreEqual(ServiceKind.Health, BuildingDefinition.SanitizeServiceKind(ServiceKind.Health));
            Assert.AreEqual(ServiceKind.Leisure, BuildingDefinition.SanitizeServiceKind(ServiceKind.Leisure));
        }

        [Test]
        public void HospitalAndParkAssets_MatchEpic02SpecAndUnlockThresholds()
        {
            BuildingDefinition hospital = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(HospitalPath);
            BuildingDefinition park = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(ParkPath);
            Assert.IsNotNull(hospital);
            Assert.IsNotNull(park);

            Assert.AreEqual("hospital", hospital.Id);
            Assert.AreEqual(new Vector2Int(3, 3), hospital.Footprint);
            Assert.AreEqual(500, hospital.ConstructionCost);
            Assert.AreEqual(ServiceKind.Health, hospital.ServiceKind);
            Assert.AreEqual(7, hospital.ServiceRadius);
            Assert.AreEqual(12, hospital.UnlockBuildings);
            Assert.IsFalse(UnlockPolicy.IsUnlocked(hospital, 9));
            Assert.AreEqual(3, UnlockPolicy.Remaining(hospital, 9));
            Assert.IsTrue(UnlockPolicy.IsUnlocked(hospital, 12));

            Assert.AreEqual("park", park.Id);
            Assert.AreEqual(new Vector2Int(2, 2), park.Footprint);
            Assert.AreEqual(150, park.ConstructionCost);
            Assert.AreEqual(ServiceKind.Leisure, park.ServiceKind);
            Assert.AreEqual(5, park.ServiceRadius);
            Assert.AreEqual(10, park.UnlockBuildings);
            Assert.IsFalse(UnlockPolicy.IsUnlocked(park, 9));
            Assert.IsTrue(UnlockPolicy.IsUnlocked(park, 10));
        }

        [Test]
        public void HealthAndLeisureCoverage_DoNotMixWithFireAndPolice()
        {
            ServiceBuildingSystem system = new ServiceBuildingSystem();
            system.SetSources(ServiceKind.Health, new List<ServiceSource> { new ServiceSource(new GridPosition(0, 0), 7) });
            system.SetSources(ServiceKind.Leisure, new List<ServiceSource> { new ServiceSource(new GridPosition(20, 20), 5) });
            system.BuildSnapshot(new List<GridPosition> { new GridPosition(3, 4), new GridPosition(20, 22) });

            Assert.AreEqual(1, system.CoveredHomesCount(ServiceKind.Health));
            Assert.AreEqual(1, system.CoveredHomesCount(ServiceKind.Leisure));
            Assert.AreEqual(0, system.CoveredHomesCount(ServiceKind.Fire));
            Assert.AreEqual(0, system.CoveredHomesCount(ServiceKind.Police));
        }
    }
}
