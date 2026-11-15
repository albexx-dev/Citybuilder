using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Ручной декор (EPIC-56 «Декор [5]»): деревья на клетках, домен не трогаем, клетки не занимают.
    // ЛКМ — поставить (замена), ПКМ — снести, Esc — выйти; раз в RefreshInterval уборка из-под зданий и зон.
    public class DecorPlacementController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Визуализатор сетки. Обязателен: даёт Grid и клетку под курсором.")] public GridVisualizer GridVisualizer;
        [Tooltip("Кисть зон. Пусто = ищется на этом же объекте.")] public ZonePaintingController ZonePainter;
        [Tooltip("Виды декора для строки выбора. Пусто = ставить нечего.")] public List<DecorDefinition> Definitions = new List<DecorDefinition>();

        [Header("Decor")]
        [Tooltip("Интервал проверки занятости клеток, секунды.")] public float RefreshInterval = 0.5f;

        private readonly Dictionary<GridPosition, GameObject> placed = new Dictionary<GridPosition, GameObject>();
        private readonly DecorGhostView ghostView = new DecorGhostView();
        private Transform root;
        private float timer;
        public bool placementActive = true;

        // Выбранный вид и число деревьев: для строки декора, проверок и E2E-моста.
        public DecorDefinition Selected { get; private set; }
        public int PlacedCount { get { return placed.Count; } }

        public void SetSelected(DecorDefinition def) { Selected = def; }

        private void Start() { EnsureReady(); }
        // Вход в режим декора: сразу убираем декор, попавший под здания или зоны.
        private void OnEnable() { placementActive = true; if (GridVisualizer != null) SweepBlocked(); }
        // Прячем призрак при выключении: иначе висящий куб.
        private void OnDisable() { ghostView.Hide(); }

        private void Update()
        {
            if (!EnsureReady() || !placementActive) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                placementActive = false;
                ghostView.Hide();
                return;
            }
            GridPosition cell;
            if (!GridVisualizer.TryGetHoveredCell(out cell))
            {
                ghostView.Hide();
                return;
            }
            ghostView.Show(cell, CanPlaceAt(cell));
            if (UiClickGuard.IsOverMenu(Input.mousePosition)) return;
            if (Input.GetMouseButtonDown(0)) TryPlaceAt(cell);
            if (Input.GetMouseButtonDown(1)) TryRemoveAt(cell);
            timer += Time.deltaTime;
            if (timer >= RefreshInterval) { timer = 0f; SweepBlocked(); }
        }

        // Проверяет ссылки и восстанавливает призрак после перезагрузки домена.
        private bool EnsureReady()
        {
            if (GridVisualizer == null) GridVisualizer = GetComponent<GridVisualizer>();
            if (ZonePainter == null) ZonePainter = GetComponent<ZonePaintingController>();
            if (GridVisualizer != null)
            {
                if (Application.isPlaying) ghostView.Ensure(GridVisualizer, transform);
                return true;
            }
            Debug.LogError("DecorPlacementController: задайте GridVisualizer в Inspector.");
            enabled = false;
            return false;
        }

        // Свободная незонированная клетка внутри карты. Кисть зон недоступна — фильтр зон пропускается.
        public bool CanPlaceAt(GridPosition cell)
        {
            if (GridVisualizer == null) return false;
            if (!GridVisualizer.Grid.IsInside(cell) || GridVisualizer.Grid.IsOccupied(cell)) return false;
            if (ZonePainter == null) return true;
            ZoneSystem zones = ZonePainter.Zones;
            return zones == null || zones.GetZone(cell) == ZoneType.None;
        }

        // ЛКМ: поставить выбранный вид, заменив существующий декор на клетке.
        public bool TryPlaceAt(GridPosition cell)
        {
            if (Selected == null || !CanPlaceAt(cell)) return false;
            TryRemoveAt(cell);
            GameObject view = CreateView(Selected);
            if (view == null) return false;
            float worldX, worldZ;
            GridVisualizer.Grid.GridToWorld(cell, out worldX, out worldZ);
            view.transform.position = new Vector3(worldX, 0f, worldZ);
            view.transform.SetParent(DecorRoot(), false);
            placed[cell] = view;
            return true;
        }

        // ПКМ: снести декор с клетки. Здания и зоны не трогаем.
        public bool TryRemoveAt(GridPosition cell)
        {
            GameObject view;
            if (!placed.TryGetValue(cell, out view)) return false;
            placed.Remove(cell);
            BuildingViewFactory.DestroySafe(view);
            return true;
        }

        // Вид декора: префаб из определения или куб-заглушка. Коллайдеры убираем.
        private GameObject CreateView(DecorDefinition def)
        {
            GameObject view = def.Prefab != null ? Instantiate(def.Prefab) : GameObject.CreatePrimitive(PrimitiveType.Cube);
            Collider[] colliders = view.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++) BuildingViewFactory.DestroySafe(colliders[i]);
            return view;
        }

        // Периодическая уборка: здания декор не занимают, зонированные клетки тоже.
        public void SweepBlocked()
        {
            List<GridPosition> decorCells = new List<GridPosition>(placed.Keys);
            List<GridPosition> occupied = GridVisualizer.Grid.GetOccupiedCells();
            List<GridPosition> remove = DecorCleanupPolicy.CellsToRemove(decorCells, occupied, ZonedCells());
            for (int i = 0; i < remove.Count; i++) TryRemoveAt(remove[i]);
        }

        // Клетки зон из кисти. Пусто, если зонирование недоступно.
        private List<GridPosition> ZonedCells()
        {
            List<GridPosition> cells = new List<GridPosition>();
            if (ZonePainter == null) return cells;
            ZoneSystem zones = ZonePainter.Zones;
            if (zones == null) return cells;
            List<KeyValuePair<GridPosition, ZoneType>> zoned = zones.GetZonedCells();
            for (int i = 0; i < zoned.Count; i++) cells.Add(zoned[i].Key);
            return cells;
        }

        private Transform DecorRoot()
        {
            if (root != null) return root;
            Transform found = transform.Find("DecorRoot");
            root = found != null ? found : new GameObject("DecorRoot").transform;
            root.SetParent(transform, false);
            return root;
        }
    }
}
