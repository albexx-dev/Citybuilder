using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Строит меши сетки: линии, ховер-квад, красный оверлей занятых клеток и подсветку радиуса службы (EPIC-12 шаг 3).
    public class GridMeshBuilder : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Визуализатор. Обязателен: даёт размеры, цвета и сетку.")]
        public GridVisualizer Visualizer;

        private GameObject hoverQuad;
        private Mesh occupiedMesh;
        private Mesh radiusMesh;
        private MeshRenderer radiusRenderer;

        public bool HasVisuals { get { return hoverQuad != null && occupiedMesh != null; } }

        public void EnsureVisuals()
        {
            if (!Ready()) return;
            EnsureLines();
            EnsureOverlays();
        }

        public void SetHover(float worldX, float worldZ)
        {
            if (hoverQuad == null) return;
            hoverQuad.transform.position = new Vector3(worldX, 0.02f, worldZ);
            hoverQuad.SetActive(true);
        }

        public void HideHover()
        {
            if (hoverQuad == null) return;
            hoverQuad.SetActive(false);
        }

        public void RebuildOccupied()
        {
            if (!Ready()) return;
            if (occupiedMesh == null) occupiedMesh = OccupiedOverlayBuilder.BuildOverlay(transform, Visualizer.OccupiedColor);
            OccupiedOverlayBuilder.Fill(occupiedMesh, Visualizer.Grid.GetOccupiedCells(), Visualizer.CellSize);
        }

        public MeshRenderer RadiusRenderer => radiusRenderer;

        // Подсветить клетки радиуса выбранной службы или очистить при пустом списке (EPIC-12 шаг 3).
        public void RebuildRadius(List<GridPosition> cells, Color color)
        {
            if (!Ready()) return;
            if (radiusMesh == null)
            {
                radiusMesh = OccupiedOverlayBuilder.BuildOverlay(transform, color);
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    MeshFilter filter = child.GetComponent<MeshFilter>();
                    if (filter != null && filter.sharedMesh == radiusMesh)
                    {
                        child.name = "RadiusOverlay";
                        radiusRenderer = child.GetComponent<MeshRenderer>();
                        break;
                    }
                }
            }
            if (radiusRenderer == null)
            {
                GameObject go = FindChild("RadiusOverlay");
                if (go != null) radiusRenderer = go.GetComponent<MeshRenderer>();
            }
            if (radiusRenderer != null && radiusRenderer.sharedMaterial != null)
            {
                radiusRenderer.sharedMaterial.color = color;
            }
            OccupiedOverlayBuilder.Fill(radiusMesh, cells ?? new List<GridPosition>(), Visualizer.CellSize);
        }

        private bool Ready()
        {
            if (Visualizer == null) Visualizer = GetComponent<GridVisualizer>();
            if (Visualizer != null) return true;
            Debug.LogError("GridMeshBuilder: задайте Visualizer в Inspector.");
            return false;
        }

        private void EnsureLines()
        {
            GameObject lines = FindChild("GridLines");
            MeshFilter linesFilter = lines == null ? null : lines.GetComponent<MeshFilter>();
            bool linesReady = linesFilter != null && linesFilter.mesh != null && linesFilter.mesh.vertexCount > 0;
            if (linesReady) return;
            if (lines != null) Destroy(lines);
            BuildGridLines();
        }

        private void EnsureOverlays()
        {
            if (hoverQuad == null)
            {
                hoverQuad = FindChild("HoverQuad");
                if (hoverQuad == null) hoverQuad = BuildQuad(Visualizer.HoverColor);
                hoverQuad.SetActive(false);
            }
            if (occupiedMesh == null)
            {
                GameObject overlay = FindChild("OccupiedOverlay");
                if (overlay != null) occupiedMesh = overlay.GetComponent<MeshFilter>().mesh;
                else RebuildOccupied();
            }
        }

        private GameObject FindChild(string childName)
        {
            Transform found = transform.Find(childName);
            return found == null ? null : found.gameObject;
        }

        private void BuildGridLines()
        {
            GameObject go = new GameObject("GridLines");
            go.transform.SetParent(transform, false);
            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.mesh = new Mesh();
            go.AddComponent<MeshRenderer>().material = BuildMaterial(Visualizer.LineColor);
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
            filter.mesh.vertices = vertices;
            filter.mesh.SetIndices(indices, MeshTopology.Lines, 0);
            filter.mesh.RecalculateBounds();
        }

        private GameObject BuildQuad(Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "HoverQuad";
            go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(Visualizer.CellSize, Visualizer.CellSize, 1f);
            Destroy(go.GetComponent<Collider>());
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
    }
}
