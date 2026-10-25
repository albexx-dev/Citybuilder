using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Покрытие служб: у источников (станций) есть позиция и радиус в клетках,
    // у точек спроса (зданий) — позиция. Чистый C#, без Unity.
    // Расстояние евклидово, сравнение по квадратам (без корня).
    public class ServiceCoverage
    {
        // Источник покрытия: станция и её радиус действия в клетках.
        private struct Source
        {
            public readonly GridPosition Position;
            public readonly int Radius;

            public Source(GridPosition position, int radius)
            {
                Position = position;
                Radius = radius;
            }
        }

        private readonly Dictionary<EntityId, Source> sources = new Dictionary<EntityId, Source>();
        private readonly Dictionary<EntityId, GridPosition> points = new Dictionary<EntityId, GridPosition>();

        public int SourceCount { get { return sources.Count; } }

        public int PointCount { get { return points.Count; } }

        // Дубликат id перезаписывает источник. Отрицательный радиус — 0.
        public void SetSource(EntityId id, GridPosition position, int radius)
        {
            sources[id] = new Source(position, radius > 0 ? radius : 0);
        }

        public void RemoveSource(EntityId id) { sources.Remove(id); }

        public void ClearSources() { sources.Clear(); }

        // Дубликат id перезаписывает позицию точки.
        public void SetPoint(EntityId id, GridPosition position) { points[id] = position; }

        public void RemovePoint(EntityId id) { points.Remove(id); }

        // Убирает и источники, и точки.
        public void Clear()
        {
            sources.Clear();
            points.Clear();
        }

        // Точка накрыта, если хотя бы один источник достаёт до неё.
        public bool IsCovered(GridPosition point)
        {
            foreach (KeyValuePair<EntityId, Source> pair in sources)
            {
                if (IsInside(pair.Value, point))
                {
                    return true;
                }
            }
            return false;
        }

        // Доля конкретной точки: точки нет — 0, накрыта — 1, иначе — 0.
        public float RateOf(EntityId pointId)
        {
            GridPosition position;
            if (!points.TryGetValue(pointId, out position))
            {
                return 0f;
            }
            return IsCovered(position) ? 1f : 0f;
        }

        // Сколько точек спроса накрыто (перекрытие источников не двоит счёт).
        public int CoveredPoints
        {
            get
            {
                int covered = 0;
                foreach (KeyValuePair<EntityId, GridPosition> pair in points)
                {
                    if (IsCovered(pair.Value))
                    {
                        covered = covered + 1;
                    }
                }
                return covered;
            }
        }

        // Доля накрытых точек 0..1. Точек нет — 1.
        public float Coverage01
        {
            get
            {
                int total = points.Count;
                if (total == 0)
                {
                    return 1f;
                }
                return (float)CoveredPoints / total;
            }
        }

        // Расстояние до ближайшего источника, округлённое вниз. Источников нет — −1.
        public int NearestDistance(GridPosition point)
        {
            if (sources.Count == 0)
            {
                return -1;
            }
            long best = long.MaxValue;
            foreach (KeyValuePair<EntityId, Source> pair in sources)
            {
                long distance = SquaredDistance(pair.Value.Position, point);
                if (distance < best)
                {
                    best = distance;
                }
            }
            return (int)Math.Sqrt(best);
        }

        private static bool IsInside(Source source, GridPosition point)
        {
            long radius = source.Radius;
            return SquaredDistance(source.Position, point) <= radius * radius;
        }

        // Квадрат расстояния: без корня и без переполнения int.
        private static long SquaredDistance(GridPosition a, GridPosition b)
        {
            long dx = (long)a.X - b.X;
            long dy = (long)a.Y - b.Y;
            return dx * dx + dy * dy;
        }
    }
}
