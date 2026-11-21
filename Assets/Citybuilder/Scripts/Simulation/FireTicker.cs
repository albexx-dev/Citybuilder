using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Связка FireSystem с городом на границе тика (EPIC-43, ADR-009): подаёт дома и
    // снимок покрытия служб (ServiceBuildingSystem), пишет события пожаров в ленту.
    // Plain C#: снос сгоревшего дома выполняет Unity-сторона через делегат demolish.
    public class FireTicker
    {
        private readonly FireSystem fires = new FireSystem();
        private ServiceBuildingSystem coverage;
        private CityEventLog log;
        private PopulationSystem population;
        private Action<BuildingState> demolishState;

        public FireTicker()
        {
            fires.Fired += OnFireEvent;
        }

        // Снимок пожара для FireOverlayView (EPIC-43): домен горения, не сериализуется.
        public FireSystem Fires
        {
            get { return fires; }
        }

        // Один тик: дома, снимок ServiceBuildingSystem (null = все дома защищены), лента,
        // выселение жителей через PopulationSystem и демонтаж через делегат (паттерн DemolishOne).
        public void Tick(int tick, List<BuildingState> homes, ServiceBuildingSystem coverageSystem,
            CityEventLog log, PopulationSystem population, Action<BuildingState> demolishState)
        {
            coverage = coverageSystem;
            this.log = log;
            this.population = population;
            this.demolishState = demolishState;
            fires.Tick(tick, homes, HouseCoverage, BurnDown);
        }

        // Сброс по команде хоста (LoadController.ClearAll): горение не сериализуется (ADR-009).
        public void Reset()
        {
            fires.Reset();
            coverage = null;
            log = null;
            population = null;
            demolishState = null;
        }

        // Порядок сноса — как в SettlementDemolitionController.DemolishOne: сначала выселение.
        private void BurnDown(BuildingState home)
        {
            if (home == null) return;
            if (population != null) population.EvictHome(home);
            if (demolishState != null) demolishState(home);
        }

        // NaN при отсутствии снимка — FireSystem считает дом накрытым (безопасная сторона).
        private float HouseCoverage(BuildingState home)
        {
            if (coverage == null || home == null) return float.NaN;
            return coverage.Coverage01(ServiceKind.Fire, home.Origin);
        }

        private void OnFireEvent(EventKind kind, string message, int tick)
        {
            if (log != null) log.Add(kind, message, tick);
        }
    }
}
