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

        [Tooltip("Ключ имени в реестре RU/EN. Пусто — DisplayName.")]
        public string DisplayNameKey;

        [Tooltip("Цвет полотна дороги в сцене.")]
        public Color SurfaceColor = new Color(0.35f, 0.38f, 0.42f, 1f);

        [Header("Construction")]
        [Tooltip("Стоимость одной клетки дороги. Не может быть отрицательной.")]
        public int CostPerCell = 10;

        [Tooltip("Категория каталога (задача 173). Дороги ставятся режимом «Дороги», поле — под будущую дорогу-карточку.")]
        public BuildingCategory Category = BuildingCategory.Roads;

        [Header("Level")]
        [Tooltip("Уровень полотна: 1 — обычная дорога, 2 — мост (ADR-018).")]
        public int Level = 1;

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
