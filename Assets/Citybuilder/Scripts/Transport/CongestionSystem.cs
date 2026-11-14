using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Пробки транспорта (T3, этап 057; EPIC-15 шаги 1 и 3). Чистая логика без Unity.
    // Множитель времени растёт с нагрузкой клетки; ниже порога — без эффекта.
    public class CongestionSystem
    {
        private readonly Func<GridPosition, int> loadOf;
        private readonly GridSystem grid;
        private readonly Dictionary<GridPosition, int> recordedLoads = new Dictionary<GridPosition, int>();
        private int threshold;
        private float factor;
        private float maxMultiplier;

        public int Threshold { get => threshold; set => threshold = value < 1 ? 1 : value; }
        public float Factor { get => factor; set => factor = float.IsNaN(value) || value < 0f ? 0f : value; }
        public float MaxMultiplier { get => maxMultiplier; set => maxMultiplier = float.IsNaN(value) || value < 1f ? 1f : value; }
        public int CongestionThreshold { get => Threshold; set => Threshold = value; }
        public float CongestionFactor { get => Factor; set => Factor = value; }
        public float Max { get => MaxMultiplier; set => MaxMultiplier = value; }
        public CongestionSystem(RouteSystem routes) : this(routes, null, 4, 0.25f, 3.0f) { }
        public CongestionSystem(RouteSystem routes, GridSystem grid) : this(routes, grid, 4, 0.25f, 3.0f) { }
        public CongestionSystem(RouteSystem routes, int threshold, float factor, float maxMultiplier) : this(routes, null, threshold, factor, maxMultiplier) { }
        public CongestionSystem(RouteSystem routes, GridSystem grid, int threshold, float factor, float maxMultiplier) : this(routes != null ? (Func<GridPosition, int>)routes.LoadOf : null, grid, threshold, factor, maxMultiplier) { }
        public CongestionSystem(Func<GridPosition, int> loadOf) : this(loadOf, null, 4, 0.25f, 3.0f) { }
        public CongestionSystem(Func<GridPosition, int> loadOf, GridSystem grid) : this(loadOf, grid, 4, 0.25f, 3.0f) { }
        public CongestionSystem(Func<GridPosition, int> loadOf, GridSystem grid, int threshold, float factor, float maxMultiplier)
        {
            this.loadOf = loadOf;
            this.grid = grid;
            Threshold = threshold;
            Factor = factor;
            MaxMultiplier = maxMultiplier;
        }

        public CongestionSystem(Func<int, int> loadOf) : this(pos => loadOf != null ? loadOf(pos.X) : 0, null, 4, 0.25f, 3.0f) { }
        public CongestionSystem(Func<int, int> loadOf, int threshold, float factor, float maxMultiplier) : this(pos => loadOf != null ? loadOf(pos.X) : 0, null, threshold, factor, maxMultiplier) { }

        public void TrackTrip(IReadOnlyList<GridPosition> path)
        {
            if (path == null || path.Count == 0) return;
            for (int i = 0; i < path.Count; i++)
            {
                GridPosition cell = path[i];
                int c;
                recordedLoads.TryGetValue(cell, out c);
                recordedLoads[cell] = c + 1;
            }
        }

        public void ClearLoads() => recordedLoads.Clear();

        public int GetLoad(GridPosition cell)
        {
            int b = loadOf != null ? loadOf(cell) : 0;
            int r;
            recordedLoads.TryGetValue(cell, out r);
            return b > r ? b : r;
        }

        public bool IsCongested(int load) => load >= Threshold;

        public float MultiplierFor(int load)
        {
            if (float.IsNaN(factor) || float.IsNaN(maxMultiplier) || !IsCongested(load)) return 1f;
            float value = 1f + (load - Threshold + 1) * Factor;
            if (float.IsNaN(value) || value < 1f) return 1f;
            if (value > MaxMultiplier) return MaxMultiplier;
            return value;
        }

        public float Load01(GridPosition cell)
        {
            if (MaxMultiplier <= 1f) return 0f;
            float m = MultiplierFor(GetLoad(cell));
            if (float.IsNaN(m) || m <= 1f) return 0f;
            float ratio = (m - 1f) / (MaxMultiplier - 1f);
            if (float.IsNaN(ratio) || ratio < 0f) return 0f;
            return ratio > 1f ? 1f : ratio;
        }

        public float WorstMultiplier()
        {
            float worst = 1f;
            if (grid != null)
            {
                GridSize size = grid.Size;
                for (int x = 0; x < size.Width; x++)
                for (int y = 0; y < size.Height; y++)
                {
                    float m = MultiplierFor(GetLoad(new GridPosition(x, y)));
                    if (!float.IsNaN(m) && m > worst) worst = m;
                }
            }
            else if (recordedLoads.Count > 0)
            {
                foreach (KeyValuePair<GridPosition, int> pair in recordedLoads)
                {
                    float m = MultiplierFor(GetLoad(pair.Key));
                    if (!float.IsNaN(m) && m > worst) worst = m;
                }
            }
            return float.IsNaN(worst) ? 1f : worst;
        }

        public int CongestedCellsCount()
        {
            if (grid == null) return 0;
            int count = 0;
            GridSize size = grid.Size;
            for (int x = 0; x < size.Width; x++)
            for (int y = 0; y < size.Height; y++)
            {
                if (IsCongested(GetLoad(new GridPosition(x, y)))) count++;
            }
            return count;
        }

        public float WorstLoad01()
        {
            if (MaxMultiplier <= 1f) return 0f;
            float w = WorstMultiplier();
            if (float.IsNaN(w) || w <= 1f) return 0f;
            float r = (w - 1f) / (MaxMultiplier - 1f);
            if (float.IsNaN(r) || r < 0f) return 0f;
            return r > 1f ? 1f : r;
        }

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
