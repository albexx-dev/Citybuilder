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

        private void Start()
        {
            EnsureReady();
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
            if (system == null)
            {
                system = new BuildingSystem(GridVisualizer.Grid);
            }
            if (validMaterial == null || invalidMaterial == null)
            {
                validMaterial = BuildGhostMaterial(new Color(0f, 1f, 0f, 0.4f));
                invalidMaterial = BuildGhostMaterial(new Color(1f, 0f, 0f, 0.4f));
            }
            if (ghost == null)
            {
                ghost = transform.Find("Ghost") != null ? transform.Find("Ghost").gameObject : BuildGhost();
                ghost.SetActive(false);
            }
            return true;
        }

        private void Update()
        {
            if (!EnsureReady())
            {
                return;
            }
            if (system == null || !placementActive)
            {
                return;
            }
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
            if (Input.GetMouseButtonDown(0))
            {
                TryPlace(cell);
            }
            if (Input.GetMouseButtonDown(1))
            {
                TryDemolish(cell);
            }
        }

        private void ShowGhost(GridPosition cell)
        {
            ghost.transform.position = FootprintCenter(cell, 0.5f);
            MeshRenderer renderer = ghost.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = system.CanPlace(Definition, cell) ? validMaterial : invalidMaterial;
            ghost.SetActive(true);
        }

        private void TryPlace(GridPosition cell)
        {
            if (TreasuryView != null)
            {
                if (!system.CanPlace(Definition, cell)) return;
                Treasury.TransactionResult spent = TreasuryView.Treasury.TrySpend(Definition.ConstructionCost, Definition.Id);
                if (!spent.Success) return;
            }
            BuildingState state = system.Place(Definition, cell);
            if (state == null) return;
            states.Add(state);
            views.Add(SpawnView(state));
            GridVisualizer.RefreshOccupied();
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
                    if (cells[c] == cell)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private Vector3 FootprintCenter(GridPosition origin, float height)
        {
            float worldX, worldZ, cellSize = GridVisualizer.Grid.CellSize;
            GridVisualizer.Grid.GridToWorld(origin, out worldX, out worldZ);
            float centerX = worldX + (Definition.Footprint.x - 1) * cellSize * 0.5f;
            float centerZ = worldZ + (Definition.Footprint.y - 1) * cellSize * 0.5f;
            return new Vector3(centerX, height, centerZ);
        }

        private GameObject SpawnView(BuildingState state)
        {
            Vector3 position = FootprintCenter(state.Origin, 0f);
            if (Definition.Prefab != null)
            {
                return Instantiate(Definition.Prefab, position, Quaternion.identity);
            }
            float cellSize = GridVisualizer.Grid.CellSize;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(Definition.Footprint.x * cellSize, 1f, Definition.Footprint.y * cellSize);
            cube.transform.position = FootprintCenter(state.Origin, 0.5f);
            return cube;
        }

        private GameObject BuildGhost()
        {
            float cellSize = GridVisualizer.Grid.CellSize;
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Ghost";
            go.transform.SetParent(transform, false);
            Destroy(go.GetComponent<Collider>());
            go.transform.localScale = new Vector3(Definition.Footprint.x * cellSize, 1f, Definition.Footprint.y * cellSize);
            return go;
        }

        private Material BuildGhostMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Unlit/Color"));
            material.color = color;
            return material;
        }
    }
}
