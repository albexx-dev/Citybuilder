using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Вид мостов: палуба с перилами по BridgeMeshBuilder, цвет полотна из RoadDefinition (ADR-018).
    public class BridgeRoadView : MonoBehaviour
    {
        private const string Prefix = "BridgeMesh_";
        private readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

        public void Rebuild(Dictionary<string, List<RoadCellKey>> cellsByType, float cellSize,
            Dictionary<string, Color> colorById)
        {
            ClearDecks(cellsByType);
            if (cellsByType == null || cellSize <= 0f) return;
            foreach (KeyValuePair<string, List<RoadCellKey>> pair in cellsByType)
                BuildDeck(pair.Key, pair.Value, cellSize, colorById);
        }

        // Удалённые типы — Destroy(child.gameObject); живые чистим и оставляем материал.
        private void ClearDecks(Dictionary<string, List<RoadCellKey>> cellsByType)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child == null || !child.name.StartsWith(Prefix)) continue;
                string typeId = child.name.Substring(Prefix.Length);
                if (cellsByType != null && cellsByType.ContainsKey(typeId))
                {
                    MeshFilter filter = child.GetComponent<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null) filter.sharedMesh.Clear();
                    continue;
                }
                DropMaterial(typeId);
                Destroy(child.gameObject);
            }
        }

        // Материал удалённого типа больше не нужен.
        private void DropMaterial(string typeId)
        {
            Material material;
            if (!materials.TryGetValue(typeId, out material)) return;
            materials.Remove(typeId);
            if (material != null) Destroy(material);
        }

        private void BuildDeck(string typeId, List<RoadCellKey> cells, float cellSize,
            Dictionary<string, Color> colorById)
        {
            Color color;
            if (colorById == null || !colorById.TryGetValue(typeId, out color)) color = Color.gray;
            Vector3[] vertices;
            int[] triangles;
            BridgeMeshBuilder.Build(cells, cellSize, out vertices, out triangles);
            Transform found = transform.Find(Prefix + typeId);
            GameObject go = found != null ? found.gameObject : new GameObject(Prefix + typeId);
            if (found == null) go.transform.SetParent(transform, false);
            MeshFilter filter = go.GetComponent<MeshFilter>();
            if (filter == null) filter = go.AddComponent<MeshFilter>();
            if (filter.sharedMesh == null) filter.sharedMesh = new Mesh();
            filter.sharedMesh.Clear();
            filter.sharedMesh.vertices = vertices;
            filter.sharedMesh.triangles = triangles;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = go.AddComponent<MeshRenderer>();
            Material material;
            if (!materials.TryGetValue(typeId, out material) || material == null)
            {
                material = new Material(Shader.Find("Unlit/Color"));
                materials[typeId] = material;
            }
            material.color = color;
            renderer.sharedMaterial = material;
        }
    }
}
