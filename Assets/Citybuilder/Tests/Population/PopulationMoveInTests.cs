using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Въезд 055: связные дома раньше, иначе старый first-fit. Снимок настоящий, логика чистая.
    // Unity нужен только фикстурам (CreateInstance), проверки — числа чистой логики.
    public class PopulationMoveInTests
    {
        private PopulationSystem population;
        private List<BuildingState> homes;
        private ConnectivitySystem connectivity;

        [SetUp]
        public void SetUp()
        {
            MakeScene(null, out population, out homes, out connectivity);
        }

        private static void MakeWorld(out GridSystem grid, out SettlementSystem settlement, out PopulationSystem population)
        {
            grid = new GridSystem(new GridSize(20, 20), 2f);
            settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            population = new PopulationSystem();
        }

        private static BuildingDefinition HouseDef(int capacity)
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.AllowedZone = ZoneType.None;
            definition.Footprint = new Vector2Int(1, 1);
            definition.ResidentCapacity = capacity;
            definition.JobsProvided = 0;
            return definition;
        }

        private static BuildingState MustSettle(SettlementSystem settlement, BuildingDefinition definition, int slot)
        {
            BuildingState state = settlement.Settle(definition, new GridPosition(slot * 2, 0));
            Assert.IsNotNull(state);
            return state;
        }

        // Два дома (0,0) и (2,0) плюс снимок связности. Дорог нет — все несвязные.
        private static void MakeScene(List<GridPosition> roads, out PopulationSystem population, out List<BuildingState> homes, out ConnectivitySystem connectivity)
        {
            GridSystem grid;
            SettlementSystem settlement;
            MakeWorld(out grid, out settlement, out population);
            homes = new List<BuildingState> { MustSettle(settlement, HouseDef(4), 0), MustSettle(settlement, HouseDef(4), 1) };
            List<GridPosition> cells = new List<GridPosition>();
            for (int i = 0; i < homes.Count; i++)
            {
                for (int c = 0; c < homes[i].Cells.Count; c++) cells.Add(homes[i].Cells[c]);
            }
            connectivity = new ConnectivitySystem(grid, roads, cells);
        }

        // Дом связан, если хотя бы одна его клетка связана. Повторяет правило контроллера.
        private static bool IsHomeConnected(ConnectivitySystem connectivity, BuildingState home)
        {
            if (home == null || home.Cells == null) return false;
            for (int i = 0; i < home.Cells.Count; i++)
            {
                if (connectivity.IsConnected(home.Cells[i])) return true;
            }
            return false;
        }

        [Test]
        public void ConnectedHome_WinsOverEarlier()
        {
            MakeScene(new List<GridPosition> { new GridPosition(3, 0) }, out population, out homes, out connectivity);
            BuildingState chosen = PopulationController.FindMoveInHome(homes, population, 2,
                home => IsHomeConnected(connectivity, home));
            Assert.AreSame(homes[1], chosen);
        }

        [Test]
        public void TwoConnected_FirstWins()
        {
            MakeScene(new List<GridPosition> { new GridPosition(1, 0), new GridPosition(3, 0) }, out population, out homes, out connectivity);
            BuildingState chosen = PopulationController.FindMoveInHome(homes, population, 2,
                home => IsHomeConnected(connectivity, home));
            Assert.AreSame(homes[0], chosen);
        }

        [Test]
        public void NullPredicate_OldFirstFit()
        {
            BuildingState chosen = PopulationController.FindMoveInHome(homes, population, 2, null);
            Assert.AreSame(homes[0], chosen);
        }

        [Test]
        public void OnlyUnconnected_SettledNotBlocked()
        {
            MakeScene(new List<GridPosition> { new GridPosition(10, 10) }, out population, out homes, out connectivity);
            BuildingState chosen = PopulationController.FindMoveInHome(homes, population, 2,
                home => IsHomeConnected(connectivity, home));
            Assert.AreSame(homes[0], chosen);
        }

        [Test]
        public void ConnectedFull_FallsBackToFree()
        {
            MakeScene(new List<GridPosition> { new GridPosition(3, 0) }, out population, out homes, out connectivity);
            Assert.IsNotNull(population.MoveIn(homes[1], 4));
            BuildingState chosen = PopulationController.FindMoveInHome(homes, population, 2,
                home => IsHomeConnected(connectivity, home));
            Assert.AreSame(homes[0], chosen);
        }

        [Test]
        public void AllFull_Null()
        {
            Assert.IsNotNull(population.MoveIn(homes[0], 4));
            Assert.IsNotNull(population.MoveIn(homes[1], 4));
            BuildingState chosen = PopulationController.FindMoveInHome(homes, population, 2, null);
            Assert.IsNull(chosen);
        }

        [Test]
        public void NullInputs_Null()
        {
            Assert.IsNull(PopulationController.FindMoveInHome(null, population, 2, null));
            Assert.IsNull(PopulationController.FindMoveInHome(homes, null, 2, null));
        }

        [Test]
        public void ControllerNoWiring_NotConnected()
        {
            GameObject go = new GameObject();
            ConnectivityController controller = go.AddComponent<ConnectivityController>();
            List<GridPosition> cells = new List<GridPosition> { new GridPosition(0, 0) };
            BuildingState home = new BuildingState(EntityId.New(), HouseDef(4), new GridPosition(0, 0), cells);
            Assert.IsFalse(controller.IsHomeConnected(null));
            Assert.IsFalse(controller.IsHomeConnected(home));
            Object.DestroyImmediate(go);
        }
    }
}
