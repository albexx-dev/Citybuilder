using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Цветные подложки зон: жилая — зелёная, коммерческая — синяя, промышленная — оранжевая.
    // Настройка сцены: компонент на том же объекте, что и GridVisualizer.
    public class ZoneView : MonoBehaviour
    {
        [Header("Height")]
        [Tooltip("Высота слоёв зон над сеткой в метрах.")]
        public float Height = 0.02f;

        private Mesh residentialMesh;
        private Mesh commercialMesh;
        private Mesh industrialMesh;

        private void Awake()
        {
            CreateLayers();
        }

        // Перестроить все три слоя по словарю зон.
        public void Rebuild(Dictionary<ZoneType, List<GridPosition>> zones, float cellSize)
        {
            FillMesh(residentialMesh, GetCells(zones, ZoneType.Residential), cellSize);
            FillMesh(commercialMesh, GetCells(zones, ZoneType.Commercial), cellSize);
            FillMesh(industrialMesh, GetCells(zones, ZoneType.Industrial), cellSize);
        }

        private static List<GridPosition> GetCells(Dictionary<ZoneType, List<GridPosition>> zones, ZoneType zone)
        {
            if (zones == null)
            {
                return null;
            }
            List<GridPosition> cells;
            if (zones.TryGetValue(zone, out cells))
            {
                return cells;
            }
            return null;
        }

        private void CreateLayers()
        {
            residentialMesh = BuildLayer("ZoneResidential", new Color(0f, 1f, 0f, 0.3f));
            commercialMesh = BuildLayer("ZoneCommercial", new Color(0f, 0f, 1f, 0.3f));
            industrialMesh = BuildLayer("ZoneIndustrial", new Color(1f, 0.5f, 0f, 0.3f));
        }

        private Mesh BuildLayer(string layerName, Color color)
        {
            Mesh mesh = new Mesh();
            GameObject go = new GameObject(layerName);
            go.transform.SetParent(transform, false);
            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.material = BuildMaterial(color);
            return mesh;
        }

        private static Material BuildMaterial(Color color)
        {
            Shader shader = Shader.Find("Unlit/Transparent");
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private void FillMesh(Mesh mesh, List<GridPosition> cells, float cellSize)
        {
            mesh.Clear();
            if (cells == null || cells.Count == 0)
            {
                return;
            }
            Vector3[] vertices = new Vector3[cells.Count * 4];
            int[] triangles = new int[cells.Count * 6];
            for (int c = 0; c < cells.Count; c++)
            {
                float x0 = cells[c].X * cellSize;
                float z0 = cells[c].Y * cellSize;
                int v = c * 4;
                vertices[v] = new Vector3(x0, Height, z0);
                vertices[v + 1] = new Vector3(x0 + cellSize, Height, z0);
                vertices[v + 2] = new Vector3(x0 + cellSize, Height, z0 + cellSize);
                vertices[v + 3] = new Vector3(x0, Height, z0 + cellSize);
                int t = c * 6;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 2;
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }
    }
}
