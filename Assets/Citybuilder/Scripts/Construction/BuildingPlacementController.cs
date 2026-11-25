using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Ставит здания мышью: призрак, ЛКМ, ПКМ. Esc гасит режим через BuildModeHotkeys. Только старый Input.
    public class BuildingPlacementController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Визуализатор сетки. Обязателен: даёт Grid и клетку под курсором.")]
        public GridVisualizer GridVisualizer;
        [Tooltip("Что строить. Обычно House 2x2.")]
        public BuildingDefinition Definition;
        [Tooltip("Типы для восстановления из слота и палитры.")]
        public List<BuildingDefinition> Definitions = new List<BuildingDefinition>();
        [Tooltip("Казна для списания стоимости. Пусто = строить бесплатно.")]
        public TreasuryView TreasuryView;
        [Tooltip("Контроллер сохранения.")]
        public SaveController Save;

        private readonly ManualBuildingsStore store = new ManualBuildingsStore();
        private readonly PlacementGhostView ghostView = new PlacementGhostView();
        private BuildingSystem system;
        public bool placementActive = true;
        // PascalCase-обёртка для кода: поле serialized в сцене, переименование тронуло бы Prototype.unity.
        public bool PlacementActive { get { return placementActive; } set { placementActive = value; } }

        // Копия построенного: нужна покрытию, инфраструктуре и сохранениям.
        public List<BuildingState> Placed { get { return store.Placed; } }
        public int PlacedRevision { get; private set; }

        private void Start() { EnsureReady(); }
        private void OnEnable() { placementActive = true; }
        private void OnDisable() { ghostView.Hide(); }

        // Сменить тип и пересоздать призрак: иначе останется старый габарит.
        public void SetDefinition(BuildingDefinition def)
        {
            if (def == null || def == Definition) return;
            Definition = def;
            ghostView.SetDefinition(def);
        }

        // Проверяет ссылки и восстанавливает систему после перезагрузки домена.
        private bool EnsureReady()
        {
            if (GridVisualizer == null || Definition == null)
            {
                Debug.LogError("BuildingPlacementController: задайте GridVisualizer и Definition в Inspector.");
                enabled = false;
                return false;
            }
            if (Save == null) Save = FindFirstObjectByType<SaveController>();
            if (!EnsureSystem()) return false;
            if (Application.isPlaying) ghostView.Ensure(Definition, GridVisualizer, transform);
            return true;
        }

        private bool EnsureSystem()
        {
            if (GridVisualizer == null) return false;
            if (system == null) system = new BuildingSystem(GridVisualizer.Grid);
            return system != null;
        }

        private void Update()
        {
            if (!EnsureReady() || system == null || !placementActive) return;
            GridPosition cell;
            if (!GridVisualizer.TryGetHoveredCell(out cell))
            {
                ghostView.Hide();
                return;
            }
            ghostView.Show(cell, system.CanPlace(Definition, cell));
            if (UiClickGuard.IsOverMenu(Input.mousePosition)) return;
            if (Input.GetMouseButtonDown(0)) TryPlaceAt(cell);
            if (Input.GetMouseButtonDown(1)) TryDemolish(cell);
        }

        // Публичная обёртка над постройкой: для E2E и сохранений.
        public bool TryPlaceAt(GridPosition cell)
        {
            if (!EnsureReady()) return false;
            if (!system.CanPlace(Definition, cell)) return false;
            if (Definition.ConstructionCost > 0 && TreasuryView != null && TreasuryView.Treasury != null)
            {
                Treasury.TransactionResult spent = TreasuryView.Treasury.TrySpend(Definition.ConstructionCost, Definition.Id);
                if (!spent.Success) return false;
            }
            BuildingState state = system.Place(Definition, cell);
            if (state == null) return false;
            store.Add(state, BuildingViewFactory.CreateView(state, GridVisualizer, true,
                delegate { store.ReplaceWithReady(state, GridVisualizer); }));
            GridVisualizer.RefreshOccupied(); PlacedRevision++;
            if (Save != null) Save.MarkDirty();
            return true;
        }

        // Восстановление из слота: без списания денег.
        public bool RestoreManual(string defId, GridPosition origin)
        {
            if (string.IsNullOrEmpty(defId) || !EnsureSystem()) return false;
            BuildingDefinition def = FindDefinition(defId);
            if (def == null) return false;
            BuildingState state = system.Place(def, origin);
            if (state == null) return false;
            store.Add(state, BuildingViewFactory.CreateView(state, GridVisualizer, false, null));
            GridVisualizer.RefreshOccupied(); PlacedRevision++;
            return true;
        }

        // Новая игра и загрузка: ручные сносятся без выселения (V1 их не заселяет).
        public void ClearPlaced()
        {
            store.Clear(system, GridVisualizer); PlacedRevision++;
        }

        public bool TryDemolish(GridPosition cell)
        {
            BuildingState state = store.FindAt(cell);
            if (state == null) return false;
            system.Demolish(state);
            store.Demolish(state);
            GridVisualizer.RefreshOccupied(); PlacedRevision++;
            if (Save != null) Save.MarkDirty();
            return true;
        }

        private BuildingDefinition FindDefinition(string defId)
        {
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    BuildingDefinition def = Definitions[i];
                    if (def != null && def.Id == defId) return def;
                }
            }
            if (Definition != null && Definition.Id == defId) return Definition;
            return null;
        }
    }
}
