using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Леса стройплощадки (EPIC-61): 4 угловые стойки и 2 горизонтальные рамы серо-коричневого Unlit/Color.
    // Родитель задаёт вызывающий: у куба это его родитель — леса не должны масштабироваться вместе с кубом.
    public class ConstructionScaffold
    {
        private const float BeamThickness = 0.12f;
        private static readonly Color FrameColor = new Color(0.55f, 0.45f, 0.32f);

        private readonly List<GameObject> beams = new List<GameObject>();
        private Material frameMaterial;

        // Каркас по периметру фундамента; height — высота стоек (не ниже номинальной).
        public void Build(Transform parent, Vector3 center, Vector2Int footprint, float cellSize, float height)
        {
            frameMaterial = new Material(Shader.Find("Unlit/Color")) { color = FrameColor };
            float halfW = footprint.x * cellSize * 0.5f;
            float halfD = footprint.y * cellSize * 0.5f;
            float tall = Mathf.Max(height, ConstructionSiteView.TargetHeight);
            for (int i = 0; i < 4; i++)
            {
                Vector3 at = center + new Vector3(i % 2 == 0 ? -halfW : halfW, tall * 0.5f, i < 2 ? -halfD : halfD);
                AddBeam(parent, at, new Vector3(BeamThickness, tall, BeamThickness));
            }
            AddFrame(parent, center, halfW, halfD, tall * 0.4f);
            AddFrame(parent, center, halfW, halfD, tall * 0.8f);
        }

        public void Clear()
        {
            for (int i = 0; i < beams.Count; i++) BuildingViewFactory.DestroySafe(beams[i]);
            beams.Clear();
            BuildingViewFactory.DestroySafe(frameMaterial);
            frameMaterial = null;
        }

        // Горизонтальная рама: 4 балки-ребра прямоугольником на высоте y.
        private void AddFrame(Transform parent, Vector3 center, float halfW, float halfD, float y)
        {
            AddBeam(parent, center + new Vector3(0f, y, -halfD), new Vector3(halfW * 2f + BeamThickness, BeamThickness, BeamThickness));
            AddBeam(parent, center + new Vector3(0f, y, halfD), new Vector3(halfW * 2f + BeamThickness, BeamThickness, BeamThickness));
            AddBeam(parent, center + new Vector3(-halfW, y, 0f), new Vector3(BeamThickness, BeamThickness, halfD * 2f + BeamThickness));
            AddBeam(parent, center + new Vector3(halfW, y, 0f), new Vector3(BeamThickness, BeamThickness, halfD * 2f + BeamThickness));
        }

        private void AddBeam(Transform parent, Vector3 position, Vector3 size)
        {
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            BuildingViewFactory.DestroySafe(beam.GetComponent<Collider>());
            beam.transform.SetParent(parent, false);
            beam.transform.localPosition = position;
            beam.transform.localScale = size;
            beam.GetComponent<MeshRenderer>().sharedMaterial = frameMaterial;
            beams.Add(beam);
        }
    }
}
