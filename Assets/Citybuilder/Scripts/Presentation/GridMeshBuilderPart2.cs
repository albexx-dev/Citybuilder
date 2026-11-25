using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса GridMeshBuilder (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class GridMeshBuilder : MonoBehaviour
    {
        private void BuildGridLines()
        {
            GameObject go = new GameObject("GridLines");
            go.transform.SetParent(transform, false);
            MeshFilter filter = go.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            filter.sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().material = BuildMaterial(DimmedLineColor(Visualizer.LineColor));
            int vertical = Visualizer.Width + 1, horizontal = Visualizer.Height + 1;
            int count = (vertical + horizontal) * 2;
            Vector3[] vertices = new Vector3[count];
            int[] indices = new int[count];
            float totalX = Visualizer.Width * Visualizer.CellSize, totalZ = Visualizer.Height * Visualizer.CellSize;
            int i = 0;
            for (int x = 0; x < vertical; x++, i += 2)
            {
                float px = x * Visualizer.CellSize;
                vertices[i] = new Vector3(px, 0f, 0f);
                vertices[i + 1] = new Vector3(px, 0f, totalZ);
            }
            for (int z = 0; z < horizontal; z++, i += 2)
            {
                float pz = z * Visualizer.CellSize;
                vertices[i] = new Vector3(0f, 0f, pz);
                vertices[i + 1] = new Vector3(totalX, 0f, pz);
            }
            for (int v = 0; v < count; v++) indices[v] = v;
            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
        }

        private GameObject BuildQuad(Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "HoverQuad";
            go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(Visualizer.CellSize, Visualizer.CellSize, 1f);
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) { if (Application.isPlaying) Destroy(collider); else DestroyImmediate(collider); }
            go.GetComponent<MeshRenderer>().material = BuildMaterial(color);
            return go;
        }

        private Material BuildMaterial(Color color)
        {
            Shader shader = Shader.Find("Unlit/Color");
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        // Приглушение сетки (EPIC-57): линии уходят из белого в тёмную зелень травы,
        // чтобы сетка не спорила с городом. Unlit/Color непрозрачен — гасим яркость.
        private static Color DimmedLineColor(Color line)
        {
            return Color.Lerp(line, new Color(0.12f, 0.24f, 0.10f), 0.65f);
        }
    }
}
