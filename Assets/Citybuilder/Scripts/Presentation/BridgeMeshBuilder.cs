using System.Collections.Generic;
using UnityEngine;
namespace Citybuilder
{
    // Палуба и перила моста: 6 треугольников на клетку, не более 3000 на модель (ADR-018/022).
    public static class BridgeMeshBuilder
    {
        public const int MaxTriangles = 3000;
        public const float DeckHeight = 1f;
        private const int PerCell = 6;
        public static void Build(List<RoadCellKey> cells, float cellSize, out Vector3[] vertices, out int[] triangles)
        {
            int count = cells == null ? 0 : cells.Count;
            if (cellSize <= 0f) count = 0;
            if (count > MaxTriangles / PerCell) count = MaxTriangles / PerCell;
            vertices = new Vector3[count * 12];
            triangles = new int[count * PerCell * 3];
            for (int i = 0; i < count; i++) AddCell(cells[i], cellSize, vertices, triangles, i * 12, i * PerCell * 3);
        }
        private static void AddCell(RoadCellKey cell, float size, Vector3[] v, int[] t, int vi, int ti)
        {
            float x = cell.X * size, z = cell.Y * size, y = DeckHeight;
            Quad(v, t, vi, ti, new Vector3(x, y, z), new Vector3(x + size, y, z),
                new Vector3(x + size, y, z + size), new Vector3(x, y, z + size));
            Rail(v, t, vi + 4, ti + 6, x + 0.05f, y, z, size, false);
            Rail(v, t, vi + 8, ti + 12, x + size - 0.05f, y, z, size, true);
        }
        private static void Rail(Vector3[] v, int[] t, int vi, int ti, float x, float y, float z, float size, bool flip)
        {
            Vector3 a = new Vector3(x, y, z);
            Vector3 b = new Vector3(x, y + 0.6f, z);
            Vector3 c = new Vector3(x, y + 0.6f, z + size);
            Vector3 d = new Vector3(x, y, z + size);
            Quad(v, t, vi, ti, a, flip ? d : b, c, flip ? b : d);
        }
        private static void Quad(Vector3[] v, int[] t, int vi, int ti, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            v[vi] = a; v[vi + 1] = b; v[vi + 2] = c; v[vi + 3] = d;
            t[ti] = vi; t[ti + 1] = vi + 2; t[ti + 2] = vi + 1;
            t[ti + 3] = vi; t[ti + 4] = vi + 3; t[ti + 5] = vi + 2;
        }
    }
}
