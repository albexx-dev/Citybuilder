using UnityEngine;

namespace Citybuilder
{
    // Статическое описание вида декора (EPIC-56 «Декор [5]»).
    // Декор не занимает клетки и не пишется в домен — только презентация.
    [CreateAssetMenu(menuName = "Citybuilder/Decor Definition")]
    public class DecorDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Уникальный id вида декора, латиницей без пробелов.")] public string Id;
        [Tooltip("Имя для UI.")] public string DisplayName;

        [Header("Presentation")]
        [Tooltip("Префаб дерева или куста. Пусто = рисуется куб-заглушка.")] public GameObject Prefab;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(Id)) Debug.LogWarning("DecorDefinition: id is empty.", this);
        }
    }
}
