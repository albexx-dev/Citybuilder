using UnityEngine;

namespace Citybuilder
{
    // Земля и рамка карты (EPIC-57, view-слой): ровная трава под сеткой с 2-3
    // пятнами других оттенков и полоса «воды» вокруг карты. Домен о земле не
    // знает, коллайдеров нет — пик клеток идёт по Plane в GridVisualizer.
    // Свет и тени не трогаются: шаг light делает оркестратор.
    public class GridGroundBuilder : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Визуализатор: даёт размеры сетки. Пусто = берётся с этого же объекта.")]
        public GridVisualizer Visualizer;

        // Цвет травы, оттенки пятен и воды (EPIC-57). Unlit/Color не читает
        // vertex colors, поэтому вариация — прямоугольными подмешами.
        public static readonly Color GrassColor = new Color(0.30f, 0.55f, 0.25f);
        public static readonly Color DarkGrass = new Color(0.24f, 0.45f, 0.20f);
        public static readonly Color LightGrass = new Color(0.38f, 0.64f, 0.31f);
        public static readonly Color WaterColor = new Color(0.20f, 0.35f, 0.55f);

        public const float GroundY = -0.02f; // земля чуть ниже линий сетки (y = 0)
        public const float PatchY = -0.015f; // пятна выше базы, но под сеткой
        public const float BorderY = -0.05f; // вода под землёй
        public const float BorderWidth = 6f; // рамка шире карты на 6 м с каждой стороны
        public const int PatchCount = 3;
        public const int ShadeCount = 2;

        private void Start()
        {
            if (!Ready()) return;
            EnsureGround();
            EnsureBorder();
        }

        // --- Чистая математика: детерминированная, покрыта EditMode-тестами ---

        // Сторона земли в метрах: клетки * размер клетки.
        public static float SideLength(int cells, float cellSize)
        {
            return cells * cellSize;
        }

        // Границы земли: прямоугольник от (0, 0) до (sideX, sideZ) на высоте y.
        public static void GroundBounds(int cellsX, int cellsZ, float cellSize, float y,
            out Vector3 min, out Vector3 max)
        {
            min = new Vector3(0f, y, 0f);
            max = new Vector3(SideLength(cellsX, cellSize), y, SideLength(cellsZ, cellSize));
        }

        // Границы воды: на BorderWidth шире земли с каждой стороны карты.
        public static void BorderBounds(int cellsX, int cellsZ, float cellSize, float y,
            out Vector3 min, out Vector3 max)
        {
            GroundBounds(cellsX, cellsZ, cellSize, y, out min, out max);
            min -= new Vector3(BorderWidth, 0f, BorderWidth);
            max += new Vector3(BorderWidth, 0f, BorderWidth);
        }

        // Оттенок пятна: детерминированный хеш по паттерну DecorSeedPolicy.
        public static int ShadeIndex(int patchIndex, int shadeCount)
        {
            return DecorSeedPolicy.VariantIndex(patchIndex, 1, shadeCount);
        }

        // Прямоугольник пятна травы: размер и позиция из хеша, всегда внутри карты.
        public static void PatchRect(int patchIndex, float side, out Vector3 min, out Vector3 max)
        {
            float width = side * (0.18f + 0.17f * DecorSeedPolicy.Normalized(patchIndex, 913));
            float x = DecorSeedPolicy.Normalized(patchIndex, 331) * (side - width);
            float z = DecorSeedPolicy.Normalized(patchIndex, 727) * (side - width);
            min = new Vector3(x, PatchY, z);
            max = new Vector3(x + width, PatchY, z + width);
        }

        // --- Построение view-слоя ---

        private bool Ready()
        {
            if (Visualizer == null) Visualizer = GetComponent<GridVisualizer>();
            if (Visualizer != null) return true;
            Debug.LogError("GridGroundBuilder: задайте Visualizer в Inspector.");
            enabled = false;
            return false;
        }

        private void EnsureGround()
        {
            if (transform.Find("GroundRoot") != null) return;
            Transform root = new GameObject("GroundRoot").transform;
            root.SetParent(transform, false);
            Vector3 min, max;
            GroundBounds(Visualizer.Width, Visualizer.Height, Visualizer.CellSize, 0f, out min, out max);
            AddQuad(root, "GrassBase", GrassColor, min.x, min.z, max.x, max.z, GroundY);
            float side = SideLength(Visualizer.Width, Visualizer.CellSize);
            for (int i = 0; i < PatchCount; i++) AddPatch(root, i, side);
        }

        private void AddPatch(Transform root, int index, float side)
        {
            Vector3 min, max;
            PatchRect(index, side, out min, out max);
            Color shade = ShadeIndex(index, ShadeCount) == 0 ? DarkGrass : LightGrass;
            AddQuad(root, "GrassPatch" + index, shade, min.x, min.z, max.x, max.z, PatchY);
        }

        private void EnsureBorder()
        {
            if (transform.Find("WaterBorder") != null) return;
            Vector3 min, max;
            BorderBounds(Visualizer.Width, Visualizer.Height, Visualizer.CellSize, 0f, out min, out max);
            AddQuad(transform, "WaterBorder", WaterColor, min.x, min.z, max.x, max.z, BorderY);
        }

        // Прямоугольный подмеш: свой меш и материал Unlit/Color, без коллайдера.
        private static void AddQuad(Transform parent, string name, Color color,
            float minX, float minZ, float maxX, float maxZ, float y)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(minX, y, minZ);
            go.AddComponent<MeshFilter>().mesh = QuadMesh(maxX - minX, maxZ - minZ);
            go.AddComponent<MeshRenderer>().material = UnlitMaterial(color);
        }

        // Квад в плоскости XZ: 4 вершины и 2 треугольника, обход как у оверлеев.
        private static Mesh QuadMesh(float width, float depth)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new[]
            {
                new Vector3(0f, 0f, 0f), new Vector3(width, 0f, 0f),
                new Vector3(width, 0f, depth), new Vector3(0f, 0f, depth)
            };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material UnlitMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Unlit/Color"));
            material.color = color;
            return material;
        }
    }
}
