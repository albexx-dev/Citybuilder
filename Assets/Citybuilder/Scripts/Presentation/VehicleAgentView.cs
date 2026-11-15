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
        public int MaxViews = 24;
        [Tooltip("Высота корпуса над дорогой, метры.")]
        public float RideHeight = 0.05f;

        [Header("Model")]
        [Tooltip("FBX-модели машин (Kenney Racing). Пусто = машины без вида.")]
        public GameObject[] CarModels;
        [Tooltip("Доворот модели вокруг Y, градусы. Нос Kenney-машины смотрит по −Z: 180 выравнивает по ходу.")]
        public float ModelYawOffset = 0f;
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
                model.transform.localScale = model.transform.localScale * ModelScale;
                AlignModel(model);
            }
            car.SetActive(false);
            return car;
        }

        // Kenney-ассет несёт сдвиг внутри FBX: центр по XZ и низ по Y выравниваем на ось вида.
        private static void AlignModel(GameObject model)
        {
            Vector3 min, max;
            if (!TryModelBounds(model, out min, out max)) return;
            Vector3 anchor = new Vector3((min.x + max.x) * 0.5f, min.y, (min.z + max.z) * 0.5f);
            model.transform.localPosition = Vector3.Scale(-anchor, model.transform.localScale);
        }

        // Объединённый AABB всех мешей модели в системе координат её корня.
        // bounds доступны и без Read/Write у меша, поэтому замер не требует настроек импорта.
        private static bool TryModelBounds(GameObject model, out Vector3 min, out Vector3 max)
        {
            min = Vector3.positiveInfinity;
            max = Vector3.negativeInfinity;
            MeshFilter[] filters = model.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; i++)
            {
                Mesh mesh = filters[i].sharedMesh;
                if (mesh == null) continue;
                Matrix4x4 toRoot = model.transform.worldToLocalMatrix * filters[i].transform.localToWorldMatrix;
                Vector3 center = mesh.bounds.center;
                Vector3 ext = mesh.bounds.extents;
                for (int c = 0; c < 8; c++)
                {
                    Vector3 corner = center + Vector3.Scale(ext, new Vector3((c & 1) * 2 - 1, (c & 2) - 1, (c & 4) / 2 - 1));
                    corner = toRoot.MultiplyPoint3x4(corner);
                    min = Vector3.Min(min, corner);
                    max = Vector3.Max(max, corner);
                }
            }
            return min.x <= max.x;
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
            car.transform.localScale = Vector3.one * (cellSize * 0.5f);
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
