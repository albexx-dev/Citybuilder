using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Данные строки «Расход: N $/ч»: фактическое содержание выросших и построенных зданий.
    // Отрисовка перенесена в InfoMenuView (EPIC-14 шаг 2); здесь остаются CurrentText/CurrentLook.
    public class UpkeepView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Автовырастание домов. Пусто = ищется на этом же объекте.")]
        public SettlementGrowthController Growth;
        [Tooltip("Ручная стройка. Пусто = ищется на этом же объекте, без неё учитываются только дома.")]
        public BuildingPlacementController Placement;

        // Цвет строки при дефиците казны.
        public static readonly Color DeficitColor = new Color(1f, 0.45f, 0.45f, 1f);

        private bool growthWarned;

        // Подпись для InfoMenuView и E2E-проверок через мост.
        public string CurrentText
        {
            get
            {
                ResolveRefs();
                WarnIfNoGrowth();
                return UpkeepStatusPolicy.Text(CurrentUpkeep());
            }
        }

        // Вид строки для InfoMenuView и E2E-проверок через мост.
        public UpkeepStatusPolicy.Look CurrentLook
        {
            get
            {
                ResolveRefs();
                return UpkeepStatusPolicy.Pick(InDeficit());
            }
        }

        // Ссылки не обязательны в Inspector: контроллеры живут на том же объекте.
        private void ResolveRefs()
        {
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Placement == null) Placement = GetComponent<BuildingPlacementController>();
        }

        // Без роста число было бы неправдой: предупреждаем один раз.
        private void WarnIfNoGrowth()
        {
            if (Growth != null || growthWarned) return;
            growthWarned = true;
            Debug.LogWarning("UpkeepView: нет SettlementGrowthController — строка расхода будет нулевой.", this);
        }

        private int CurrentUpkeep()
        {
            List<BuildingState> grown = Growth != null ? Growth.Homes : null;
            List<BuildingState> placed = Placement != null ? Placement.Placed : null;
            return UpkeepPolicy.UpkeepPerHour(grown, placed);
        }

        // Казна идёт через Growth.Treasury: отдельной ссылки в сцене не требуется.
        private bool InDeficit()
        {
            if (Growth == null || Growth.Treasury == null || Growth.Treasury.Treasury == null) return false;
            return Growth.Treasury.Treasury.IsInDeficit;
        }
    }
}
