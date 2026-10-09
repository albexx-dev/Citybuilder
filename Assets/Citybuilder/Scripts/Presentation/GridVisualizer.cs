using UnityEngine;

namespace Citybuilder
{
    // Рисует логическую сетку: линии, ховер клетки, занятые клетки.
    // Настройка сцены: пустой GameObject + этот компонент, камера сверху.
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Grid")]
        [Tooltip("Клеток по X.")]
        public int Width = 50;
        [Tooltip("Клеток по Y.")]
        public int Height = 50;
        [Tooltip("Размер клетки в метрах.")]
        public float CellSize = 1f;

        [Header("Colors")]
        public Color LineColor = Color.white;
        public Color HoverColor = new Color(1f, 1f, 0f, 0.4f);
        public Color OccupiedColor = new Color(1f, 0f, 0f, 0.4f);

        public GridSystem Grid { get; private set; }
        public GridPosition HoveredCell { get; private set; }
        public bool HasHoveredCell { get; private set; }

        private GameObject hoverQuad;
        private Mesh occupiedMesh;

        public bool TryGetHoveredCell(out GridPosition cell)
        {
            cell = HoveredCell;
            return HasHoveredCell;
        }

        // Перестроить красный оверлей после Occupy/Release.
        public void RefreshOccupied()
        {
            BuildOccupiedMesh();
        }

        private void Awake()
        {
            Grid = new GridSystem(new GridSize(Width, Height), CellSize);
            BuildGridLines();
            hoverQuad = BuildQuad(HoverColor);
            hoverQuad.SetActive(false);
            BuildOccupiedMesh();
        }

        private void Update()
        {
            UpdateHover();
        }

        private void UpdateHover()
        {
            HasHoveredCell = false;
            Camera camera = Camera.main;
            if (camera == null)
            {
                hoverQuad.SetActive(false);
                return;
            }
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            if (!plane.Raycast(ray, out distance))
            {
                hoverQuad.SetActive(false);
                return;
            }
            Vector3 point = ray.GetPoint(distance);
            GridPosition cell = Grid.WorldToGrid(point.x, point.z);
            if (!Grid.IsInside(cell))
            {
                hoverQuad.SetActive(false);
                return;
            }
            HasHoveredCell = true;
            HoveredCell = cell;
            float wx, wz;
            Grid.GridToWorld(cell, out wx, out wz);
            hoverQuad.transform.position = new Vector3(wx, 0.02f, wz);
            hoverQuad.SetActive(true);
        }

        private void BuildGridLines()
        {
            GameObject go = new GameObject("GridLines");
            go.transform.SetParent(transform, false);
            LineRenderer lines = go.AddComponent<LineRenderer>();
            lines.useWorldSpace = false;
            lines.startWidth = 0.03f;
            lines.endWidth = 0.03f;
            lines.startColor = LineColor;
            lines.endColor = LineColor;
            lines.material = BuildMaterial(Color.white);
            int vertical = Width + 1;
            int horizontal = Height + 1;
            lines.positionCount = (vertical + horizontal) * 2;
            int i = 0;
            float totalX = Width * CellSize;
            float totalZ = Height * CellSize;
            for (int x = 0; x < vertical; x++, i += 2)
            {
                float px = x * CellSize;
                lines.SetPosition(i, new Vector3(px, 0f, 0f));
                lines.SetPosition(i + 1, new Vector3(px, 0f, totalZ));
            }
            for (int z = 0; z < horizontal; z++, i += 2)
            {
                float pz = z * CellSize;
                lines.SetPosition(i, new Vector3(0f, 0f, pz));
                lines.SetPosition(i + 1, new Vector3(totalX, 0f, pz));
            }
        }

        private GameObject BuildQuad(Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(CellSize, CellSize, 1f);
            Destroy(go.GetComponent<Collider>());
            go.GetComponent<MeshRenderer>().material = BuildMaterial(color);
            return go;
        }

        private Material BuildMaterial(Color color)
        {
            Shader shader = Shader.Find("Unlit/Transparent");
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private void BuildOccupiedMesh()
        {
            if (occupiedMesh == null)
            {
                occupiedMesh = new Mesh();
                GameObject go = new GameObject("OccupiedOverlay");
                go.transform.SetParent(transform, false);
                MeshFilter filter = go.AddComponent<MeshFilter>();
                filter.mesh = occupiedMesh;
                MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                renderer.material = BuildMaterial(OccupiedColor);
            }
            occupiedMesh.Clear();
            System.Collections.Generic.List<GridPosition> cells = Grid.GetOccupiedCells();
            if (cells.Count == 0)
            {
                return;
            }
            Vector3[] vertices = new Vector3[cells.Count * 4];
            int[] triangles = new int[cells.Count * 6];
            for (int c = 0; c < cells.Count; c++)
            {
                float x0 = cells[c].X * CellSize;
                float z0 = cells[c].Y * CellSize;
                float x1 = x0 + CellSize;
                float z1 = z0 + CellSize;
                int v = c * 4;
                vertices[v] = new Vector3(x0, 0.01f, z0);
                vertices[v + 1] = new Vector3(x1, 0.01f, z0);
                vertices[v + 2] = new Vector3(x1, 0.01f, z1);
                vertices[v + 3] = new Vector3(x0, 0.01f, z1);
                int t = c * 6;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 2;
            }
            occupiedMesh.vertices = vertices;
            occupiedMesh.triangles = triangles;
            occupiedMesh.RecalculateNormals();
        }
    }
}
