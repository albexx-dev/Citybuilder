using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Строка HUD первых шагов: что нажать, пока не вырос первый дом. Только OnGUI.
    // Вешать на Grid: зоны, рост и меню ищутся на этом же объекте.
    public class StartHintView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Кисть зон. Пусто = ищется на этом же объекте.")]
        public ZonePaintingController Zones;
        [Tooltip("Автовырастание домов. Пусто = ищется на этом же объекте.")]
        public SettlementGrowthController Growth;
        [Tooltip("Меню режимов. Пусто = ищется на этом же объекте.")]
        public BuildModeMenu Menu;

        // Для E2E-моста: текущий шаг без разбора пикселей.
        public StartHintPolicy.Step CurrentStep
        {
            get { return PickStep(); }
        }

        // Для E2E-моста: текущая строка или пустая, когда подсказка не нужна.
        public string CurrentText
        {
            get { return BuildText(CurrentStep); }
        }

        private void OnGUI()
        {
            string text = CurrentText;
            if (string.IsNullOrEmpty(text)) return;
            // y=430: ниже «Дальше» (406) и выше нижней кромки при 720p.
            HudText.Draw(10, 430, text);
        }

        private StartHintPolicy.Step PickStep()
        {
            ResolveRefs();
            int residential = CountResidential();
            int grown = Growth != null ? Growth.GrownCount : 0;
            bool growthMode = Menu != null && Menu.CurrentMode == BuildModeMenu.Mode.Growth;
            return StartHintPolicy.Pick(residential, grown, growthMode);
        }

        private void ResolveRefs()
        {
            if (Zones == null) Zones = GetComponent<ZonePaintingController>();
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Menu == null) Menu = GetComponent<BuildModeMenu>();
        }

        private int CountResidential()
        {
            ZoneSystem zones = Zones != null ? Zones.Zones : null;
            if (zones == null) return 0;
            List<KeyValuePair<GridPosition, ZoneType>> cells = zones.GetZonedCells();
            int count = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].Value == ZoneType.Residential) count++;
            }
            return count;
        }

        private static string BuildText(StartHintPolicy.Step step)
        {
            switch (step)
            {
                case StartHintPolicy.Step.PaintZones:
                    return "Старт: [3] Зоны → «Жилая» → закрась 4×4 клетки";
                case StartHintPolicy.Step.EnableGrowth:
                    return "Дальше: [4] Рост — дома построятся сами";
                case StartHintPolicy.Step.WaitForHouse:
                    return "Первый дом: подожди несколько часов (▶▶ ускоряет)";
                default:
                    return string.Empty;
            }
        }
    }
}
