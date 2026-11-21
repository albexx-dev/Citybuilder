using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Декор на свободных клетках (EPIC-56, view-слой): домен о нём не знает.
    // Пересчёт раз в RefreshInterval по занятости и зонам; пул — без аллокаций.
    public class DecorManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка: размеры, занятость, мировые координаты. Обязателен.")]
        public GridVisualizer Visualizer;
        [Tooltip("Кисть зон. Пусто = фильтр только по занятости клеток.")]
        public ZonePaintingController ZonePainter;

        [Header("Decor")]
        [Tooltip("FBX/префабы декора (Kenney Nature). Пусто = декор выключен.")]
        public GameObject[] DecorPrefabs;
        [Tooltip("Шанс декора на свободной клетке, 0..1.")]
        public float Chance = DecorSeedPolicy.DefaultChance;
        [Tooltip("Интервал пересчёта декора, секунды.")]
        public float RefreshInterval = 0.5f;
        [Tooltip("Множитель масштаба поверх родного масштаба модели.")]
        public float DecorScale = 1f;

        private readonly Dictionary<GridPosition, GameObject> active = new Dictionary<GridPosition, GameObject>();
        private readonly Dictionary<GridPosition, Stack<GameObject>> origins = new Dictionary<GridPosition, Stack<GameObject>>();
        private readonly Dictionary<int, Stack<GameObject>> pools = new Dictionary<int, Stack<GameObject>>();
        private Transform decorRoot;
        private float timer;
        private bool decorVisible = true;

        private void Start()
        {
            if (Ready() && decorVisible) RefreshDecor();
        }

        private void Update()
        {
            if (!Ready() || !decorVisible) return;
            timer += Time.deltaTime;
            if (timer < RefreshInterval) return;
            timer = 0f;
            RefreshDecor();
        }

        private void OnDisable()
        {
            List<GridPosition> cells = new List<GridPosition>(active.Keys);
            for (int i = 0; i < cells.Count; i++) Release(cells[i]);
        }

        // Настройка DecorEnabled (EPIC-69 шаг 3): выкл прячет весь декор и останавливает
        // пересчёт, вкл показывает корень и сразу пересчитывает клетки (смена видна на паузе).
        public bool DecorVisible
        {
            get { return decorVisible; }
        }

        public void SetDecorVisible(bool visible)
        {
            if (decorVisible == visible) return;
            decorVisible = visible;
            if (decorRoot != null) decorRoot.gameObject.SetActive(visible);
            if (visible && Ready()) RefreshDecor();
        }

        // Полный пересчёт: спавн на свободных, уборка на занятых/зонированных.
        private void RefreshDecor()
        {
            GridSize size = Visualizer.Grid.Size;
            for (int x = 0; x < size.Width; x++)
            {
                for (int z = 0; z < size.Height; z++)
                {
                    GridPosition cell = new GridPosition(x, z);
                    if (ShouldDecorate(cell)) EnsureDecorated(cell);
                    else Release(cell);
                }
            }
        }

        private bool ShouldDecorate(GridPosition cell)
        {
            if (DecorPrefabs == null || DecorPrefabs.Length == 0) return false;
            if (Visualizer.Grid.IsOccupied(cell)) return false;
            if (HasZone(cell)) return false;
            return DecorSeedPolicy.HasDecor(cell.X, cell.Y, Chance);
        }

        // Зоны читаются через публичный ZonePaintingController.Zones (ZoneSystem).
        private bool HasZone(GridPosition cell)
        {
            if (ZonePainter == null) return false;
            ZoneSystem zones = ZonePainter.Zones;
            return zones != null && zones.GetZone(cell) != ZoneType.None;
        }

        private void EnsureDecorated(GridPosition cell)
        {
            if (active.ContainsKey(cell)) return;
            int variant = DecorSeedPolicy.VariantIndex(cell.X, cell.Y, DecorPrefabs.Length);
            GameObject prefab = DecorPrefabs[variant];
            if (prefab == null) return;
            Stack<GameObject> pool = PoolFor(variant);
            GameObject decor = pool.Count > 0 ? pool.Pop() : InstantiateDecor(prefab);
            Place(decor, cell);
            active[cell] = decor;
            origins[cell] = pool;
        }

        private void Release(GridPosition cell)
        {
            GameObject decor;
            if (!active.TryGetValue(cell, out decor)) return;
            active.Remove(cell);
            decor.SetActive(false);
            origins[cell].Push(decor);
            origins.Remove(cell);
        }

        private void Place(GameObject decor, GridPosition cell)
        {
            float worldX, worldZ;
            Visualizer.Grid.GridToWorld(cell, out worldX, out worldZ);
            decor.transform.position = new Vector3(worldX, 0f, worldZ);
            float yaw = DecorSeedPolicy.YawStep(cell.X, cell.Y, 4) * 90f;
            decor.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            decor.transform.SetParent(DecorRoot(), false);
            decor.SetActive(true);
        }

        private GameObject InstantiateDecor(GameObject prefab)
        {
            GameObject decor = Instantiate(prefab);
            decor.transform.localScale = decor.transform.localScale * DecorScale;
            return decor;
        }

        private Stack<GameObject> PoolFor(int variant)
        {
            Stack<GameObject> pool;
            if (!pools.TryGetValue(variant, out pool)) pools[variant] = pool = new Stack<GameObject>();
            return pool;
        }

        private Transform DecorRoot()
        {
            if (decorRoot != null) return decorRoot;
            Transform found = transform.Find("DecorRoot");
            decorRoot = found != null ? found : new GameObject("DecorRoot").transform;
            decorRoot.SetParent(transform, false);
            return decorRoot;
        }

        private bool Ready()
        {
            if (Visualizer != null) return true;
            Visualizer = GetComponent<GridVisualizer>();
            if (Visualizer != null) return true;
            Debug.LogError("DecorManager: задайте Visualizer в Inspector.");
            enabled = false;
            return false;
        }
    }
}
