using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Статическое описание типа здания (V1 + EPIC-01/02/04/17).
    [CreateAssetMenu(menuName = "Citybuilder/Building Definition")]
    public class BuildingDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Уникальный id здания, латиницей без пробелов.")] public string Id;
        [Tooltip("Имя для UI.")] public string DisplayName;

        [Header("Construction")]
        [Tooltip("Размер здания в клетках. Минимум 1x1.")] public Vector2Int Footprint = new Vector2Int(2, 2);
        [Tooltip("Стоимость постройки. Не может быть отрицательной.")] public int ConstructionCost = 100;

        [Header("Progression")]
        [Tooltip("Нужно выросших зданий в городе. 0 — доступно сразу")] public int UnlockBuildings = 0;

        [Header("Settlement")]
        [Tooltip("На какой зоне может стоять тип здания.")] public ZoneType AllowedZone = ZoneType.Residential;
        [Tooltip("Сколько жителей вмещает одно здание. Не может быть отрицательным.")] public int ResidentCapacity = 4;
        [Tooltip("Сколько рабочих мест даёт одно здание. Не может быть отрицательным.")] public int JobsProvided = 0;

        [Header("Service")]
        [Tooltip("Тип службы. None — не служба.")] public ServiceKind ServiceKind = ServiceKind.None;
        [Tooltip("Радиус службы в клетках. 0 — не служба.")] public int ServiceRadius = 0;

        [Header("Infrastructure")]
        [Tooltip("Сколько энергии потребляет одно здание.")] public int PowerDemand = 1;
        [Tooltip("Сколько энергии производит одно здание.")] public int PowerOutput = 0;
        [Tooltip("Сколько воды потребляет одно здание.")] public int WaterDemand = 1;
        [Tooltip("Сколько воды производит одно здание.")] public int WaterOutput = 0;
        [Tooltip("Сколько мусора производит одно здание.")] public int WasteOutput = 1;
        [Tooltip("Сколько мусора утилизирует одно здание.")] public int WasteCapacity = 0;

        [Header("Economy")]
        [Tooltip("Содержание здания в час. Не может быть отрицательным.")] public int UpkeepPerHour = 0;

        [Header("Presentation")]
        [Tooltip("Число визуальных вариантов (EPIC-17). Минимум 1.")] public int VariantCount = 1;
        [Tooltip("Префаб здания. Может быть пустым до готовности арта.")] public GameObject Prefab;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(Id)) Debug.LogWarning("BuildingDefinition: id is empty.", this);
            ClampFootprint();
            ConstructionCost = Clamp(ConstructionCost, "cost");
            UnlockBuildings = Clamp(UnlockBuildings, "unlock buildings");
            ResidentCapacity = Clamp(ResidentCapacity, "capacity");
            JobsProvided = Clamp(JobsProvided, "jobs");
            ServiceRadius = Clamp(ServiceRadius, "service radius");
            ClampServiceKind();
            PowerDemand = Clamp(PowerDemand, "power demand");
            PowerOutput = Clamp(PowerOutput, "power output");
            WaterDemand = Clamp(WaterDemand, "water demand");
            WaterOutput = Clamp(WaterOutput, "water output");
            WasteOutput = Clamp(WasteOutput, "waste output");
            WasteCapacity = Clamp(WasteCapacity, "waste capacity");
            UpkeepPerHour = Clamp(UpkeepPerHour, "upkeep");
            if (VariantCount < 1) VariantCount = 1;
        }

        public static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }

        public static ServiceKind SanitizeServiceKind(ServiceKind kind)
        {
            return System.Enum.IsDefined(typeof(ServiceKind), kind) ? kind : ServiceKind.None;
        }

        // Детерминированный выбор варианта по EntityId (EPIC-17 шаг 2).
        public static int SelectVariant(EntityId id, int variantCount)
        {
            int count = variantCount < 1 ? 1 : variantCount;
            int val = id.Value < 0 ? -id.Value : id.Value;
            return val % count;
        }

        private void ClampServiceKind()
        {
            ServiceKind fixedKind = SanitizeServiceKind(ServiceKind);
            if (fixedKind == ServiceKind) return;
            Debug.LogWarning("BuildingDefinition: service kind fixed to None.", this);
            ServiceKind = fixedKind;
        }

        private int Clamp(int value, string field)
        {
            int fixedValue = ClampNonNegative(value);
            if (fixedValue != value) Debug.LogWarning("BuildingDefinition: " + field + " fixed to 0.", this);
            return fixedValue;
        }

        private void ClampFootprint()
        {
            int fixedX = Mathf.Max(1, Footprint.x);
            int fixedY = Mathf.Max(1, Footprint.y);
            if (fixedX == Footprint.x && fixedY == Footprint.y) return;
            Debug.LogWarning("BuildingDefinition: footprint fixed to minimum 1x1.", this);
            Footprint = new Vector2Int(fixedX, fixedY);
        }

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
