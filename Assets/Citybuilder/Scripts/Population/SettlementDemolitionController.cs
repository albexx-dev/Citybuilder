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
        [Tooltip("Меню режимов стройки. Пусто = ищется на этом же объекте.")] public BuildModeMenu ModeMenu;

        [Header("Modals")]
        [Tooltip("Меню паузы.")] public PauseMenuView PauseMenu;
        [Tooltip("Меню сохранения.")] public SaveMenuView SaveMenu;
        [Tooltip("Меню загрузки.")] public LoadMenuView LoadMenu;
        [Tooltip("Главное меню.")] public MainMenu MainMenu;

        public static bool IsDemolitionAllowed(BuildModeMenu.Mode mode, bool isModalOpen)
        {
            if (isModalOpen) return false;
            return mode == BuildModeMenu.Mode.Off || mode == BuildModeMenu.Mode.Growth;
        }

        public bool IsModeAllowed()
        {
            if (ModeMenu == null) return true;
            return ModeMenu.CurrentMode == BuildModeMenu.Mode.Off || ModeMenu.CurrentMode == BuildModeMenu.Mode.Growth;
        }

        public bool IsModalOpen()
        {
            if (PauseMenu != null && PauseMenu.IsOpen) return true;
            if (SaveMenu != null && SaveMenu.IsOpen) return true;
            if (LoadMenu != null && LoadMenu.IsOpen) return true;
            if (MainMenu != null && MainMenu.IsOpen) return true;
            return false;
        }

        public bool IsDemolitionAllowed()
        {
            return IsModeAllowed() && !IsModalOpen();
        }

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
            ResolvePopulation();
            ResolveMenuRefs();
            return true;
        }
        private void ResolvePopulation()
        {
            if (PopulationSource == null) PopulationSource = GetComponent<PopulationController>();
            if (PopulationSource == null) PopulationSource = FindFirstObjectByType<PopulationController>();
        }
        private void ResolveMenuRefs()
        {
            if (ModeMenu == null) ModeMenu = GetComponent<BuildModeMenu>();
            if (ModeMenu == null) ModeMenu = FindFirstObjectByType<BuildModeMenu>();
            if (PauseMenu == null) PauseMenu = GetComponent<PauseMenuView>();
            if (PauseMenu == null) PauseMenu = FindFirstObjectByType<PauseMenuView>();
            if (SaveMenu == null) SaveMenu = GetComponent<SaveMenuView>();
            if (SaveMenu == null) SaveMenu = FindFirstObjectByType<SaveMenuView>();
            if (LoadMenu == null) LoadMenu = GetComponent<LoadMenuView>();
            if (LoadMenu == null) LoadMenu = FindFirstObjectByType<LoadMenuView>();
            if (MainMenu == null) MainMenu = GetComponent<MainMenu>();
            if (MainMenu == null) MainMenu = FindFirstObjectByType<MainMenu>();
        }
        private void HandleDemolishInput()
        {
            if (!Input.GetMouseButtonDown(1) || !IsDemolitionAllowed()) return;
            if (UiClickGuard.IsOverMenu(Input.mousePosition)) return;
            GridPosition cell;
            if (!GridVisualizer.TryGetHoveredCell(out cell)) return;
            TryDemolish(cell);
        }
        public bool TryDemolish(GridPosition cell)
        {
            if (Growth == null) return false;
            ResolvePopulation();
            BuildingState state = Growth.FindStateAt(cell);
            if (state == null) return false;
            DemolishOne(state);
            return true;
        }
        // Снести все выросшие дома: выселение, вид и занятость — порядком TryDemolish.
        public void ClearGrown()
        {
            if (Growth == null) return;
            ResolvePopulation();
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
