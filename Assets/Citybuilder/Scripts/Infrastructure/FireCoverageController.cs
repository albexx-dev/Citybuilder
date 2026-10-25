using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Покрытие пожарных служб: источники — поставленные здания с радиусом службы,
    // точки спроса — выросшие дома. Считаем на ServiceCoverage. Настройка: объект Grid.
    public class FireCoverageController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Рост поселений. Обязателен: даёт Homes.")]
        public SettlementGrowthController Growth;
        [Tooltip("Ручная стройка. Обязательна: даёт поставленные станции.")]
        public BuildingPlacementController Placement;

        [Header("Refresh")]
        [Tooltip("Пауза между пересчётами в секундах. Минимум 0.5.")]
        public float RefreshIntervalSec = 2f;

        private readonly ServiceCoverage coverage = new ServiceCoverage();
        private float timer;

        // Станций в последнем снимке. Снимка нет — 0.
        public int Stations
        {
            get { return coverage.SourceCount; }
        }

        // Домов в последнем снимке. Снимка нет — 0.
        public int Homes
        {
            get { return coverage.PointCount; }
        }

        // Накрытых домов в последнем снимке. Снимка нет — 0.
        public int Covered
        {
            get { return coverage.CoveredPoints; }
        }

        // Доля накрытых домов 0..1. Снимка нет — 1.
        public float Rate01
        {
            get { return coverage.Coverage01; }
        }

        private void Start()
        {
            ClampSettings();
            if (!EnsureReady()) return;
            RefreshNow();
        }

        private void OnValidate()
        {
            ClampSettings();
        }

        private void Update()
        {
            if (!EnsureReady()) return;
            timer += Time.deltaTime;
            if (timer < RefreshIntervalSec) return;
            timer = 0f;
            RefreshNow();
        }

        // Пересчитать снимок сейчас. Clear каждый раз: снос станции или дома исчезает сам.
        public void RefreshNow()
        {
            coverage.Clear();
            if (Growth == null || Placement == null) return;
            AddStations();
            AddHomes();
        }

        // Поставленные вручную здания: источником считается только здание с радиусом больше нуля.
        private void AddStations()
        {
            List<BuildingState> placed = Placement.Placed;
            if (placed == null) return;
            for (int i = 0; i < placed.Count; i++)
            {
                AddStation(placed[i]);
            }
        }

        private void AddStation(BuildingState state)
        {
            if (state == null || state.Definition == null) return;
            int radius = state.Definition.ServiceRadius;
            if (radius <= 0) return;
            coverage.SetSource(state.Id, state.Origin, radius);
        }

        private void AddHomes()
        {
            List<BuildingState> grown = Growth.Homes;
            if (grown == null) return;
            for (int i = 0; i < grown.Count; i++)
            {
                BuildingState home = grown[i];
                if (home == null) continue;
                coverage.SetPoint(home.Id, home.Origin);
            }
        }

        // Без Growth и Placement гаснет с ошибкой: данные снимка взять негде.
        private bool EnsureReady()
        {
            if (Growth == null || Placement == null)
            {
                Debug.LogError("FireCoverageController: задайте Growth и Placement в Inspector.");
                enabled = false;
                return false;
            }
            return true;
        }

        private void ClampSettings()
        {
            if (RefreshIntervalSec < 0.5f) RefreshIntervalSec = 0.5f;
        }
    }
}
