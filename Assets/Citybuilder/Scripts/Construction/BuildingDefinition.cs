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

        [Header("Settlement")]
        [Tooltip("На какой зоне может стоять тип здания.")]
        public ZoneType AllowedZone = ZoneType.Residential;

        [Tooltip("Сколько жителей вмещает одно здание. Не может быть отрицательным.")]
        public int ResidentCapacity = 4;

        [Tooltip("Сколько рабочих мест даёт одно здание. Не может быть отрицательным.")]
        public int JobsProvided = 0;

        [Header("Service")]
        [Tooltip("Радиус службы в клетках. 0 — не служба")]
        public int ServiceRadius = 0;

        [Header("Infrastructure")]
        [Tooltip("Сколько энергии потребляет одно здание. Не может быть отрицательным.")]
        public int PowerDemand = 1;

        [Tooltip("Сколько энергии производит одно здание. Не может быть отрицательным.")]
        public int PowerOutput = 0;

        [Tooltip("Сколько воды потребляет одно здание. Не может быть отрицательным.")]
        public int WaterDemand = 1;

        [Tooltip("Сколько воды производит одно здание. Не может быть отрицательным.")]
        public int WaterOutput = 0;

        [Tooltip("Сколько мусора производит одно здание. Не может быть отрицательным.")]
        public int WasteOutput = 1;

        [Tooltip("Сколько мусора утилизирует одно здание. Не может быть отрицательным.")]
        public int WasteCapacity = 0;

        [Header("Presentation")]
        [Tooltip("Префаб здания. Может быть пустым до готовности арта.")]
        public GameObject Prefab;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                Debug.LogWarning("BuildingDefinition: id is empty.", this);
            }

            ClampFootprint();
            ConstructionCost = Clamp(ConstructionCost, "cost");
            ResidentCapacity = Clamp(ResidentCapacity, "capacity");
            JobsProvided = Clamp(JobsProvided, "jobs");
            ServiceRadius = Clamp(ServiceRadius, "service radius");
            PowerDemand = Clamp(PowerDemand, "power demand");
            PowerOutput = Clamp(PowerOutput, "power output");
            WaterDemand = Clamp(WaterDemand, "water demand");
            WaterOutput = Clamp(WaterOutput, "water output");
            WasteOutput = Clamp(WasteOutput, "waste output");
            WasteCapacity = Clamp(WasteCapacity, "waste capacity");
        }

        // Чистая часть клампа без логирования: отрицательное становится нулём.
        public static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }

        // Поле в 0 и одно предупреждение с его именем.
        private int Clamp(int value, string field)
        {
            int fixedValue = ClampNonNegative(value);
            if (fixedValue != value)
            {
                Debug.LogWarning("BuildingDefinition: " + field + " fixed to 0.", this);
            }
            return fixedValue;
        }

        // Сторона меньше 1 ломает покрытие клеток: поднимаем до 1x1.
        private void ClampFootprint()
        {
            int fixedX = Mathf.Max(1, Footprint.x);
            int fixedY = Mathf.Max(1, Footprint.y);
            if (fixedX == Footprint.x && fixedY == Footprint.y) return;
            Debug.LogWarning("BuildingDefinition: footprint fixed to minimum 1x1.", this);
            Footprint = new Vector2Int(fixedX, fixedY);
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
