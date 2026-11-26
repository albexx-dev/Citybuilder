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
        [Tooltip("Камера отсечения. Пусто = Camera.main.")]
        public Camera ViewCamera;

        [Header("Pool")]
        [Tooltip("Размер пула видов (1..40). При лимите 120 видятся все видимые агенты ADR-011.")]
        public int MaxViews = AgentManager.MaxAgents;
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

        [Header("Visibility")]
        [Tooltip("Дальность в клетках: не менее диагонали карты и высоты камеры при зуме.")]
        public float ViewDistanceCells = 72f;
        [Tooltip("Запас по краю пирамиды в клетках, чтобы машины не дёргались.")]
        public float FrustumMarginCells = 2f;

        private GameObject[] cars;
        private readonly VehicleVisibilityPolicy visibility = new VehicleVisibilityPolicy();
        private Vector3 position, angles;
        private Quaternion rotation;
        // Замер ADR-001: машины, прошедшие и отсеянные в последнем Sync.
        public int LastVisibleCars { get { return visibility.VisibleCount; } }
        public int LastHiddenCars { get { return visibility.HiddenCount; } }
        // Обновить все виды из менеджера. Вызывается хостом каждый кадр после Advance.
        public void Sync(AgentManager manager)
        {
            if (manager == null || Visualizer == null) return;
            EnsurePool();
            VehicleVisibilityPolicy policy = PrepareVisibility();
            for (int i = 0; i < cars.Length; i++) ShowCar(i, manager.AgentAt(i), policy);
        }

        // Политика кадра без аллокаций: параметры камеры копируются в переиспользуемый объект.
        private VehicleVisibilityPolicy PrepareVisibility()
        {
            Camera camera = ViewCamera != null ? ViewCamera : Camera.main;
            if (camera == null) { visibility.Configure(0f, 0f, 0f, 0f); return null; }
            float cell = Mathf.Max(0.01f, Visualizer.Grid.CellSize);
            Transform t = camera.transform;
            Vector3 p = t.position, f = t.forward, r = t.right, u = t.up;
            float distance = Mathf.Max(ViewDistanceCells * cell, Mathf.Max(0f, p.y) + 1f);
            visibility.Configure(camera.fieldOfView, camera.aspect, Mathf.Min(camera.farClipPlane, distance), FrustumMarginCells * cell);
            visibility.SetCamera(p.x, p.y, p.z, f.x, f.y, f.z, r.x, r.y, r.z, u.x, u.y, u.z);
            return visibility;
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
        private void ShowCar(int index, AgentManager.VehicleAgent agent, VehicleVisibilityPolicy policy)
        {
            GameObject car = cars[index];
            if (agent.Active && agent.Path != null && agent.Path.Count >= 2)
                PlaceCar(car, agent, policy);
            else car.SetActive(false);
        }

        // Интерполяция позиции и поворота; отсечение по политике кадра до записи в Transform.
        private void PlaceCar(GameObject car, AgentManager.VehicleAgent agent, VehicleVisibilityPolicy policy)
        {
            List<GridPosition> path = agent.Path;
            int last = path.Count - 1;
            int index = Mathf.Clamp((int)agent.Progress, 0, last);
            int next = index < last ? index + 1 : last;
            float t = index < last ? agent.Progress - index : 0f;
            float ax, az, bx, bz;
            Visualizer.Grid.GridToWorld(path[index], out ax, out az);
            Visualizer.Grid.GridToWorld(path[next], out bx, out bz);
            float x = Mathf.Lerp(ax, bx, t);
            float z = Mathf.Lerp(az, bz, t);
            if (policy != null && !policy.IsVisible(x, RideHeight, z))
            {
                car.SetActive(false);
                return;
            }
            position.Set(x, RideHeight, z);
            car.transform.position = position;
            float dx = bx - ax;
            float dz = bz - az;
            if (Mathf.Abs(dx) + Mathf.Abs(dz) > 0.001f)
            {
                angles.Set(0f, Mathf.Atan2(dx, dz) * Mathf.Rad2Deg + ModelYawOffset, 0f);
                rotation.eulerAngles = angles;
                car.transform.rotation = rotation;
            }
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
