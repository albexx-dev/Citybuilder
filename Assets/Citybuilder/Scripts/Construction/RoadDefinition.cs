using UnityEngine;

namespace Citybuilder
{
    // Статическое описание типа дороги. Только данные для стройки.
    [CreateAssetMenu(menuName = "Citybuilder/Road Definition")]
    public class RoadDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Уникальный id дороги, латиницей без пробелов.")]
        public string Id;

        [Tooltip("Имя для UI.")]
        public string DisplayName;

        [Header("Construction")]
        [Tooltip("Стоимость одной клетки дороги. Не может быть отрицательной.")]
        public int CostPerCell = 10;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                Debug.LogWarning("RoadDefinition: id is empty.", this);
            }

            if (CostPerCell < 0)
            {
                Debug.LogWarning("RoadDefinition: cost fixed to 0.", this);
                CostPerCell = 0;
            }
        }
    }
}
