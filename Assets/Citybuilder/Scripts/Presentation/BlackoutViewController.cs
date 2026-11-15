using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Вид блэкаута (EPIC-50): здание сменило питание — вид пересобирается (паттерн RebuildView
    // из EPIC-59), обесточенное получает тёмные материалы. Сейв не нужен: набор отключённых
    // восстанавливается пересчётом SupplyBalance. Сцена: объект с SettlementGrowthController.
    public class BlackoutViewController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Выросшие здания и вид. Обязателен: даёт Homes, View и GridVisualizer.")]
        public SettlementGrowthController Growth;
        [Tooltip("Электросеть. Обязателен: даёт IsHomePowered.")]
        public PowerController Power;

        [Header("Refresh")]
        [Tooltip("Пауза между проверками в секундах. Минимум 0.5.")]
        public float RefreshIntervalSec = 2f;

        private readonly List<EntityId> unpowered = new List<EntityId>();
        private float timer;

        private void Start()
        {
            ClampInterval();
            ResolveRefs();
            if (!EnsureReady()) return;
            RefreshNow();
        }

        private void Update()
        {
            if (!EnsureReady()) return;
            timer += Time.deltaTime;
            if (timer < RefreshIntervalSec) return;
            timer = 0f;
            RefreshNow();
        }

        // Обновить набор обесточенных и пересобрать только сменившие статус виды.
        public void RefreshNow()
        {
            List<EntityId> fresh = new List<EntityId>();
            List<BuildingState> homes = Growth.Homes;
            for (int i = 0; i < homes.Count; i++) ApplyStatus(homes[i], fresh);
            unpowered.Clear();
            unpowered.AddRange(fresh);
        }

        private void ApplyStatus(BuildingState home, List<EntityId> fresh)
        {
            if (home == null || home.Definition == null) return;
            if (Power.IsHomePowered(home))
            {
                if (unpowered.Contains(home.Id)) RebuildView(home, true);
                return;
            }
            fresh.Add(home.Id);
            if (!unpowered.Contains(home.Id)) RebuildView(home, false);
        }

        // Полная пересборка вида по статусу питания: AddBuilding сам снимает старый вид.
        private void RebuildView(BuildingState home, bool powered)
        {
            Growth.View.AddBuilding(home, home.Definition, Growth.GridVisualizer.Grid.CellSize, true, powered);
        }

        private void ResolveRefs()
        {
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Power == null) Power = GetComponent<PowerController>();
        }

        private bool EnsureReady()
        {
            if (Growth == null || Power == null)
            {
                Debug.LogError("BlackoutViewController: задайте Growth и Power в Inspector.");
                enabled = false;
                return false;
            }
            // Вид мог ещё не собраться у Growth — мягко ждём следующего кадра.
            return Growth.View != null && Growth.GridVisualizer != null;
        }

        private void ClampInterval()
        {
            if (RefreshIntervalSec < 0.5f) RefreshIntervalSec = 0.5f;
        }
    }
}
