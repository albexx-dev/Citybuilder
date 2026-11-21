using UnityEngine;

namespace Citybuilder
{
    // Видимость декора (issue #95): декор ставится вручную через DecorPlacementController,
    // авто-спавн по DecorSeedPolicy убран как мёртвый путь. Тумблер гасит корень ручного
    // декора и собственный корень, если тот остался в сцене, не удаляя сами объекты.
    public class DecorManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Корень ручного декора. Пусто = ищется у DecorPlacementController по имени 'DecorRoot'.")]
        public GameObject ManualDecorRoot;

        private bool decorVisible = true;

        public bool DecorVisible { get { return decorVisible; } }

        public void SetDecorVisible(bool visible)
        {
            decorVisible = visible;
            ToggleRoot(OwnRoot(), visible);
            ToggleRoot(ResolveManualRoot(), visible);
        }

        private GameObject OwnRoot()
        {
            Transform found = transform.Find("DecorRoot");
            return found != null ? found.gameObject : null;
        }

        private GameObject ResolveManualRoot()
        {
            if (ManualDecorRoot != null) return ManualDecorRoot;
            DecorPlacementController placer = FindFirstObjectByType<DecorPlacementController>();
            if (placer == null) return null;
            Transform found = placer.transform.Find("DecorRoot");
            ManualDecorRoot = found != null ? found.gameObject : null;
            return ManualDecorRoot;
        }

        private static void ToggleRoot(GameObject root, bool visible)
        {
            if (root != null) root.SetActive(visible);
        }
    }
}
