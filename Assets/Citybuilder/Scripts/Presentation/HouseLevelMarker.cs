using UnityEngine;

namespace Citybuilder
{
    // Маркер уровня (issue #94): отдельный куб со своим материалом над крышей дома.
    // Материалы Kenney-префаба не трогаются, emissive на инстансах не включается.
    // 12 треугольников на маркер — в лимите декора ADR-010 (≤300).
    public static class HouseLevelMarker
    {
        public const string MarkerName = "LevelMarker";
        private const float Size = 0.42f;
        private const float Thickness = 0.12f;
        private const float Gap = 0.18f;
        // Два материала на уровни 2 и 3: переиспользуются, при смене уровня/сносе не утекают.
        private static readonly Material[] markerMaterials = new Material[2];

        // level 1 снимает старый маркер; 2 и 3 ставят свой цвет.
        public static void Update(GameObject house, int level)
        {
            if (house == null) return;
            Detach(house);
            if (!HouseLevelStyle.HasMarker(level)) return;

            Transform root = house.transform;
            float above = TopAbove(root);
            Vector3 scale = root.lossyScale;
            float sx = Safe(scale.x);
            float sy = Safe(scale.y);
            float sz = Safe(scale.z);

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = MarkerName;
            marker.transform.SetParent(root, false);
            marker.transform.localPosition = new Vector3(0f, (above + Gap) / sy, 0f);
            marker.transform.localScale = new Vector3(Size / sx, Thickness / sy, Size / sz);
            DestroySafe(marker.GetComponent<Collider>());
            marker.GetComponent<MeshRenderer>().sharedMaterial = MaterialFor(level);
        }

        public static void Detach(GameObject house)
        {
            if (house == null) return;
            Transform old = house.transform.Find(MarkerName);
            if (old == null) return;
            DestroySafe(old.gameObject);
        }

        // Верхняя точка модели в мировых координатах: у префаба и у куба с крышей.
        private static float TopAbove(Transform house)
        {
            float top = house.position.y;
            Renderer[] renderers = house.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null || renderers[i].gameObject.name == MarkerName) continue;
                top = Mathf.Max(top, renderers[i].bounds.max.y);
            }
            return top - house.position.y;
        }

        // Свои материалы только для маркеров: shared-материалы зданий не трогаются.
        private static Material MaterialFor(int level)
        {
            int index = level >= 3 ? 1 : 0;
            if (markerMaterials[index] == null)
            {
                markerMaterials[index] = new Material(Shader.Find("Unlit/Color"))
                {
                    color = HouseLevelStyle.MarkerColor(level)
                };
            }
            return markerMaterials[index];
        }

        private static float Safe(float value)
        {
            float absolute = Mathf.Abs(value);
            return absolute < 0.01f ? 0.01f : absolute;
        }

        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}
