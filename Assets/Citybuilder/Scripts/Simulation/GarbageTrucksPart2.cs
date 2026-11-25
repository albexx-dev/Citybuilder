using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Хвост класса GarbageTrucks (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class GarbageTrucks
    {
        // Прямой маршрут по клеткам: сначала по X, потом по Y (v1: без дорог, напрямик).
        private static List<GridPosition> DirectPath(GridPosition from, GridPosition to)
        {
            List<GridPosition> path = new List<GridPosition>();
            int stepX = Math.Sign(to.X - from.X);
            int stepY = Math.Sign(to.Y - from.Y);
            int x = from.X;
            int y = from.Y;
            path.Add(from);
            while (x != to.X) { x += stepX; path.Add(new GridPosition(x, y)); }
            while (y != to.Y) { y += stepY; path.Add(new GridPosition(x, y)); }
            return path;
        }

        private static List<EntityId> IdsOf(List<GarbageSystem.Pickup> load)
        {
            List<EntityId> ids = new List<EntityId>(load.Count);
            for (int i = 0; i < load.Count; i++) ids.Add(load[i].Id);
            return ids;
        }
    }
}
