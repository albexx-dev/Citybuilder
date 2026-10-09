using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Плоский вид дорог: серый меш построенных и зелёный/красный предпросмотр.
    public class RoadView : MonoBehaviour
    {
        private Mesh builtMesh;
        private Mesh previewMesh;
        private MeshRenderer previewRenderer;
        private Material validMaterial;
        private Material invalidMaterial;

        // Перестраивает единый меш построенных клеток на высоте 0.015.
        public void Rebuild(List<GridPosition> cells, float cellSize)
        {
            EnsureBuilt();
            FillMesh(builtMesh, cells, cellSize, 0.015f);
        }

        // Предпросмотр отрезка на высоте 0.025: зелёный — можно, красный — нельзя.
        public void ShowPreview(List<GridPosition> cells, float cellSize, bool valid)
        {
            EnsurePreview();
            FillMesh(previewMesh, cells, cellSize, 0.025f);
            previewRenderer.sharedMaterial = valid ? validMaterial : invalidMaterial;
        }

        // Убрать предпросмотр (отмена или завершение drag).
        public void HidePreview()
        {
            if (previewMesh != null)
            {
                previewMesh.Clear();
            }
        }

        private void EnsureBuilt()
        {
            if (builtMesh != null)
            {
                return;
            }
            builtMesh = new Mesh();
            GameObject go = new GameObject("RoadsMesh");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().mesh = builtMesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Unlit/Color"));
            material.color = Color.gray;
            renderer.material = material;
        }

        private void EnsurePreview()
        {
            if (previewMesh != null)
            {
                return;
            }
            previewMesh = new Mesh();
            GameObject go = new GameObject("RoadPreview");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().mesh = previewMesh;
            previewRenderer = go.AddComponent<MeshRenderer>();
            validMaterial = PreviewMaterial(new Color(0f, 1f, 0f, 0.4f));
            invalidMaterial = PreviewMaterial(new Color(1f, 0f, 0f, 0.4f));
            previewRenderer.material = validMaterial;
        }

        private Material PreviewMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Unlit/Color"));
            material.color = color;
            return material;
        }

        // Единый меш плоских квадратов: 4 вершины и 2 треугольника на клетку.
        private void FillMesh(Mesh mesh, List<GridPosition> cells, float cellSize, float height)
        {
            mesh.Clear();
            if (cells == null || cells.Count == 0 || cellSize <= 0f)
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
