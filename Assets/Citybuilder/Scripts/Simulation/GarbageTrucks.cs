using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Флот мусоровозов (EPIC-52): слоты мусоровозов, маршрут полигон → дома → полигон
    // и движение по клеткам. Plain C#: вид читает GarbageTrucks.Truck (как
    // VehicleAgentView читает AgentManager.VehicleAgent). Ноги маршрута — делегат
    // (дороги в сцене), иначе прямой L-маршрут по клеткам.
    public class GarbageTrucks
    {
        public const int MaxTrucks = 2;
        public const int HousesPerRun = 4;
        public const float CellsPerSecond = 2f;

        // Мусоровоз: маршрут [полигон, дом1..домN, полигон], дома идут по порядку середины.
        public struct Truck
        {
            public bool Active;
            public List<GridPosition> Route;
            public List<EntityId> Houses;
            public float Progress;
            public int Picked;
        }

        private readonly Truck[] trucks = new Truck[MaxTrucks];

        // Мусоровозов в пути прямо сейчас.
        public int ActiveCount
        {
            get { int count = 0; for (int i = 0; i < MaxTrucks; i++) if (trucks[i].Active) count++; return count; }
        }

        // Мусоровоз по индексу слота 0..MaxTrucks-1. Вне диапазона — неактивный.
        public Truck At(int index)
        {
            return index >= 0 && index < MaxTrucks ? trucks[index] : default(Truck);
        }

        public void Reset()
        {
            for (int i = 0; i < MaxTrucks; i++) trucks[i] = default(Truck);
        }

        // Кандидат депо без Unity-зависимостей: позиция и ёмкость из BuildingDefinition.
        public struct DepotCandidate
        {
            public GridPosition Origin;
            public int Capacity;

            public DepotCandidate(GridPosition origin, int capacity)
            {
                Origin = origin;
                Capacity = capacity;
            }
        }

        // Выбор депо (issue #90): 0 → null, 1 → оно, ≥2 → самое ёмкое;
        // равная ёмкость → первое в списке (детерминизм, без System.Random).
        public static GridPosition? SelectDepot(List<DepotCandidate> depots)
        {
            if (depots == null || depots.Count == 0) return null;
            int best = 0;
            for (int i = 1; i < depots.Count; i++)
            {
                if (depots[i].Capacity > depots[best].Capacity) best = i;
            }
            return depots[best].Origin;
        }

        // Выезд: если полигон есть и очередь не пуста, свободный слот берёт первый рейс.
        public void Dispatch(GridPosition? depot, List<GarbageSystem.Pickup> ready,
            Func<GridPosition, GridPosition, List<GridPosition>> pathFinder)
        {
            int slot = FreeSlot();
            if (!depot.HasValue || ready.Count == 0 || slot < 0) return;
            int count = Math.Min(HousesPerRun, ready.Count);
            List<GarbageSystem.Pickup> load = ready.GetRange(0, count);
            List<GridPosition> route = BuildRoute(depot.Value, load, pathFinder);
            if (route == null) return;
            ready.RemoveRange(0, count);
            Truck truck = default(Truck);
            truck.Active = true;
            truck.Route = route;
            truck.Houses = IdsOf(load);
            trucks[slot] = truck;
        }

        // Движение по секундам: у дома зовётся picked, на финальном полигоне слот гаснет.
        // Подбор приблизительный — по номеру ноги маршрута, а не по позиции дома (v1).
        public void Advance(float deltaTime, Action<EntityId> picked)
        {
            if (deltaTime <= 0f) return;
            for (int i = 0; i < MaxTrucks; i++)
            {
                if (!trucks[i].Active) continue;
                Truck truck = trucks[i];
                truck.Progress += CellsPerSecond * deltaTime;
                while (truck.Picked < truck.Houses.Count && truck.Progress >= truck.Picked + 1)
                {
                    truck.Picked++;
                    picked(truck.Houses[truck.Picked - 1]);
                }
                if (truck.Progress >= truck.Route.Count - 1) trucks[i] = default(Truck);
                else trucks[i] = truck;
            }
        }

        private int FreeSlot()
        {
            for (int i = 0; i < MaxTrucks; i++) if (!trucks[i].Active) return i;
            return -1;
        }

        // Маршрут полигон → дома → полигон: конкатенация ног без дублей стыковых клеток.
        private List<GridPosition> BuildRoute(GridPosition depot, List<GarbageSystem.Pickup> load,
            Func<GridPosition, GridPosition, List<GridPosition>> pathFinder)
        {
            List<GridPosition> route = new List<GridPosition>();
            List<GridPosition> stops = new List<GridPosition> { depot };
            for (int i = 0; i < load.Count; i++) stops.Add(load[i].At);
            stops.Add(depot);
            for (int i = 0; i < stops.Count - 1; i++)
            {
                List<GridPosition> leg = Leg(stops[i], stops[i + 1], pathFinder);
                if (leg == null) return null;
                if (route.Count > 0 && leg.Count > 0) leg.RemoveAt(0);
                route.AddRange(leg);
            }
            return route.Count >= 2 ? route : null;
        }

        // Нога маршрута: делегат (в сцене — дороги), иначе прямой L-маршрут по клеткам.
        private static List<GridPosition> Leg(GridPosition from, GridPosition to,
            Func<GridPosition, GridPosition, List<GridPosition>> pathFinder)
        {
            if (pathFinder != null)
            {
                List<GridPosition> path = pathFinder(from, to);
                if (path != null && path.Count >= 2) return path;
            }
            return DirectPath(from, to);
        }

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
