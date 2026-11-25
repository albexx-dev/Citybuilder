using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса BuildingInfoQueryTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class BuildingInfoQueryTests
    {
        [Test]
        public void Query_PoliceStation_ReturnsServiceRadiusAndCounts()
        {
            GridSize size = new GridSize(50, 50);
            BuildingDefinition houseDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            BuildingDefinition policeDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            policeDef.Id = "police_station"; policeDef.DisplayName = "Police Station";
            policeDef.ServiceKind = ServiceKind.Police; policeDef.ServiceRadius = 5; policeDef.UpkeepPerHour = 6;

            BuildingState h1 = new BuildingState(EntityId.New(), houseDef, new GridPosition(2, 2), new List<GridPosition> { new GridPosition(2, 2) });
            BuildingState h2 = new BuildingState(EntityId.New(), houseDef, new GridPosition(30, 30), new List<GridPosition> { new GridPosition(30, 30) });
            BuildingState st = new BuildingState(EntityId.New(), policeDef, new GridPosition(0, 0), new List<GridPosition> { new GridPosition(0, 0) });

            BuildingInfo info = BuildingInfoQuery.Query(new GridPosition(0, 0), size, new List<BuildingState> { h1, h2 }, new List<BuildingState> { st }, null, null, 1f, 1f, 1f, null);
            Assert.IsTrue(info.Found); Assert.IsTrue(info.HasServiceRadius);
            Assert.AreEqual(ServiceKind.Police, info.ServiceKind); Assert.AreEqual(5, info.ServiceRadius);
            Assert.AreEqual(1, info.CoveredHomes); Assert.AreEqual(2, info.TotalHomes);
        }

        [Test]
        public void CanProcessHotkeys_TypingOrModal_GuardsHotkeys()
        {
            Assert.IsFalse(BuildModeHotkeys.CanProcessHotkeys(true, 0));
            Assert.IsFalse(BuildModeHotkeys.CanProcessHotkeys(false, 42));
            Assert.IsFalse(BuildModeHotkeys.CanProcessHotkeys(true, 42));
            Assert.IsTrue(BuildModeHotkeys.CanProcessHotkeys(false, 0));
        }

        [Test]
        public void BuildModeHotkeys_IsModalOpen_DetectsActiveModals()
        {
            GameObject go = new GameObject("TestHotkeysModal");
            try
            {
                BuildModeHotkeys hotkeys = go.AddComponent<BuildModeHotkeys>();
                PauseMenuView pause = go.AddComponent<PauseMenuView>();
                SaveMenuView save = go.AddComponent<SaveMenuView>();
                LoadMenuView load = go.AddComponent<LoadMenuView>();
                MainMenu main = go.AddComponent<MainMenu>();

                hotkeys.Pause = pause; hotkeys.SaveMenu = save; hotkeys.LoadMenu = load; hotkeys.Main = main;
                Assert.IsFalse(hotkeys.IsModalOpen());
                pause.Open(); Assert.IsTrue(hotkeys.IsModalOpen()); pause.Close();
                save.Open(); Assert.IsTrue(hotkeys.IsModalOpen()); save.Close();
                load.Open(); Assert.IsTrue(hotkeys.IsModalOpen()); load.Close();
                main.Open(); Assert.IsTrue(hotkeys.IsModalOpen()); main.Close();
                Assert.IsFalse(hotkeys.IsModalOpen());
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
