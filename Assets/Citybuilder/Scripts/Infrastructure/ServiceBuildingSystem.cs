using System.Collections.Generic;

namespace Citybuilder
{
    // Обобщённая система служб по видам ServiceKind (EPIC-01, ADR-006).
    // Владеет источниками по видам и пересчитывает снимок покрытия на математике ServiceCoverage.
    public class ServiceBuildingSystem
    {
        private readonly Dictionary<ServiceKind, List<ServiceSource>> sourcesByKind =
            new Dictionary<ServiceKind, List<ServiceSource>>();
        private readonly Dictionary<ServiceKind, ServiceCoverage> coverageByKind =
            new Dictionary<ServiceKind, ServiceCoverage>();
        private readonly List<GridPosition> lastHouses = new List<GridPosition>();

        // Задать список источников для одного вида службы. ServiceKind.None игнорируется.
        public void SetSources(ServiceKind kind, IReadOnlyList<ServiceSource> sources)
        {
            if (kind == ServiceKind.None) return;
            List<ServiceSource> list = EnsureSourceList(kind);
            list.Clear();
            if (sources == null) return;
            for (int i = 0; i < sources.Count; i++) list.Add(sources[i]);
        }

        // Полный сброс источников и снимка.
        public void Clear()
        {
            sourcesByKind.Clear();
            coverageByKind.Clear();
            lastHouses.Clear();
        }

        // Пересчитать снимок покрытия домов по всем зарегистрированным видам служб.
        public void BuildSnapshot(IReadOnlyList<GridPosition> houses)
        {
            lastHouses.Clear();
            if (houses != null)
            {
                for (int i = 0; i < houses.Count; i++) lastHouses.Add(houses[i]);
            }
            coverageByKind.Clear();
            foreach (KeyValuePair<ServiceKind, List<ServiceSource>> pair in sourcesByKind)
            {
                coverageByKind[pair.Key] = BuildKindCoverage(pair.Value, lastHouses);
            }
        }

        // Собрать источники служб из размещённых зданий и построить снимок по выросшим домам.
        public void BuildSnapshotFromStates(IReadOnlyList<BuildingState> placedStations, IReadOnlyList<BuildingState> grownHomes)
        {
            sourcesByKind.Clear();
            List<GridPosition> houses = new List<GridPosition>();
            if (placedStations != null)
            {
                for (int i = 0; i < placedStations.Count; i++)
                {
                    BuildingState state = placedStations[i];
                    if (state == null) continue;
                    AddStationSource(state);
                    if (RequiresService(state)) houses.Add(state.Origin);
                }
            }
            if (grownHomes != null)
            {
                for (int i = 0; i < grownHomes.Count; i++)
                {
                    if (grownHomes[i] != null) houses.Add(grownHomes[i].Origin);
                }
            }
            BuildSnapshot(houses);
        }

        private static bool RequiresService(BuildingState state) =>
            state != null && state.Definition != null && (state.Definition.ResidentCapacity > 0 || state.Definition.ServiceKind == ServiceKind.None);

        public int SourcesCount(ServiceKind kind)
        {
            List<ServiceSource> list;
            return sourcesByKind.TryGetValue(kind, out list) ? list.Count : 0;
        }

        public int TotalHomesCount() => lastHouses.Count;

        public int CoveredHomesCount(ServiceKind kind)
        {
            if (kind == ServiceKind.None) return 0;
            ServiceCoverage coverage;
            return coverageByKind.TryGetValue(kind, out coverage) ? coverage.CoveredPoints : 0;
        }

        // Доля покрытых домов (0..1). Если домов нет — 1 (потребность не блокирует город).
        public float Coverage01(ServiceKind kind)
        {
            if (lastHouses.Count == 0) return 1f;
            if (kind == ServiceKind.None) return 0f;
            ServiceCoverage coverage;
            return coverageByKind.TryGetValue(kind, out coverage) ? coverage.Coverage01 : 0f;
        }

        // Покрыт ли конкретный дом (1 = в радиусе хотя бы одной станции данного вида, иначе 0).
        public float Coverage01(ServiceKind kind, GridPosition house)
        {
            if (kind == ServiceKind.None) return 0f;
            ServiceCoverage coverage;
            if (!coverageByKind.TryGetValue(kind, out coverage)) return 0f;
            return coverage.IsCovered(house) ? 1f : 0f;
        }

        private void AddStationSource(BuildingState state)
        {
            if (state == null || state.Definition == null) return;
            BuildingDefinition def = state.Definition;
            if (def.ServiceKind == ServiceKind.None || def.ServiceRadius <= 0) return;
            EnsureSourceList(def.ServiceKind).Add(new ServiceSource(state.Origin, def.ServiceRadius));
        }

        private List<ServiceSource> EnsureSourceList(ServiceKind kind)
        {
            List<ServiceSource> list;
            if (!sourcesByKind.TryGetValue(kind, out list))
            {
                list = new List<ServiceSource>();
                sourcesByKind[kind] = list;
            }
            return list;
        }

        private static ServiceCoverage BuildKindCoverage(List<ServiceSource> sources, List<GridPosition> houses)
        {
            ServiceCoverage coverage = new ServiceCoverage();
            for (int i = 0; i < sources.Count; i++) coverage.SetSource(EntityId.New(), sources[i].Center, sources[i].Radius);
            for (int i = 0; i < houses.Count; i++) coverage.SetPoint(EntityId.New(), houses[i]);
            return coverage;
        }
    }
}
