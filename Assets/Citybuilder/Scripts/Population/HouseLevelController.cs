using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Драйвер уровней домов (EPIC-59): тик часов → комфорт дома → HouseLevelPolicy → уровень и вид.
    // Сцена: тот же объект, что SettlementGrowthController. Компонента нет — уровни не меняются.
    // Комфорт дома: среднее городских сетей (энергия/вода/мусор) и служб у самого дома.
    public class HouseLevelController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Выросшие дома и вид. Обязателен: даёт Homes, View и Runner с часами.")]
        public SettlementGrowthController Growth;
        [Tooltip("Службы города. Пусто = службы не входят в комфорт домов.")]
        public ServiceCoverageController Services;
        [Tooltip("Электросеть. Пусто = свет не входит в комфорт домов.")]
        public PowerController Power;
        [Tooltip("Водопровод. Пусто = вода не входит в комфорт домов.")]
        public WaterController Water;
        [Tooltip("Мусор. Пусто = мусор не входит в комфорт домов.")]
        public WasteController Waste;

        private readonly HouseLevelPolicy policy = new HouseLevelPolicy();

        private void OnEnable()
        {
            ResolveRefs();
            if (!EnsureReady() || Growth.Runner.Clock == null) return;
            Growth.Runner.Clock.TickCompleted += OnClockTick;
        }

        private void OnDisable()
        {
            if (Growth == null || Growth.Runner == null || Growth.Runner.Clock == null) return;
            Growth.Runner.Clock.TickCompleted -= OnClockTick;
        }

        // Один игровой час: комфорт каждого дома → policy → серия и уровень в состоянии дома.
        public void OnClockTick(SimulationTime time)
        {
            if (Growth == null) return;
            List<BuildingState> homes = Growth.Homes;
            for (int i = 0; i < homes.Count; i++) ApplyTick(homes[i]);
        }

        private void ApplyTick(BuildingState home)
        {
            // Уровень растёт только у жилых домов: службы апгрейдятся вручную (EPIC-51),
            // магазины и заводы уровней не имеют. Сломанное состояние пропускается.
            if (home == null || home.Definition == null || !HouseLevelPolicy.IsResidential(home.Definition)) return;
            HouseLevelPolicy.Verdict verdict = policy.Evaluate(home.Level, home.ComfortStreak, HomeComfort(home.Origin));
            home.ApplyHouseTick(verdict.Level, verdict.Streak);
            if (verdict.Action == HouseLevelPolicy.LevelAction.Hold) return;
            RebuildView(home);
        }

        // Дом подрос или деградировал: пересобрать вид по новому уровню (куб или префаб).
        private void RebuildView(BuildingState home)
        {
            if (Growth.View == null || Growth.GridVisualizer == null || home.Definition == null) return;
            Growth.View.AddBuilding(home, home.Definition, Growth.GridVisualizer.Grid.CellSize, true);
        }

        // Комфорт дома 0..1: среднее двух групп — городские сети и службы у конкретного дома.
        // Системы нет — её группа = 1 (не ограничивает), как у NeedProviders.
        private float HomeComfort(GridPosition origin)
        {
            float supply = SupplyComfort();
            float services = ServiceComfort(origin);
            return (supply + services) * 0.5f;
        }

        private float SupplyComfort()
        {
            int count = 0;
            float sum = 0f;
            if (Power != null) { sum += Power.Rate01; count++; }
            if (Water != null) { sum += Water.Rate01; count++; }
            if (Waste != null) { sum += Waste.Rate01; count++; }
            return count == 0 ? 1f : sum / count;
        }

        private float ServiceComfort(GridPosition origin)
        {
            if (Services == null) return 1f;
            ServiceBuildingSystem system = Services.System;
            float sum = system.Coverage01(ServiceKind.Fire, origin);
            sum += system.Coverage01(ServiceKind.Police, origin);
            sum += system.Coverage01(ServiceKind.Health, origin);
            sum += system.Coverage01(ServiceKind.Leisure, origin);
            return sum * 0.25f;
        }

        private void ResolveRefs()
        {
            if (Services == null) Services = GetComponent<ServiceCoverageController>();
            if (Power == null) Power = GetComponent<PowerController>();
            if (Water == null) Water = GetComponent<WaterController>();
            if (Waste == null) Waste = GetComponent<WasteController>();
        }

        private bool EnsureReady()
        {
            if (Growth == null || Growth.Runner == null)
            {
                Debug.LogError("HouseLevelController: задайте Growth (с Runner) в Inspector.");
                enabled = false;
                return false;
            }
            return true;
        }
    }
}
