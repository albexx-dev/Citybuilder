using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Вид мостов: палуба с перилами по BridgeMeshBuilder, цвет полотна из RoadDefinition (ADR-018).
    public class BridgeRoadView : MonoBehaviour
    {
        public void Rebuild(Dictionary<string, List<RoadCellKey>> cellsByType, float cellSize,
            Dictionary<string, Color> colorById)
        {
            ClearDecks();
            if (cellsByType == null || cellSize <= 0f) return;
            foreach (KeyValuePair<string, List<RoadCellKey>> pair in cellsByType)
                BuildDeck(pair.Key, pair.Value, cellSize, colorById);
        }

        // Гасим старые полотна типов, включая типы без клеток (снос/загрузка старого слота).
        private void ClearDecks()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (!child.name.StartsWith("BridgeMesh_")) continue;
                MeshFilter filter = child.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null) filter.sharedMesh.Clear();
            }
        }

        private void BuildDeck(string typeId, List<RoadCellKey> cells, float cellSize,
            Dictionary<string, Color> colorById)
        {
            Color color;
            if (colorById == null || !colorById.TryGetValue(typeId, out color)) color = Color.gray;
            Vector3[] vertices;
            int[] triangles;
            BridgeMeshBuilder.Build(cells, cellSize, out vertices, out triangles);
            Transform found = transform.Find("BridgeMesh_" + typeId);
            GameObject go = found != null ? found.gameObject : new GameObject("BridgeMesh_" + typeId);
            if (found == null) go.transform.SetParent(transform, false);
            MeshFilter filter = go.GetComponent<MeshFilter>();
            if (filter == null) filter = go.AddComponent<MeshFilter>();
            if (filter.sharedMesh == null) filter.sharedMesh = new Mesh();
            filter.sharedMesh.Clear();
            filter.sharedMesh.vertices = vertices;
            filter.sharedMesh.triangles = triangles;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = go.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Unlit/Color"));
            material.color = color;
            renderer.sharedMaterial = material;
        }
    }
}
