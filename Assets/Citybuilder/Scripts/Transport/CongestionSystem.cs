using System;

namespace Citybuilder
{
    // Пробки транспорта (T3, этап 057; EPIC-15 шаги 1 и 3). Чистая логика без Unity.
    // Множитель времени растёт с нагрузкой клетки; ниже порога — без эффекта.
    public class CongestionSystem
    {
        private readonly Func<GridPosition, int> loadOf;
        private readonly GridSystem grid;
        private int threshold;
        private float factor;
        private float maxMultiplier;

        public int Threshold
        {
            get { return threshold; }
            set { threshold = value < 1 ? 1 : value; }
        }

        public float Factor
        {
            get { return factor; }
            set { factor = value < 0f ? 0f : value; }
        }

        public float MaxMultiplier
        {
            get { return maxMultiplier; }
            set { maxMultiplier = value < 1f ? 1f : value; }
        }

        public int CongestionThreshold { get { return Threshold; } set { Threshold = value; } }
        public float CongestionFactor { get { return Factor; } set { Factor = value; } }
        public float Max { get { return MaxMultiplier; } set { MaxMultiplier = value; } }

        public CongestionSystem(RouteSystem routes)
            : this(routes, null, 4, 0.25f, 3.0f) { }

        public CongestionSystem(RouteSystem routes, GridSystem grid)
            : this(routes, grid, 4, 0.25f, 3.0f) { }

        public CongestionSystem(RouteSystem routes, int threshold, float factor, float maxMultiplier)
            : this(routes, null, threshold, factor, maxMultiplier) { }

        public CongestionSystem(RouteSystem routes, GridSystem grid, int threshold, float factor, float maxMultiplier)
            : this(routes != null ? (Func<GridPosition, int>)routes.LoadOf : null, grid, threshold, factor, maxMultiplier) { }

        public CongestionSystem(Func<GridPosition, int> loadOf)
            : this(loadOf, null, 4, 0.25f, 3.0f) { }

        public CongestionSystem(Func<GridPosition, int> loadOf, GridSystem grid)
            : this(loadOf, grid, 4, 0.25f, 3.0f) { }

        public CongestionSystem(Func<GridPosition, int> loadOf, GridSystem grid, int threshold, float factor, float maxMultiplier)
        {
            this.loadOf = loadOf;
            this.grid = grid;
            Threshold = threshold;
            Factor = factor;
            MaxMultiplier = maxMultiplier;
        }

        public CongestionSystem(Func<int, int> loadOf)
            : this((Func<GridPosition, int>)null, null, 4, 0.25f, 3.0f) { }

        public CongestionSystem(Func<int, int> loadOf, int threshold, float factor, float maxMultiplier)
            : this((Func<GridPosition, int>)null, null, threshold, factor, maxMultiplier) { }

        public bool IsCongested(int load) { return load >= Threshold; }

        public float MultiplierFor(int load)
        {
            if (!IsCongested(load)) return 1f;
            float value = 1f + (load - Threshold + 1) * Factor;
            if (value < 1f) value = 1f;
            if (value > MaxMultiplier) value = MaxMultiplier;
            return value;
        }

        public float Load01(GridPosition cell)
        {
            if (loadOf == null || MaxMultiplier <= 1f) return 0f;
            float m = MultiplierFor(loadOf(cell));
            if (float.IsNaN(m) || m <= 1f) return 0f;
            float ratio = (m - 1f) / (MaxMultiplier - 1f);
            if (float.IsNaN(ratio) || ratio < 0f) return 0f;
            return ratio > 1f ? 1f : ratio;
        }

        public float WorstMultiplier()
        {
            if (grid == null || loadOf == null) return 1f;
            float worst = 1f;
            GridSize size = grid.Size;
            for (int x = 0; x < size.Width; x++)
            {
                for (int y = 0; y < size.Height; y++)
                {
                    float m = MultiplierFor(loadOf(new GridPosition(x, y)));
                    if (m > worst) worst = m;
                }
            }
            return worst;
        }

        public int CongestedCellsCount()
        {
            if (grid == null || loadOf == null) return 0;
            int count = 0;
            GridSize size = grid.Size;
            for (int x = 0; x < size.Width; x++)
            {
                for (int y = 0; y < size.Height; y++)
                {
                    if (IsCongested(loadOf(new GridPosition(x, y)))) count++;
                }
            }
            return count;
        }

        public float WorstLoad01()
        {
            if (MaxMultiplier <= 1f) return 0f;
            float w = WorstMultiplier();
            if (float.IsNaN(w) || w <= 1f) return 0f;
            float r = (w - 1f) / (MaxMultiplier - 1f);
            return r > 1f ? 1f : r;
        }

        public int GrowthTickMultiplier()
        {
            int ticks = (int)Math.Round(WorstMultiplier());
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
