using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Пул видов мусоровозов (EPIC-52): Kenney-модели по слотам GarbageTrucks. Обычный
    // класс без MonoBehaviour (владелец — GarbageTruckView), рисование по прецеденту
    // VehicleAgentView: интерполяция позиции и поворота по прогрессу между клетками.
    public class GarbageTruckPool
    {
        private readonly GameObject[] cars;
        private readonly float yawOffset;
        private readonly float rideHeight;

        public GarbageTruckPool(Transform parent, GameObject[] models, int count,
            float modelScale, float yawOffset, float rideHeight)
        {
            this.yawOffset = yawOffset;
            this.rideHeight = rideHeight;
            if (models == null || models.Length == 0)
                Debug.LogWarning("GarbageTruckView: задайте TruckModels (FBX Kenney Racing) в Inspector.");
            cars = new GameObject[count];
            for (int i = 0; i < count; i++) cars[i] = BuildCar(parent, models, i, modelScale);
        }

        // Вид слота: инстанс FBX-модели, вариант = slot % models.Length (стабилен между кадрами).
        private GameObject BuildCar(Transform parent, GameObject[] models, int slot, float modelScale)
        {
            GameObject car = new GameObject("GarbageTruck");
            car.transform.SetParent(parent, false);
            if (models != null && models.Length > 0 && models[slot % models.Length] != null)
            {
                GameObject model = Object.Instantiate(models[slot % models.Length]);
                model.transform.SetParent(car.transform, false);
                model.transform.localScale = model.transform.localScale * modelScale;
                VehicleModelUtils.Align(model);
            }
            car.SetActive(false);
            return car;
        }

        // Обновить все виды из системы: активные мусоровозы на маршруте, остальные скрыты.
        public void Sync(GarbageSystem garbage, GridVisualizer visualizer)
        {
            if (visualizer == null) return;
            float cellSize = visualizer.Grid.CellSize;
            for (int i = 0; i < cars.Length; i++) Place(cars[i], garbage.TruckAt(i), visualizer, cellSize);
        }

        // Позиция и поворот по прогрессу мусоровоза с интерполяцией между клетками.
        private void Place(GameObject car, GarbageTrucks.Truck truck, GridVisualizer visualizer, float cellSize)
        {
            if (!truck.Active || truck.Route == null || truck.Route.Count < 2)
            {
                car.SetActive(false);
                return;
            }
            List<GridPosition> route = truck.Route;
            int last = route.Count - 1;
            int cell = Mathf.Clamp((int)truck.Progress, 0, last);
            int next = cell < last ? cell + 1 : last;
            float t = cell < last ? truck.Progress - cell : 0f;
            float ax, az, bx, bz;
            visualizer.Grid.GridToWorld(route[cell], out ax, out az);
            visualizer.Grid.GridToWorld(route[next], out bx, out bz);
            car.transform.localScale = Vector3.one * (cellSize * 0.5f);
            car.transform.position = new Vector3(Mathf.Lerp(ax, bx, t), rideHeight, Mathf.Lerp(az, bz, t));
            float dx = bx - ax;
            float dz = bz - az;
            if (Mathf.Abs(dx) + Mathf.Abs(dz) > 0.001f)
                car.transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(dx, dz) * Mathf.Rad2Deg + yawOffset, 0f);
            car.SetActive(true);
        }

        public void DestroyAll()
        {
            for (int i = 0; i < cars.Length; i++) DestroySafe(cars[i]);
        }

        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}
