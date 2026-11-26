using System;
using System.Collections.Generic;
namespace Citybuilder
{
    // Occupation дорог по ключу (X,Y,Level) и A* с явными съездами 1<->2. Без Unity.
    public class BridgeRoadNetwork
    {
        public const string Wood = "wood";
        public const string Stone = "stone";
        private readonly GridSystem grid;
        private readonly Dictionary<RoadCellKey, string> roads = new Dictionary<RoadCellKey, string>();
        private readonly HashSet<GridPosition> ramps = new HashSet<GridPosition>();
        public BridgeRoadNetwork(GridSystem grid)
        {
            if (grid == null) throw new ArgumentNullException("grid");
            this.grid = grid;
        }
        public bool IsRoad(RoadCellKey key) { return roads.ContainsKey(key); }
        // Верхний уровень доминирует при показе; нижний остаётся в occupation.
        public int TopLevel(GridPosition cell)
        {
            return IsRoad(new RoadCellKey(cell.X, cell.Y, 2)) ? 2 : (IsRoad(new RoadCellKey(cell.X, cell.Y, 1)) ? 1 : -1);
        }
        public bool CanPlace(RoadCellKey key, string typeId)
        {
            if (key.Level < 1 || key.Level > 3 || !grid.IsInside(key.Position)) return false;
            if (IsRoad(key)) return false;
            if (key.Level == 1) return !grid.IsOccupied(key.Position);
            if (key.Level != 2 || (typeId != Wood && typeId != Stone)) return false;
            // Под мостом можно оставить дорогу level 1, но не здание или зону.
            return !grid.IsOccupied(key.Position) || IsRoad(new RoadCellKey(key.X, key.Y, 1));
        }
        public bool Place(RoadCellKey key, string typeId)
        {
            if (!CanPlace(key, typeId)) return false;
            roads.Add(key, typeId);
            if (key.Level == 1) grid.Occupy(key.Position, EntityId.New());
            return true;
        }
        // Легаси-дорогу, уже занявшую клетку через RoadSystem, добавляем без повторного Occupy.
        public bool RegisterStreet(RoadCellKey key)
        {
            if (key.Level != RoadCellKey.StreetLevel || !grid.IsInside(key.Position)) return false;
            if (!grid.IsOccupied(key.Position) || IsRoad(key)) return false;
            roads.Add(key, "asphalt");
            return true;
        }
        public bool Demolish(RoadCellKey key)
        {
            if (!IsRoad(key)) return false;
            roads.Remove(key);
            ramps.Remove(key.Position);
            if (key.Level == 1) grid.Release(key.Position);
            return true;
        }
        public bool SetRamp(RoadCellKey lower, RoadCellKey upper)
        {
            if (lower.Level != 1 || upper.Level != 2 || lower.Position != upper.Position) return false;
            if (!IsRoad(lower) || !IsRoad(upper)) return false;
            ramps.Add(lower.Position);
            return true;
        }
        public List<RoadCellKey> FindPath(RoadCellKey from, RoadCellKey to)
        {
            if (!IsRoad(from) || !IsRoad(to)) return null;
            if (from == to) return new List<RoadCellKey> { from };
            List<RoadCellKey> open = new List<RoadCellKey> { from };
            HashSet<RoadCellKey> closed = new HashSet<RoadCellKey>();
            Dictionary<RoadCellKey, RoadCellKey> parent = new Dictionary<RoadCellKey, RoadCellKey>();
            Dictionary<RoadCellKey, int> g = new Dictionary<RoadCellKey, int> { { from, 0 } };
            while (open.Count > 0)
            {
                int best = 0;
                int bestF = int.MaxValue;
                for (int i = 0; i < open.Count; i++)
                {
                    RoadCellKey key = open[i];
                    int dx = key.X - to.X; if (dx < 0) dx = -dx;
                    int dy = key.Y - to.Y; if (dy < 0) dy = -dy;
                    int dl = key.Level - to.Level; if (dl < 0) dl = -dl;
                    int f = g[key] + dx + dy + dl;
                    if (f < bestF) { bestF = f; best = i; }
                }
                RoadCellKey cur = open[best];
                open.Remove(cur);
                if (cur == to) return BuildPath(parent, to);
                closed.Add(cur);
                Relax(cur, open, closed, parent, g);
            }
            return null;
        }
        private void Relax(RoadCellKey cur, List<RoadCellKey> open, HashSet<RoadCellKey> closed,
            Dictionary<RoadCellKey, RoadCellKey> parent, Dictionary<RoadCellKey, int> g)
        {
            TryStep(new RoadCellKey(cur.X + 1, cur.Y, cur.Level), cur, open, closed, parent, g);
            TryStep(new RoadCellKey(cur.X - 1, cur.Y, cur.Level), cur, open, closed, parent, g);
            TryStep(new RoadCellKey(cur.X, cur.Y + 1, cur.Level), cur, open, closed, parent, g);
            TryStep(new RoadCellKey(cur.X, cur.Y - 1, cur.Level), cur, open, closed, parent, g);
            if (cur.Level < 3 && ramps.Contains(cur.Position))
            {
                int other = cur.Level == 1 ? 2 : 1;
                TryStep(new RoadCellKey(cur.X, cur.Y, other), cur, open, closed, parent, g);
            }
        }
        private void TryStep(RoadCellKey next, RoadCellKey cur, List<RoadCellKey> open,
            HashSet<RoadCellKey> closed, Dictionary<RoadCellKey, RoadCellKey> parent, Dictionary<RoadCellKey, int> g)
        {
            if (!IsRoad(next) || closed.Contains(next)) return;
            int cost = g[cur] + 1;
            if (g.ContainsKey(next) && cost >= g[next]) return;
            g[next] = cost;
            parent[next] = cur;
            if (!open.Contains(next)) open.Add(next);
        }
        private List<RoadCellKey> BuildPath(Dictionary<RoadCellKey, RoadCellKey> parent, RoadCellKey to)
        {
            List<RoadCellKey> path = new List<RoadCellKey>();
            RoadCellKey cur = to;
            path.Add(cur);
            while (parent.ContainsKey(cur))
            {
                cur = parent[cur];
                path.Add(cur);
            }
            path.Reverse();
            return path;
        }
    }
}
