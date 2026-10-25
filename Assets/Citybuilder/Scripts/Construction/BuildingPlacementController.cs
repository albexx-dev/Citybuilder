using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Ставит здания мышью: призрак, ЛКМ, Escape, ПКМ. Только старый Input.
    public class BuildingPlacementController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Визуализатор сетки. Обязателен: даёт Grid и клетку под курсором.")]
        public GridVisualizer GridVisualizer;
        [Tooltip("Что строить. Обычно House 2x2.")]
        public BuildingDefinition Definition;
        [Tooltip("Казна для списания стоимости. Пусто = строить бесплатно.")]
        public TreasuryView TreasuryView;

        private BuildingSystem system;
        private GameObject ghost;
        private Material validMaterial;
        private Material invalidMaterial;
        private bool placementActive = true;
        private readonly List<BuildingState> states = new List<BuildingState>();
        private readonly List<GameObject> views = new List<GameObject>();

        // Копия построенного: нужна покрытию и будущим сохранениям.
        public List<BuildingState> Placed
        {
            get { return new List<BuildingState>(states); }
        }

        private void Start()
        {
            EnsureReady();
        }

        // Прячем призрак при выключении: иначе висящий куб.
        private void OnDisable()
        {
            if (ghost != null) ghost.SetActive(false);
        }

        // Сменить тип и пересоздать призрак: иначе останется старый габарит.
        public void SetDefinition(BuildingDefinition def)
        {
            if (def == null || def == Definition) return;
            Definition = def;
            RebuildGhost();
        }

        // Проверяет ссылки и восстанавливает систему после перезагрузки домена.
        // Список построенного при перезагрузке теряется: снос старых зданий чинится сохранениями (не V1).
        private bool EnsureReady()
        {
            if (GridVisualizer == null || Definition == null)
            {
                Debug.LogError("BuildingPlacementController: задайте GridVisualizer и Definition в Inspector.");
                enabled = false;
                return false;
            }
            if (system == null) system = new BuildingSystem(GridVisualizer.Grid);
            if (validMaterial == null || invalidMaterial == null)
            {
                validMaterial = BuildingViewFactory.CreateGhostMaterial(new Color(0f, 1f, 0f, 0.4f));
                invalidMaterial = BuildingViewFactory.CreateGhostMaterial(new Color(1f, 0f, 0f, 0.4f));
            }
            if (ghost == null) RebuildGhost();
            return true;
        }

        private void RebuildGhost()
        {
            if (ghost != null)
            {
                ghost.SetActive(false);
                Destroy(ghost);
            }
            ghost = BuildingViewFactory.CreateGhost(Definition, GridVisualizer, transform);
            if (ghost != null) ghost.SetActive(false);
        }

        private void Update()
        {
            if (!EnsureReady()) return;
            if (system == null || !placementActive) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                placementActive = false;
                ghost.SetActive(false);
                return;
            }
            GridPosition cell;
            if (!GridVisualizer.TryGetHoveredCell(out cell))
            {
                ghost.SetActive(false);
                return;
            }
            ShowGhost(cell);
            if (Input.GetMouseButtonDown(0) && !UiClickGuard.IsOverMenu(Input.mousePosition))
            {
                TryPlaceAt(cell);
            }
            if (Input.GetMouseButtonDown(1) && !UiClickGuard.IsOverMenu(Input.mousePosition))
            {
                TryDemolish(cell);
            }
        }

        private void ShowGhost(GridPosition cell)
        {
            ghost.transform.position = FootprintCenter(Definition, cell, 0.5f);
            MeshRenderer renderer = ghost.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = system.CanPlace(Definition, cell) ? validMaterial : invalidMaterial;
            }
            ghost.SetActive(true);
        }

        // Публичная обёртка над постройкой: для E2E и будущих сохранений.
        public bool TryPlaceAt(GridPosition cell)
        {
            if (!EnsureReady()) return false;
            if (!system.CanPlace(Definition, cell)) return false;
            if (TreasuryView != null)
            {
                Treasury.TransactionResult spent = TreasuryView.Treasury.TrySpend(Definition.ConstructionCost, Definition.Id);
                if (!spent.Success) return false;
            }
            BuildingState state = system.Place(Definition, cell);
            if (state == null) return false;
            states.Add(state);
            views.Add(SpawnView(state));
            GridVisualizer.RefreshOccupied();
            return true;
        }

        private void TryDemolish(GridPosition cell)
        {
            int index = FindStateIndex(cell);
            if (index < 0) return;
            system.Demolish(states[index]);
            Destroy(views[index]);
            states.RemoveAt(index);
            views.RemoveAt(index);
            GridVisualizer.RefreshOccupied();
        }

        private int FindStateIndex(GridPosition cell)
        {
            for (int i = 0; i < states.Count; i++)
            {
                List<GridPosition> cells = states[i].Cells;
                for (int c = 0; c < cells.Count; c++)
                {
                    if (cells[c] == cell) return i;
                }
            }
            return -1;
        }

        private Vector3 FootprintCenter(BuildingDefinition def, GridPosition origin, float height)
        {
            return BuildingViewFactory.FootprintCenter(def, origin, height, GridVisualizer);
        }

        private GameObject SpawnView(BuildingState state)
        {
            return BuildingViewFactory.CreateView(state, GridVisualizer);
        }
    }
}
