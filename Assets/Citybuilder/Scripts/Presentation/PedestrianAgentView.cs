using UnityEngine;

namespace Citybuilder
{
    // Виды пешеходов (EPIC-46): пул фигурок «капсула + голова» из примитивов, без коллайдеров и физики.
    // Вид [i] привязан к слоту агента [i]; приглушённый цвет детерминирован слотом (seed).
    public class PedestrianAgentView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка: размер клетки и мировые координаты. Обязательна.")]
        public GridVisualizer Visualizer;

        [Header("Pool")]
        [Tooltip("Максимум видимых фигурок (1..30). Агенты сверх лимита ходят без вида (LOD в #62).")]
        public int MaxViews = 30;
        [Tooltip("Высота фигурки над землёй, метры.")]
        public float GroundHeight = 0.05f;
        [Tooltip("Множитель масштаба: 1 = рост 0.5 м при клетке 1 м.")]
        public float Scale = 1f;

        // Приглушённая палитра одежды: 4 варианта, выбирается по слоту.
        private static readonly Color[] Palette =
        {
            new Color(0.55f, 0.45f, 0.40f), new Color(0.40f, 0.48f, 0.55f),
            new Color(0.50f, 0.42f, 0.50f), new Color(0.45f, 0.52f, 0.40f)
        };

        private GameObject[] figures;
        private Material[] materials;

        // Обновить все виды из менеджера. Вызывается хостом каждый кадр после Advance.
        public void Sync(PedestrianAgentManager manager)
        {
            if (manager == null || Visualizer == null) return;
            EnsurePool();
            for (int i = 0; i < figures.Length; i++) ShowFigure(i, manager.AgentAt(i));
        }

        private void EnsurePool()
        {
            int count = Mathf.Clamp(MaxViews, 1, PedestrianAgentManager.MaxPedestrians);
            if (figures != null && figures.Length == count) return;
            ClearPool();
            EnsureMaterials();
            figures = new GameObject[count];
            for (int i = 0; i < count; i++) figures[i] = BuildFigure(i);
        }

        private void EnsureMaterials()
        {
            if (materials != null) return;
            materials = new Material[Palette.Length];
            for (int i = 0; i < Palette.Length; i++)
                materials[i] = new Material(Shader.Find("Unlit/Color")) { color = Palette[i] };
        }

        private void ClearPool()
        {
            if (figures == null) return;
            for (int i = 0; i < figures.Length; i++) DestroySafe(figures[i]);
            figures = null;
        }

        // Фигурка: капсула-тело (0.36 м) и сфера-голова на общем корне; опора в ногах.
        private GameObject BuildFigure(int slot)
        {
            GameObject figure = new GameObject("Pedestrian");
            figure.transform.SetParent(transform, false);
            Material material = materials[slot % materials.Length];
            BuildPart(figure, PrimitiveType.Capsule, new Vector3(0.16f, 0.18f, 0.16f), 0.18f, material);
            BuildPart(figure, PrimitiveType.Sphere, Vector3.one * 0.14f, 0.44f, material);
            figure.SetActive(false);
            return figure;
        }

        // Часть фигурки: примитив без коллайдера, локальная высота центра от ног.
        private static void BuildPart(GameObject figure, PrimitiveType type, Vector3 scale, float centerY, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(figure.transform, false);
            part.transform.localScale = scale;
            part.transform.localPosition = new Vector3(0f, centerY, 0f);
            part.GetComponent<Renderer>().sharedMaterial = material;
        }

        private void ShowFigure(int index, PedestrianAgentManager.PedestrianAgent agent)
        {
            GameObject figure = figures[index];
            if (!agent.Active)
            {
                figure.SetActive(false);
                return;
            }
            float ax, az, bx, bz;
            Visualizer.Grid.GridToWorld(agent.Current, out ax, out az);
            Visualizer.Grid.GridToWorld(agent.Next, out bx, out bz);
            float t = Mathf.Clamp01(agent.Progress);
            figure.transform.position = new Vector3(Mathf.Lerp(ax, bx, t), GroundHeight, Mathf.Lerp(az, bz, t));
            figure.transform.localScale = Vector3.one * (Visualizer.Grid.CellSize * Scale);
            figure.SetActive(true);
        }

        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        private void OnDestroy()
        {
            ClearPool();
            if (materials == null) return;
            for (int i = 0; i < materials.Length; i++) DestroySafe(materials[i]);
            materials = null;
        }
    }
}
