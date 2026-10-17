using System;
using System.Collections.Generic;
namespace Citybuilder
{
    public class RouteSystem
    {
        private readonly GridSystem grid;
        private readonly List<GridPosition> roadCells = new List<GridPosition>();
        private bool useRoadList = false;
        private readonly Dictionary<GridPosition, int> loads = new Dictionary<GridPosition, int>();
        public RouteSystem(GridSystem grid) : this(grid, null) { }
        public RouteSystem(GridSystem grid, List<GridPosition> roads)
        {
            if (grid == null) throw new ArgumentNullException("grid");
            this.grid = grid;
            Rebuild(roads);
        }
        public void Rebuild(List<GridPosition> roads)
        {
            roadCells.Clear();
            useRoadList = roads != null;
            if (roads == null) return;
            for (int i = 0; i < roads.Count; i++)
            {
                GridPosition cell = roads[i];
                if (!grid.IsInside(cell)) continue;
                if (roadCells.Contains(cell)) continue;
                roadCells.Add(cell);
            }
        }
        public List<GridPosition> FindPath(GridPosition from, GridPosition to)
        {
            if (!grid.IsInside(from) || !grid.IsInside(to)) return null;
            if (!IsRoad(from) || !IsRoad(to)) return null;
            if (from == to)
            {
                List<GridPosition> one = new List<GridPosition>();
                one.Add(from);
                return one;
            }
            return Search(from, to);
        }
        private bool IsRoad(GridPosition cell)
        {
            if (!grid.IsInside(cell)) return false;
            if (useRoadList) return roadCells.Contains(cell);
            return grid.IsOccupied(cell);
        }
        private List<GridPosition> Search(GridPosition from, GridPosition to)
        {
            List<GridPosition> open = new List<GridPosition>();
            List<GridPosition> closed = new List<GridPosition>();
            Dictionary<GridPosition, GridPosition> parent = new Dictionary<GridPosition, GridPosition>();
            Dictionary<GridPosition, int> gScore = new Dictionary<GridPosition, int>();
            open.Add(from);
            gScore[from] = 0;
            while (open.Count > 0)
            {
                int bestIndex = FindBestOpen(open, to, gScore);
                GridPosition current = open[bestIndex];
                open.RemoveAt(bestIndex);
                if (current == to) return BuildPath(parent, to);
                closed.Add(current);
                RelaxNeighbors(current, open, closed, parent, gScore);
            }
            return null;
        }
        private int FindBestOpen(List<GridPosition> open, GridPosition to, Dictionary<GridPosition, int> gScore)
        {
            int bestIndex = 0;
            int bestF = FScore(open[0], to, gScore);
            for (int i = 1; i < open.Count; i++)
            {
                int f = FScore(open[i], to, gScore);
                if (f < bestF) { bestF = f; bestIndex = i; }
            }
            return bestIndex;
        }
        private int FScore(GridPosition node, GridPosition to, Dictionary<GridPosition, int> gScore)
        {
            int g = gScore.ContainsKey(node) ? gScore[node] : int.MaxValue;
            return g + Heuristic(node, to);
        }
        private int Heuristic(GridPosition a, GridPosition b)
        {
            int dx = a.X - b.X; if (dx < 0) dx = -dx;
            int dy = a.Y - b.Y; if (dy < 0) dy = -dy;
            return dx + dy;
        }
        private void RelaxNeighbors(GridPosition current, List<GridPosition> open, List<GridPosition> closed, Dictionary<GridPosition, GridPosition> parent, Dictionary<GridPosition, int> gScore)
        {
            int currentG = gScore[current];
            TryNeighbor(new GridPosition(current.X + 1, current.Y), current, currentG, open, closed, parent, gScore);
            TryNeighbor(new GridPosition(current.X - 1, current.Y), current, currentG, open, closed, parent, gScore);
            TryNeighbor(new GridPosition(current.X, current.Y + 1), current, currentG, open, closed, parent, gScore);
            TryNeighbor(new GridPosition(current.X, current.Y - 1), current, currentG, open, closed, parent, gScore);
        }
        private void TryNeighbor(GridPosition neighbor, GridPosition current, int currentG, List<GridPosition> open, List<GridPosition> closed, Dictionary<GridPosition, GridPosition> parent, Dictionary<GridPosition, int> gScore)
        {
            if (!grid.IsInside(neighbor)) return;
            if (!IsRoad(neighbor)) return;
            if (closed.Contains(neighbor)) return;
            int tentative = currentG + 1;
            if (gScore.ContainsKey(neighbor) && tentative >= gScore[neighbor]) return;
            parent[neighbor] = current;
            gScore[neighbor] = tentative;
            if (!open.Contains(neighbor)) open.Add(neighbor);
        }
        private List<GridPosition> BuildPath(Dictionary<GridPosition, GridPosition> parent, GridPosition to)
        {
            List<GridPosition> path = new List<GridPosition>();
            GridPosition cur = to;
            while (true)
            {
                path.Add(cur);
                if (!parent.ContainsKey(cur)) break;
                cur = parent[cur];
            }
            path.Reverse();
            return path;
        }
        public void TrackLoad(List<GridPosition> path)
        {
            if (path == null || path.Count == 0) return;
            for (int i = 0; i < path.Count; i++)
            {
                GridPosition cell = path[i];
                if (!loads.ContainsKey(cell)) loads[cell] = 0;
                loads[cell] = loads[cell] + 1;
            }
        }
        public int LoadOf(GridPosition cell)
        {
            if (!loads.ContainsKey(cell)) return 0;
            return loads[cell];
        }
        public void ClearLoads() { loads.Clear(); }
        public bool HasLoad(GridPosition cell) { return LoadOf(cell) > 0; }
    }
}
