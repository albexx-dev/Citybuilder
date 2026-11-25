using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Хвост класса CongestionSystem (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class CongestionSystem
    {
        public int GrowthTickMultiplier()
        {
            float worst = WorstMultiplier();
            if (float.IsNaN(worst)) return 1;
            int ticks = (int)Math.Round(worst);
            return ticks < 1 ? 1 : ticks;
        }

        // Форматирует строку для инфоменю (EPIC-15 шаг 3): «Пробки: N (худшая клетка Z%, рост ×M)».
        public string FormatSummaryLine()
        {
            int count = CongestedCellsCount();
            int pct = (int)Math.Round(WorstLoad01() * 100f);
            int mult = GrowthTickMultiplier();
            return "Пробки: " + count + " (худшая клетка " + pct + "%, рост ×" + mult + ")";
        }
    }
}
