using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Инфо-связность зданий с дорогами (T1, этап 053). Только чтение снимка.
    // Здание связано, если клетка по 4-соседству примыкает к дороге. BFS позже.
    // Ничем не используется: поведение игры не меняет.
    public class ConnectivitySystem
    {
        private readonly GridSystem grid;
        private readonly List<GridPosition> roadCells = new List<GridPosition>();
        private readonly List<GridPosition> buildingCells = new List<GridPosition>();

        public ConnectivitySystem(GridSystem grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }
            this.grid = grid;
        }

        public ConnectivitySystem(GridSystem grid, List<GridPosition> roads, List<GridPosition> buildings)
            : this(grid)
        {
            Rebuild(roads, buildings);
        }

        public int TotalCount
        {
            get { return buildingCells.Count; }
        }

        public int ConnectedCount
        {
            get
            {
                int connected = 0;
                for (int i = 0; i < buildingCells.Count; i++)
                {
                    if (IsConnected(buildingCells[i]))
                    {
                        connected = connected + 1;
                    }
                }
                return connected;
            }
        }

        public float ConnectionRate01
        {
            get
            {
                if (buildingCells.Count == 0)
                {
                    return 0f;
                }
                return (float)ConnectedCount / buildingCells.Count;
            }
        }

        // Заменяет снимок целиком. Списки копируются внутрь, null — пустой набор.
        public void Rebuild(List<GridPosition> roads, List<GridPosition> buildings)
        {
            roadCells.Clear();
            CopyInside(roads, roadCells);
            buildingCells.Clear();
            CopyInside(buildings, buildingCells);
        }

        // Вне карты или не-здание — false. Диагональ дорогой не считается.
        public bool IsConnected(GridPosition cell)
        {
            if (!grid.IsInside(cell))
            {
                return false;
            }
            if (!buildingCells.Contains(cell))
            {
                return false;
            }
            if (roadCells.Contains(new GridPosition(cell.X + 1, cell.Y)))
            {
                return true;
            }
            if (roadCells.Contains(new GridPosition(cell.X - 1, cell.Y)))
            {
                return true;
            }
            if (roadCells.Contains(new GridPosition(cell.X, cell.Y + 1)))
            {
                return true;
            }
            return roadCells.Contains(new GridPosition(cell.X, cell.Y - 1));
        }

        private void CopyInside(List<GridPosition> source, List<GridPosition> target)
        {
            if (source == null)
            {
                return;
            }
            for (int i = 0; i < source.Count; i++)
            {
                if (grid.IsInside(source[i]) && !target.Contains(source[i]))
                {
                    target.Add(source[i]);
                }
            }
        }
    }
}
