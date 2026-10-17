using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Пробки транспорта (T3, этап 057). Чистая логика, никем не используется.
    // Множитель времени растёт с нагрузкой клетки; ниже порога — без эффекта.
    public class CongestionSystem
    {
        private Func<GridPosition, int> loadOf;
        private GridSystem grid;

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

        public int CongestionThreshold
        {
            get { return Threshold; }
            set { Threshold = value; }
        }

        public float CongestionFactor
        {
            get { return Factor; }
            set { Factor = value; }
        }

        public float Max
        {
            get { return MaxMultiplier; }
            set { MaxMultiplier = value; }
        }

        public CongestionSystem(RouteSystem routes)
            : this(routes, null, 4, 0.25f, 3.0f) { }

        public CongestionSystem(RouteSystem routes, GridSystem grid)
            : this(routes, grid, 4, 0.25f, 3.0f) { }

        public CongestionSystem(RouteSystem routes, int threshold, float factor, float maxMultiplier)
            : this(routes, null, threshold, factor, maxMultiplier) { }

        public CongestionSystem(RouteSystem routes, GridSystem grid, int threshold, float factor, float maxMultiplier)
        {
            Func<GridPosition, int> provider = null;
            if (routes != null)
            {
                provider = routes.LoadOf;
            }
            loadOf = provider;
            this.grid = grid;
            Threshold = threshold;
            Factor = factor;
            MaxMultiplier = maxMultiplier;
        }

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

        public bool IsCongested(int load)
        {
            return load >= Threshold;
        }

        public float MultiplierFor(int load)
        {
            if (!IsCongested(load))
            {
                return 1f;
            }
            float value = 1f + (load - Threshold + 1) * Factor;
            if (value < 1f)
            {
                value = 1f;
            }
            if (value > MaxMultiplier)
            {
                value = MaxMultiplier;
            }
            return value;
        }

        public float WorstMultiplier()
        {
            if (grid == null || loadOf == null)
            {
                return 1f;
            }
            float worst = 1f;
            GridSize size = grid.Size;
            for (int x = 0; x < size.Width; x++)
            {
                for (int y = 0; y < size.Height; y++)
                {
                    int load = loadOf(new GridPosition(x, y));
                    float m = MultiplierFor(load);
                    if (m > worst)
                    {
                        worst = m;
                    }
                }
            }
            return worst;
        }

        public int GrowthTickMultiplier()
        {
            int ticks = (int)Math.Round(WorstMultiplier());
            if (ticks < 1)
            {
                ticks = 1;
            }
            return ticks;
        }
    }
}
