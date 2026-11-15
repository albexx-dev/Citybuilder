using System;

namespace Citybuilder
{
    // Правило блуждания пешехода (EPIC-46): цель в радиусе 6-10 от дома, жадный шаг по свободным клеткам.
    // Делит PedestrianAgentManager: чистая логика выбора клетки, без пула и таймингов.
    public class PedestrianWanderPolicy
    {
        public const int MinWanderRadius = 6;
        public const int MaxWanderRadius = 10;
        public const int GoalAttempts = 8;

        // Занятость клетки: в сцене — GridSystem.IsOccupied, в тестах — fake. null = всё свободно.
        public Func<GridPosition, bool> IsBlocked;

        private static readonly GridPosition[] Offsets =
            { new GridPosition(1, 0), new GridPosition(-1, 0), new GridPosition(0, 1), new GridPosition(0, -1) };

        private readonly Random random;

        public PedestrianWanderPolicy() : this(46) { }

        // Seed делает выбор цели воспроизводимым в тестах.
        public PedestrianWanderPolicy(int seed) { random = new Random(seed); }

        // Случайная свободная цель на манхэттен-расстоянии 6-10 от дома. Максимум GoalAttempts попыток.
        public bool TryPickGoal(GridPosition home, out GridPosition goal)
        {
            for (int attempt = 0; attempt < GoalAttempts; attempt++)
            {
                int dx = random.Next(-MaxWanderRadius, MaxWanderRadius + 1);
                int dy = random.Next(-MaxWanderRadius, MaxWanderRadius + 1);
                int distance = Math.Abs(dx) + Math.Abs(dy);
                GridPosition candidate = new GridPosition(home.X + dx, home.Y + dy);
                if (distance < MinWanderRadius || distance > MaxWanderRadius || IsBlockedCell(candidate)) continue;
                goal = candidate;
                return true;
            }
            goal = home;
            return false;
        }

        // Свободная соседняя клетка дома для спавна (вне карты считается занятой). Нет — false.
        public bool TrySpawnCell(GridPosition home, out GridPosition spawn)
        {
            for (int i = 0; i < Offsets.Length; i++)
            {
                GridPosition cell = home + Offsets[i];
                if (!IsBlockedCell(cell)) { spawn = cell; return true; }
            }
            spawn = home;
            return false;
        }

        // Жадный шаг: свободный сосед, строго сокращающий манхэттен-расстояние до цели. Нет — false.
        public bool TryStep(GridPosition current, GridPosition goal, out GridPosition step)
        {
            int best = Math.Abs(current.X - goal.X) + Math.Abs(current.Y - goal.Y);
            GridPosition choice = current;
            for (int i = 0; i < Offsets.Length; i++)
            {
                GridPosition cell = current + Offsets[i];
                int distance = Math.Abs(cell.X - goal.X) + Math.Abs(cell.Y - goal.Y);
                if (!IsBlockedCell(cell) && distance < best) { best = distance; choice = cell; }
            }
            step = choice;
            return choice != current;
        }

        private bool IsBlockedCell(GridPosition cell) { return IsBlocked != null && IsBlocked(cell); }
    }
}
