using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Связка CrimeSystem с городом на границе тика (EPIC-47): подаёт дома и снимок покрытия
    // служб, уровень преступности города считает как 1 − Coverage01(Police), пишет кражи в
    // ленту и списывает штраф казне. Plain C#: без Unity, как FireTicker (EPIC-43).
    public class CrimeTicker
    {
        private readonly CrimeSystem crimes = new CrimeSystem();
        private ServiceBuildingSystem coverage;
        private CityEventLog log;
        private Treasury treasury;

        public CrimeTicker()
        {
            crimes.Fired += OnCrimeEvent;
        }

        // Снимок краж для InfoMenuView (EPIC-47): уровень преступности и модификаторы домов.
        public CrimeSystem Crimes
        {
            get { return crimes; }
        }

        // Один тик: дома, снимок ServiceBuildingSystem (null = город в безопасности),
        // лента событий и казна (null — кражи без штрафа).
        public void Tick(int tick, List<BuildingState> homes, ServiceBuildingSystem coverageSystem,
            CityEventLog log, Treasury treasury)
        {
            coverage = coverageSystem;
            this.log = log;
            this.treasury = treasury;
            crimes.Tick(tick, homes, HouseCoverage, CrimeLevel(coverageSystem), FineTreasury);
        }

        // NaN при отсутствии снимка — CrimeSystem считает дом в безопасности (план тестов эпика).
        private float HouseCoverage(BuildingState home)
        {
            if (coverage == null || home == null) return float.NaN;
            return coverage.Coverage01(ServiceKind.Police, home.Origin);
        }

        // Уровень преступности города = 1 − доля домов под полицией; без снимка — 0.
        private static float CrimeLevel(ServiceBuildingSystem coverageSystem)
        {
            if (coverageSystem == null) return 0f;
            return 1f - coverageSystem.Coverage01(ServiceKind.Police);
        }

        // Штраф за кражу: обязательное списание как содержание — казна может уйти в минус.
        private void FineTreasury(BuildingState home)
        {
            if (treasury != null) treasury.Charge(CrimeSystem.FinePerTheft);
        }

        private void OnCrimeEvent(EventKind kind, string message, int tick)
        {
            if (log != null) log.Add(kind, message, tick);
        }
    }
}
