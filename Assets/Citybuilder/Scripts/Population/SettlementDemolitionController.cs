using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Снос выросших домов: ПКМ по дому и очистка всех. Только старый Input.
    // Ростом владеет SettlementGrowthController на том же объекте.
    public class SettlementDemolitionController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязательна: даёт клетку под курсором.")] public GridVisualizer GridVisualizer;
        [Tooltip("Рост поселений. Обязателен: владеет списком выросших домов.")] public SettlementGrowthController Growth;
        [Tooltip("Вид выросших домов. Пусто = ищется на этом же объекте.")] public SettlementView View;
        [Tooltip("Население. Опционален: при сносе выселяет жителей дома.")] public PopulationController PopulationSource;

        // Только ручной ввод: снос по ПКМ. Рост идёт по тикам часов в Growth, не по кадрам.
        private void Update()
        {
            if (!EnsureReady()) return;
            HandleDemolishInput();
        }
        private bool EnsureReady()
        {
            if (GridVisualizer == null || Growth == null)
            {
                Debug.LogError("SettlementDemolitionController: задайте GridVisualizer и Growth.");
                enabled = false;
                return false;
            }
            if (View == null) View = GetComponent<SettlementView>();
            return true;
        }
        private void HandleDemolishInput()
        {
            if (!Input.GetMouseButtonDown(1) || UiClickGuard.IsOverMenu(Input.mousePosition)) return;
            GridPosition cell;
            if (!GridVisualizer.TryGetHoveredCell(out cell)) return;
            TryDemolish(cell);
        }
        private void TryDemolish(GridPosition cell)
        {
            BuildingState state = Growth.FindStateAt(cell);
            if (state == null) return;
            DemolishOne(state);
        }
        // Снести все выросшие дома: выселение, вид и занятость — порядком TryDemolish.
        public void ClearGrown()
        {
            if (Growth == null) return;
            List<BuildingState> homes = Growth.Homes;
            for (int i = homes.Count - 1; i >= 0; i--)
            {
                DemolishOne(homes[i]);
            }
        }
        private void DemolishOne(BuildingState state)
        {
            if (PopulationSource != null && PopulationSource.Population != null) PopulationSource.Population.EvictHome(state);
            Growth.DemolishState(state);
        }
    }
}
