using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Виды машин-агентов (ADR-011, EPIC-44): пул Kenney-машинок без коллайдеров (EPIC-60).
    // Вид [i] привязан к слоту агента [i]; вариант модели детерминирован слотом.
    public class VehicleAgentView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка: размер клетки и мировые координаты. Обязателен.")]
        public GridVisualizer Visualizer;

        [Header("Pool")]
        [Tooltip("Максимум видимых машин (1..40). Агенты сверх лимита ездят без вида.")]
        public int MaxViews = 12;
        [Tooltip("Высота корпуса над дорогой, метры.")]
        public float RideHeight = 0.05f;
        [Tooltip("Целевая длина машины в клетках: авто-фит масштаба по AABB модели.")]
        public const float CarLengthCells = 1.6f;

        [Header("Model")]
        [Tooltip("FBX-модели машин (Kenney Racing). Пусто = машины без вида.")]
        public GameObject[] CarModels;
        [Tooltip("Доворот модели вокруг Y, градусы. Нос Kenney-машины смотрит по −Z: 180 выравнивает по ходу.")]
        public float ModelYawOffset = 180f;
        [Tooltip("Множитель масштаба поверх родного масштаба FBX (машина ~13.5 м: при CellSize 1 попробуйте 0.06).")]
        public float ModelScale = 1f;

        private GameObject[] cars;

        // Обновить все виды из менеджера. Вызывается хостом каждый кадр после Advance.
        public void Sync(AgentManager manager)
        {
            if (manager == null || Visualizer == null) return;
            EnsurePool();
            float cellSize = Visualizer.Grid.CellSize;
            for (int i = 0; i < cars.Length; i++) ShowCar(i, manager.AgentAt(i), cellSize);
        }

        private void EnsurePool()
        {
            int count = Mathf.Clamp(MaxViews, 1, AgentManager.MaxAgents);
            if (cars != null && cars.Length == count) return;
            ClearPool();
            cars = new GameObject[count];
            if (CarModels == null || CarModels.Length == 0)
                Debug.LogError("VehicleAgentView: задайте CarModels (FBX Kenney Racing) в Inspector.");
            for (int i = 0; i < count; i++) cars[i] = BuildCar(i);
        }

        private void ClearPool()
        {
            if (cars == null) return;
            for (int i = 0; i < cars.Length; i++) DestroySafe(cars[i]);
            cars = null;
        }

        // Вид слота: инстанс FBX-модели, вариант = slot % CarModels.Length (стабилен между кадрами).
        private GameObject BuildCar(int slot)
        {
            GameObject car = new GameObject("Vehicle");
            car.transform.SetParent(transform, false);
            if (CarModels != null && CarModels.Length > 0 && CarModels[slot % CarModels.Length] != null)
            {
                GameObject model = Instantiate(CarModels[slot % CarModels.Length]);
                model.transform.SetParent(car.transform, false);
                // Авто-фит под клетку (длина ~1.6 клетки), ModelScale — ручной множитель поверх.
                float target = Visualizer.Grid.CellSize * CarLengthCells * Mathf.Max(0.01f, ModelScale);
                VehicleModelUtils.Fit(model, target);
            }
            car.SetActive(false);
            return car;
        }

        private void ShowCar(int index, AgentManager.VehicleAgent agent, float cellSize)
        {
            GameObject car = cars[index];
            if (agent.Active && agent.Path != null && agent.Path.Count >= 2) PlaceCar(car, agent, cellSize);
            else car.SetActive(false);
        }

        // Интерполяция позиции и поворота по прогрессу агента между клетками.
        private void PlaceCar(GameObject car, AgentManager.VehicleAgent agent, float cellSize)
        {
            List<GridPosition> path = agent.Path;
            int last = path.Count - 1;
            int index = (int)agent.Progress;
            if (index < 0) index = 0;
            if (index > last) index = last;
            int next = index < last ? index + 1 : last;
            float t = index < last ? agent.Progress - index : 0f;
            float ax, az, bx, bz;
            Visualizer.Grid.GridToWorld(path[index], out ax, out az);
            Visualizer.Grid.GridToWorld(path[next], out bx, out bz);
            car.transform.position = new Vector3(Mathf.Lerp(ax, bx, t), RideHeight, Mathf.Lerp(az, bz, t));
            float dx = bx - ax;
            float dz = bz - az;
            if (Mathf.Abs(dx) + Mathf.Abs(dz) > 0.001f)
                car.transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(dx, dz) * Mathf.Rad2Deg + ModelYawOffset, 0f);
            car.SetActive(true);
        }

        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        private void OnDestroy()
        {
            ClearPool();
        }
    }
}
