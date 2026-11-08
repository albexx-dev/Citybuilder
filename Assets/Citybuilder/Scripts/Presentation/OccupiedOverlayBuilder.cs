using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Меш-заливка занятых клеток (красный оверлей): создание слоя, чистая геометрия квадов, заполнение меша.
    // Вынесено из GridMeshBuilder (EPIC-10, шаг 2); линии сетки и ховер остались там. Только показ.
    public static class OccupiedOverlayBuilder
    {
        // Новый слой OccupiedOverlay: пустой меш и материал Unlit/Color заданного цвета.
        public static Mesh BuildOverlay(Transform parent, Color color)
        {
            Mesh mesh = new Mesh();
            GameObject go = new GameObject("OccupiedOverlay");
            go.transform.SetParent(parent, false);
            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            Shader shader = Shader.Find("Unlit/Color");
            Material material = new Material(shader);
            material.color = color;
            renderer.material = material;
            return mesh;
        }

        // Заполнить меш квадами занятых клеток; геометрию считает BuildGeometry.
        public static void Fill(Mesh mesh, List<GridPosition> cells, float cellSize)
        {
            mesh.Clear();
            if (cells.Count == 0) return;
            Vector3[] vertices;
            int[] triangles;
            BuildGeometry(cells, cellSize, out vertices, out triangles);
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }

        // Чистая геометрия: 4 вершины и 2 треугольника на клетку, высота 0.01.
        public static void BuildGeometry(List<GridPosition> cells, float cellSize, out Vector3[] vertices, out int[] triangles)
        {
            vertices = new Vector3[cells.Count * 4];
            triangles = new int[cells.Count * 6];
            for (int c = 0; c < cells.Count; c++) FillCellQuad(vertices, triangles, c, cells[c], cellSize);
        }

        // Вершины и треугольники одной занятой клетки.
        private static void FillCellQuad(Vector3[] vertices, int[] triangles, int c, GridPosition cell, float cellSize)
        {
            float x0 = cell.X * cellSize;
            float z0 = cell.Y * cellSize;
            float x1 = x0 + cellSize;
            float z1 = z0 + cellSize;
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
    }
}
