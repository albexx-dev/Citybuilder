using UnityEngine;

namespace Citybuilder
{
    // Общая утилита Kenney-моделей машин (EPIC-44/52): авто-масштаб под клетку дороги
    // (Fit) и выравнивание центра по оси вида-хоста (Align). Выделено из VehicleAgentView
    // для вида мусоровозов (EPIC-52), чтобы не дублировать замер. Порядок: Fit -> Align.
    public static class VehicleModelUtils
    {
        // Сдвинуть модель так, чтобы её объединённый AABB стоял центром на оси корня.
        public static void Align(GameObject model)
        {
            Vector3 min, max;
            if (!TryBounds(model, out min, out max)) return;
            Vector3 anchor = new Vector3((min.x + max.x) * 0.5f, min.y, (min.z + max.z) * 0.5f);
            model.transform.localPosition = Vector3.Scale(-anchor, model.transform.localScale);
        }

        // Авто-масштаб под клетку: длина модели по Z становится targetLength (метры мира),
        // затем выравнивание — порядок важен: Align считает сдвиг уже под новым масштабом.
        // Возвращает применённый множитель (для тестов); модель-заглушка без мешей — 1.
        public static float Fit(GameObject model, float targetLength)
        {
            Vector3 min, max;
            if (!TryBounds(model, out min, out max)) return 1f;
            float worldLength = (max.z - min.z) * model.transform.localScale.z;
            if (worldLength <= 0.001f) return 1f;
            float multiplier = targetLength / worldLength;
            model.transform.localScale = model.transform.localScale * multiplier;
            Align(model);
            return multiplier;
        }

        // Объединённый AABB всех мешей модели в системе координат её корня.
        // bounds доступны и без Read/Write у меша, поэтому замер не требует настроек импорта.
        public static bool TryBounds(GameObject model, out Vector3 min, out Vector3 max)
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
    }
}
