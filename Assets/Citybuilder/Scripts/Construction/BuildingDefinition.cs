using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Статическое описание типа здания. Только данные для стройки (V1).
    [CreateAssetMenu(menuName = "Citybuilder/Building Definition")]
    public class BuildingDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Уникальный id здания, латиницей без пробелов.")]
        public string Id;

        [Tooltip("Имя для UI.")]
        public string DisplayName;

        [Header("Construction")]
        [Tooltip("Размер здания в клетках. Минимум 1x1.")]
        public Vector2Int Footprint = new Vector2Int(2, 2);

        [Tooltip("Стоимость постройки. Не может быть отрицательной.")]
        public int ConstructionCost = 100;

        [Header("Presentation")]
        [Tooltip("Префаб здания. Может быть пустым до готовности арта.")]
        public GameObject Prefab;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                Debug.LogWarning("BuildingDefinition: id is empty.", this);
            }

            int fixedX = Mathf.Max(1, Footprint.x);
            int fixedY = Mathf.Max(1, Footprint.y);
            if (fixedX != Footprint.x || fixedY != Footprint.y)
            {
                Debug.LogWarning("BuildingDefinition: footprint fixed to minimum 1x1.", this);
                Footprint = new Vector2Int(fixedX, fixedY);
            }

            if (ConstructionCost < 0)
            {
                Debug.LogWarning("BuildingDefinition: cost fixed to 0.", this);
                ConstructionCost = 0;
            }
        }

        // Все клетки прямоугольника от origin (включительно).
        public List<GridPosition> CoversCells(GridPosition origin)
        {
            List<GridPosition> cells = new List<GridPosition>(Footprint.x * Footprint.y);
            for (int dx = 0; dx < Footprint.x; dx++)
            {
                for (int dy = 0; dy < Footprint.y; dy++)
                {
                    cells.Add(new GridPosition(origin.X + dx, origin.Y + dy));
                }
            }
            return cells;
        }
    }
}
