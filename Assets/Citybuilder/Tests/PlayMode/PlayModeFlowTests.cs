using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Citybuilder
{
    // PlayMode-оркестратор ключевого пути: зоны → рост → слот v6 и RU/EN → панели.
    // Все входы — публичные швы контроллеров; OnGUI и Input в тесте не вызываются.
    public partial class PlayModeFlowTests
    {
        private const int Slot = 2;
        private GameObject root;
        private SimulationRunner runner;
        private ZonePaintingController zones;
        private SettlementGrowthController growth;
        private SaveController save;
        private LoadController load;
        private BuildingDefinition def;
        private string indexBackup;
        private Func<string> savedLoad;
        private Action<string> savedSave;
        private string storedLanguage;
        private Language savedLanguage;

        [SetUp]
        public void SetUp()
        {
            string dir = Application.persistentDataPath;
            string index = Path.Combine(dir, SaveSlotIndex.IndexFileName);
            indexBackup = File.Exists(index) ? File.ReadAllText(index) : null;
            string slot = Path.Combine(dir, SaveSlotIndex.FileNameForSlot(Slot));
            if (File.Exists(slot)) File.Delete(slot);
            savedLoad = Localization.LoadPreference;
            savedSave = Localization.SavePreference;
            savedLanguage = Localization.Language;
            storedLanguage = string.Empty;
            Localization.LoadPreference = () => storedLanguage;
            Localization.SavePreference = value => storedLanguage = value;
            root = new GameObject("PlayModeFlowTests");
        }

        [TearDown]
        public void TearDown()
        {
            if (def != null) UnityEngine.Object.DestroyImmediate(def);
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            string dir = Application.persistentDataPath;
            string slot = Path.Combine(dir, SaveSlotIndex.FileNameForSlot(Slot));
            if (File.Exists(slot)) File.Delete(slot);
            string index = Path.Combine(dir, SaveSlotIndex.IndexFileName);
            if (indexBackup == null) { if (File.Exists(index)) File.Delete(index); }
            else File.WriteAllText(index, indexBackup);
            Localization.SetLanguage(savedLanguage);
            Localization.LoadPreference = savedLoad;
            Localization.SavePreference = savedSave;
        }

        [UnityTest]
        public IEnumerator NewGameZonesGrowthSaveLoadV6()
        {
            BuildCity();
            yield return null;
            load.NewGame();
            Assert.AreEqual(0, growth.GrownCount);
            zones.PaintZoneAt(new GridPosition(0, 0), ZoneType.Residential);
            zones.PaintZoneAt(new GridPosition(1, 0), ZoneType.Residential);
            Tick(4);
            Assert.AreEqual(2, growth.GrownCount);
            Assert.AreEqual(2, zones.Zones.CountZone(ZoneType.Residential));
            long tick = runner.Clock.CurrentTime.TickCount;
            Assert.IsTrue(save.SaveNow(Slot));
            CitySnapshot slot = SaveSlotStorage.Load(Application.persistentDataPath, Slot);
            Assert.IsNotNull(slot);
            Assert.AreEqual(SaveV6Snapshot.Version, slot.Version);
            Assert.AreEqual(2, slot.Buildings.Count);
            Assert.AreEqual(tick, slot.TickCount);
            load.NewGame();
            Assert.AreEqual(0, growth.GrownCount);
            Assert.IsTrue(load.LoadNow(Slot));
            Assert.AreEqual(2, growth.GrownCount);
            Assert.AreEqual(tick, runner.Clock.CurrentTime.TickCount);
            Assert.AreEqual(2, zones.Zones.CountZone(ZoneType.Residential));
        }

        [UnityTest]
        public IEnumerator LanguageSwitchAndPanels()
        {
            LanguageToggleView toggle = root.AddComponent<LanguageToggleView>();
            InfoMenuView info = root.AddComponent<InfoMenuView>();
            EventLogView log = root.AddComponent<EventLogView>();
            QuestLogView quests = root.AddComponent<QuestLogView>();
            yield return null;
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Жилая", Localization.T("zone_residential"));
            toggle.Toggle();
            Assert.AreEqual(Language.En, Localization.Language);
            Assert.AreEqual("Residential", Localization.T("zone_residential"));
            Assert.AreEqual("En", storedLanguage);
            info.SetOpen(true);
            log.SetOpen(true);
            quests.SetOpen(true);
            Assert.IsTrue(info.IsOpen && log.IsOpen && quests.IsOpen);
            toggle.Toggle();
            Assert.AreEqual(Language.Ru, Localization.Language);
            Assert.IsTrue(info.IsOpen);
            info.SetOpen(false);
            log.SetOpen(false);
            quests.SetOpen(false);
            Assert.IsFalse(info.IsOpen || log.IsOpen || quests.IsOpen);
        }

        private void Tick(int count)
        {
            for (int i = 0; i < count; i++) runner.Clock.Update(1f);
        }

    }
}
