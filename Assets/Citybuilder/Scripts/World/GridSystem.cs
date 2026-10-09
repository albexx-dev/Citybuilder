using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Логическая квадратная сетка. Только данные, без GameObject.
    // Начало координат (0, 0), клетка CellSize x CellSize метров.
    public class GridSystem
    {
        private readonly Dictionary<GridPosition, EntityId> owners =
            new Dictionary<GridPosition, EntityId>();

        public GridSize Size { get; private set; }
        public float CellSize { get; private set; }

        public GridSystem(GridSize size, float cellSize)
        {
            if (cellSize <= 0f)
            {
                throw new ArgumentOutOfRangeException("cellSize", "CellSize must be positive.");
            }
            Size = size;
            CellSize = cellSize;
        }

        public int OccupiedCount
        {
            get { return owners.Count; }
        }

        public bool IsInside(GridPosition position)
        {
            return Size.Contains(position);
        }

        // Мировые метры в клетку. Дробная часть отбрасывается.
        public GridPosition WorldToGrid(float worldX, float worldZ)
        {
            int x = (int)Math.Floor(worldX / CellSize);
            int y = (int)Math.Floor(worldZ / CellSize);
            return new GridPosition(x, y);
        }

        // Центр клетки в мировых метрах.
        public void GridToWorld(GridPosition position, out float worldX, out float worldZ)
        {
            worldX = (position.X + 0.5f) * CellSize;
            worldZ = (position.Y + 0.5f) * CellSize;
        }

        // Вне карты считается занятым — стройка там запрещена.
        public bool IsOccupied(GridPosition position)
        {
            if (!IsInside(position))
            {
                return true;
            }
            return owners.ContainsKey(position);
        }

        public bool Occupy(GridPosition position, EntityId owner)
        {
            if (IsOccupied(position))
            {
                return false;
            }
            owners.Add(position, owner);
            return true;
        }

        public void Release(GridPosition position)
        {
            owners.Remove(position);
        }

        // Снимок занятых клеток для визуализации. Только чтение.
        public System.Collections.Generic.List<GridPosition> GetOccupiedCells()
        {
            return new System.Collections.Generic.List<GridPosition>(owners.Keys);
        }
    }
}
