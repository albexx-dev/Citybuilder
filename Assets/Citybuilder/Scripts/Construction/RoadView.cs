using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Плоский вид дорог: серый меш построенных + оверлей пробок (жёлтый/красный, EPIC-15 шаг 2) + предпросмотр.
    public class RoadView : MonoBehaviour
    {
        public const float YellowThreshold = 0.33f;
        public const float RedThreshold = 0.66f;

        private Mesh builtMesh;
        private Mesh previewMesh;
        private Mesh yellowMesh;
        private Mesh redMesh;
        private MeshRenderer previewRenderer;
        private Material validMaterial;
        private Material invalidMaterial;
        private int lastTick = -1;

        public void Rebuild(List<GridPosition> cells, float cellSize)
        {
            EnsureBuilt();
            FillMesh(builtMesh, cells, cellSize, 0.015f);
        }

        public void ShowPreview(List<GridPosition> cells, float cellSize, bool valid)
        {
            EnsurePreview();
            FillMesh(previewMesh, cells, cellSize, 0.025f);
            previewRenderer.sharedMaterial = valid ? validMaterial : invalidMaterial;
        }

        public void HidePreview()
        {
            if (previewMesh != null) previewMesh.Clear();
        }

        // Классифицирует нагрузку дороги 0..1: 0 = свободно, 1 = жёлтая (0.33..0.66), 2 = красная (>0.66).
        public static int BucketForLoad01(float load01)
        {
            if (float.IsNaN(load01) || load01 <= YellowThreshold) return 0;
            return load01 <= RedThreshold ? 1 : 2;
        }

        // Обновляет покраску клеток дороги раз в тик (EPIC-15 шаг 2). Возвращает true, если меши пересобраны.
        public bool RefreshCongestionOncePerTick(
            int tick, IReadOnlyList<GridPosition> roadCells, float cellSize, Func<GridPosition, float> loadOf)
        {
            if (tick == lastTick) return false;
            lastTick = tick;
            EnsureCongestionMeshes();
            List<GridPosition> yellow = new List<GridPosition>();
            List<GridPosition> red = new List<GridPosition>();
            if (roadCells != null && loadOf != null)
            {
                for (int i = 0; i < roadCells.Count; i++)
                {
                    int bucket = BucketForLoad01(loadOf(roadCells[i]));
                    if (bucket == 1) yellow.Add(roadCells[i]);
                    else if (bucket == 2) red.Add(roadCells[i]);
                }
            }
            FillMesh(yellowMesh, yellow, cellSize, 0.018f);
            FillMesh(redMesh, red, cellSize, 0.019f);
            return true;
        }

        private void EnsureBuilt()
        {
            if (builtMesh != null) return;
            builtMesh = EnsureChildMesh("RoadsMesh", Color.gray, out _);
        }

        private void EnsureCongestionMeshes()
        {
            if (yellowMesh == null) yellowMesh = EnsureChildMesh("RoadsYellow", new Color(0.95f, 0.8f, 0.15f, 1f), out _);
            if (redMesh == null) redMesh = EnsureChildMesh("RoadsRed", new Color(0.9f, 0.2f, 0.15f, 1f), out _);
        }

        private void EnsurePreview()
        {
            if (previewMesh != null && previewRenderer != null) return;
            validMaterial = PreviewMaterial(new Color(0f, 1f, 0f, 0.4f));
            invalidMaterial = PreviewMaterial(new Color(1f, 0f, 0f, 0.4f));
            previewMesh = EnsureChildMesh("RoadPreview", new Color(0f, 1f, 0f, 0.4f), out previewRenderer);
            previewRenderer.sharedMaterial = validMaterial;
        }

        private Mesh EnsureChildMesh(string childName, Color color, out MeshRenderer renderer)
        {
            Transform found = transform.Find(childName);
            GameObject go = found != null ? found.gameObject : new GameObject(childName);
            if (found == null) go.transform.SetParent(transform, false);
            MeshFilter filter = go.GetComponent<MeshFilter>();
            if (filter == null) filter = go.AddComponent<MeshFilter>();
            if (filter.sharedMesh == null) filter.sharedMesh = new Mesh();
            renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = PreviewMaterial(color);
            return filter.sharedMesh;
        }

        private static Material PreviewMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Unlit/Color"));
            material.color = color;
            return material;
        }

        private static void FillMesh(Mesh mesh, List<GridPosition> cells, float cellSize, float height)
        {
            mesh.Clear();
            if (cells == null || cells.Count == 0 || cellSize <= 0f) return;
            Vector3[] vertices = new Vector3[cells.Count * 4];
            int[] triangles = new int[cells.Count * 6];
            for (int c = 0; c < cells.Count; c++)
            {
                float x0 = cells[c].X * cellSize;
                float z0 = cells[c].Y * cellSize;
                int v = c * 4;
                vertices[v] = new Vector3(x0, height, z0);
                vertices[v + 1] = new Vector3(x0 + cellSize, height, z0);
                vertices[v + 2] = new Vector3(x0 + cellSize, height, z0 + cellSize);
                vertices[v + 3] = new Vector3(x0, height, z0 + cellSize);
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
        }
    }
}
