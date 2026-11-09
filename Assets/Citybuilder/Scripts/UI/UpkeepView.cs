using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Строка HUD «Расход: N $/ч»: фактическое содержание выросших и построенных зданий.
    // Вешать на Grid: рост и ручная стройка лежат на этом же объекте, казна берётся у роста.
    public class UpkeepView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Автовырастание домов. Пусто = ищется на этом же объекте.")]
        public SettlementGrowthController Growth;
        [Tooltip("Ручная стройка. Пусто = ищется на этом же объекте, без неё учитываются только дома.")]
        public BuildingPlacementController Placement;

        // Цвет строки при дефиците казны: подложка HudText остаётся тёмной.
        private static readonly Color DeficitColor = new Color(1f, 0.45f, 0.45f, 1f);

        // OnGUI вызывается несколько раз за кадр: предупреждение про недостающую ссылку — один раз.
        private bool growthWarned;

        // Подпись для E2E-проверок через мост.
        public string CurrentText
        {
            get { return UpkeepStatusPolicy.Text(CurrentUpkeep()); }
        }

        // Вид строки для E2E-проверок через мост.
        public UpkeepStatusPolicy.Look CurrentLook
        {
            get { return UpkeepStatusPolicy.Pick(InDeficit()); }
        }

        private void OnGUI()
        {
            ResolveRefs();
            WarnIfNoGrowth();
            string text = UpkeepStatusPolicy.Text(CurrentUpkeep());
            // x=258, y=378: справа от кнопки налога Rect(10, 374, 240, 26), там строк HUD нет.
            if (CurrentLook == UpkeepStatusPolicy.Look.Deficit)
            {
                HudText.Draw(258f, 378f, text, DeficitColor);
                return;
            }
            HudText.Draw(258f, 378f, text);
        }

        // Ссылки не обязательны в Inspector: контроллеры живут на том же объекте.
        private void ResolveRefs()
        {
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Placement == null) Placement = GetComponent<BuildingPlacementController>();
        }

        // Без роста число было бы неправдой: предупреждаем один раз, а не каждым кадром OnGUI.
        private void WarnIfNoGrowth()
        {
            if (Growth != null || growthWarned) return;
            growthWarned = true;
            Debug.LogWarning("UpkeepView: нет SettlementGrowthController — строка расхода будет нулевой.", this);
        }

        // Ровно одно списание списков на проход OnGUI.
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
