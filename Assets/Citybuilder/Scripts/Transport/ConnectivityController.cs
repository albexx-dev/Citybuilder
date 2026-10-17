using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Инфо-связность домов с дорогами для HUD. Только чтение снимка, поведения ноль.
    // Настройка сцены: объект Grid, Store/Growth/GridVisualizer обязательны. RefreshNow — для загрузок.
    public class ConnectivityController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Дороги. Обязателен: даёт Roads для снимка.")]
        public RoadNetworkStore Store;
        [Tooltip("Рост поселений. Обязателен: даёт Homes для снимка.")]
        public SettlementGrowthController Growth;
        [Tooltip("Сетка. Обязателен: даёт Grid для системы.")]
        public GridVisualizer GridVisualizer;
        [Header("Refresh")]
        [Tooltip("Пауза между пересчётами в секундах. Минимум 0.5.")]
        public float RefreshIntervalSec = 2f;

        private ConnectivitySystem connectivity;
        private float timer;

        // Связанных клеток домов из последнего снимка. Системы нет — 0.
        public int Connected
        {
            get
            {
                EnsureSystem();
                if (connectivity == null) return 0;
                return connectivity.ConnectedCount;
            }
        }

        // Всего клеток домов из последнего снимка. Системы нет — 0.
        public int Total
        {
            get
            {
                EnsureSystem();
                if (connectivity == null) return 0;
                return connectivity.TotalCount;
            }
        }

        // Доля связанных 0..1. Системы нет — 0.
        public float Rate01
        {
            get
            {
                EnsureSystem();
                if (connectivity == null) return 0f;
                return connectivity.ConnectionRate01;
            }
        }

        // Дом связан, если хотя бы одна его клетка связана. Снимка нет — false.
        public bool IsHomeConnected(BuildingState home)
        {
            if (home == null || home.Cells == null) return false;
            EnsureSystem();
            if (connectivity == null) return false;
            for (int i = 0; i < home.Cells.Count; i++)
            {
                if (connectivity.IsConnected(home.Cells[i])) return true;
            }
            return false;
        }

        private void Start()
        {
            ClampInterval();
            if (!EnsureReady()) return;
            RefreshNow();
        }

        private void OnValidate()
        {
            ClampInterval();
        }

        private void Update()
        {
            if (!EnsureReady()) return;
            timer += Time.deltaTime;
            if (timer < RefreshIntervalSec) return;
            timer = 0f;
            RefreshNow();
        }

        // Пересчитать снимок сейчас (загрузки/мост). Ссылок нет — тихо пропуск.
        public void RefreshNow()
        {
            EnsureSystem();
            if (connectivity == null) return;
            if (Store == null || Growth == null) return;
            connectivity.Rebuild(CollectRoadCells(), CollectBuildingCells());
        }

        // Чинит ссылки после перезагрузки домена. Без ссылок гаснет с ошибкой.
        private bool EnsureReady()
        {
            if (Store == null || Growth == null || GridVisualizer == null)
            {
                Debug.LogError("ConnectivityController: задайте Store, Growth и GridVisualizer в Inspector.");
                enabled = false;
                return false;
            }
            return true;
        }

        // Тихое создание поверх живой сетки: вид опрашивает каждый кадр.
        private void EnsureSystem()
        {
            if (connectivity != null) return;
            if (GridVisualizer == null) return;
            connectivity = new ConnectivitySystem(GridVisualizer.Grid);
        }

        // Клетки дорог из стора. Стора нет — пустой набор.
        private List<GridPosition> CollectRoadCells()
        {
            List<GridPosition> cells = new List<GridPosition>();
            List<RoadState> roads = Store.Roads;
            if (roads == null) return cells;
            for (int i = 0; i < roads.Count; i++)
            {
                if (roads[i] == null) continue;
                cells.Add(roads[i].Cell);
            }
            return cells;
        }

        // Все занятые клетки выросших домов. Роста нет — пустой набор.
        private List<GridPosition> CollectBuildingCells()
        {
            List<GridPosition> cells = new List<GridPosition>();
            List<BuildingState> homes = Growth.Homes;
            if (homes == null) return cells;
            for (int i = 0; i < homes.Count; i++)
            {
                if (homes[i] == null) continue;
                List<GridPosition> occupied = homes[i].Cells;
                if (occupied == null) continue;
                for (int c = 0; c < occupied.Count; c++) cells.Add(occupied[c]);
            }
            return cells;
        }

        private void ClampInterval()
        {
            if (RefreshIntervalSec < 0.5f) RefreshIntervalSec = 0.5f;
        }
    }
}
