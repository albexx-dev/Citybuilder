using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Виды машин-агентов (ADR-011, EPIC-44): пул примитивных автомобилей без коллайдеров.
    // Вид [i] привязан к слоту агента [i]; неактивные и сверх лимита — скрыты (#62).
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

        private static readonly Color BodyColor = new Color(0.45f, 0.08f, 0.08f);
        private static readonly Color CabinColor = new Color(0.30f, 0.05f, 0.05f);

        private GameObject[] cars;
        private Material bodyMaterial;
        private Material cabinMaterial;

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
            bodyMaterial = BuildMaterial(BodyColor);
            cabinMaterial = BuildMaterial(CabinColor);
            for (int i = 0; i < count; i++) cars[i] = BuildCar();
        }

        private void ClearPool()
        {
            if (cars == null) return;
            for (int i = 0; i < cars.Length; i++) DestroySafe(cars[i]);
            cars = null;
        }

        // Временная модель: корпус + кабина из кубов (замена на Kenney — эпик #60).
        private GameObject BuildCar()
        {
            GameObject car = new GameObject("Vehicle");
            car.transform.SetParent(transform, false);
            BuildPart(car, "Body", new Vector3(0.45f, 0.2f, 0.85f), new Vector3(0f, 0.1f, 0f), bodyMaterial);
            BuildPart(car, "Cabin", new Vector3(0.35f, 0.16f, 0.4f), new Vector3(0f, 0.28f, -0.08f), cabinMaterial);
            car.SetActive(false);
            return car;
        }

        private static void BuildPart(GameObject car, string partName, Vector3 size, Vector3 at, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = partName;
            DestroySafe(part.GetComponent<Collider>());
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
            part.transform.SetParent(car.transform, false);
            part.transform.localScale = size;
            part.transform.localPosition = at;
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
            if (Mathf.Abs(dx) + Mathf.Abs(dz) > 0.001f) car.transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(dx, dz) * Mathf.Rad2Deg, 0f);
            car.SetActive(true);
        }

        private static Material BuildMaterial(Color color) => new Material(Shader.Find("Unlit/Color")) { color = color };

        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        private void OnDestroy()
        {
            ClearPool();
            DestroySafe(bodyMaterial);
            DestroySafe(cabinMaterial);
            bodyMaterial = null;
            cabinMaterial = null;
        }
    }
}
