using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Фильтр метрики связности (issue #42): станции (ResidentCapacity == 0)
    // не входят в знаменатель, жилые ручные постройки — входят.
    public class ConnectivityMetricTests
    {
        private static BuildingState MakeState(BuildingDefinition definition, int x, int y)
        {
            List<GridPosition> cells = new List<GridPosition> { new GridPosition(x, y) };
            return new BuildingState(EntityId.New(), definition, new GridPosition(x, y), cells);
        }

        [Test]
        public void ResidentialOnly_ExcludesStationsAndKeepsHomes()
        {
            BuildingDefinition home = ScriptableObject.CreateInstance<BuildingDefinition>();
            home.Id = "house"; home.DisplayName = "House"; home.ResidentCapacity = 4;
            BuildingDefinition station = ScriptableObject.CreateInstance<BuildingDefinition>();
            station.Id = "power_station"; station.DisplayName = "Power Station"; station.ResidentCapacity = 0;
            List<BuildingState> placed = new List<BuildingState>
            {
                MakeState(home, 1, 1),
                MakeState(station, 5, 5),
            };
            try
            {
                List<GridPosition> metricCells = new List<GridPosition>();
                ConnectivityController.AppendBuildingCells(metricCells, placed, true);

                Assert.AreEqual(1, metricCells.Count);
                Assert.AreEqual(new GridPosition(1, 1), metricCells[0]);

                List<GridPosition> grownCells = new List<GridPosition>();
                ConnectivityController.AppendBuildingCells(grownCells, placed, false);
                Assert.AreEqual(2, grownCells.Count);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(home);
                ScriptableObject.DestroyImmediate(station);
            }
        }

        [Test]
        public void ResidentialOnly_StateWithoutDefinition_SkippedWithoutThrow()
        {
            BuildingDefinition home = ScriptableObject.CreateInstance<BuildingDefinition>();
            home.Id = "house"; home.DisplayName = "House"; home.ResidentCapacity = 4;
            BuildingState broken = new BuildingState(EntityId.New(), null, new GridPosition(3, 3), new List<GridPosition>());
            List<BuildingState> placed = new List<BuildingState> { MakeState(home, 1, 1), broken };
            try
            {
                List<GridPosition> cells = new List<GridPosition>();
                ConnectivityController.AppendBuildingCells(cells, placed, true);

                Assert.AreEqual(1, cells.Count);
                Assert.AreEqual(new GridPosition(1, 1), cells[0]);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(home);
            }
        }
    }
}
