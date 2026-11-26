using System;
using System.Collections.Generic;

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

        // Цели Home/Work/Shop (issue #166): хост задаёт рабочие места и магазины один раз
        // при изменении населения; в Advance только чтение и вращение курсора, без аллокаций.
        private IReadOnlyList<GridPosition> works;
        private IReadOnlyList<GridPosition> shops;
        private int purposeCursor;

        private static readonly GridPosition[] Offsets =
            { new GridPosition(1, 0), new GridPosition(-1, 0), new GridPosition(0, 1), new GridPosition(0, -1) };

        private readonly int seed;
        private int pickSequence;

        public PedestrianWanderPolicy() : this(46) { }

        // Seed и номер серии попыток делают выбор цели воспроизводимым без System.Random.
        public PedestrianWanderPolicy(int seed) { this.seed = seed; }

        // Сброс серии («Новая игра»/загрузка): тот же seed снова даёт те же цели.
        public void Reset() { pickSequence = 0; purposeCursor = 0; }

        // Набор целей: работа и магазин; пустые списки возвращают старое блуждание.
        public void SetDestinations(IReadOnlyList<GridPosition> workCells, IReadOnlyList<GridPosition> shopCells)
        { works = workCells; shops = shopCells; purposeCursor = 0; }

        // Цель вращается Work → Shop → Home; выбор свободной клетки рядом со зданием.
        private bool TryPickPurpose(GridPosition home, out GridPosition goal)
        {
            goal = default(GridPosition);
            int workCount = Count(works), shopCount = Count(shops);
            GridPosition homeCell;
            if (workCount + shopCount == 0 || !TrySpawnCell(home, out homeCell)) return false;
            for (int i = 0; i <= workCount + shopCount; i++)
            {
                int index = purposeCursor + i;
                if (index >= workCount + shopCount)
                {
                    purposeCursor = index + 1;
                    goal = homeCell;
                    return true;
                }
                bool work = workCount > 0 && (shopCount == 0 || index % 2 == 0);
                IReadOnlyList<GridPosition> source = work ? works : shops;
                GridPosition cell;
                if (!TrySpawnCell(source[index % source.Count], out cell)) continue;
                purposeCursor = index + 1;
                goal = cell;
                return true;
            }
            return false;
        }

        private static int Count(IReadOnlyList<GridPosition> list) { return list == null ? 0 : list.Count; }

        // Случайная свободная цель на манхэттен-расстоянии 6-10 от дома. Максимум GoalAttempts попыток.
        public bool TryPickGoal(GridPosition home, out GridPosition goal)
        {
            if (TryPickPurpose(home, out goal)) return true;
            int sequence = pickSequence;
            pickSequence += GoalAttempts;
            for (int attempt = 0; attempt < GoalAttempts; attempt++)
            {
                uint hash = GoalHash(seed, home, sequence + attempt);
                int dx = (int)(hash & 0xFFFF) % (MaxWanderRadius * 2 + 1) - MaxWanderRadius;
                int dy = (int)((hash >> 16) & 0xFFFF) % (MaxWanderRadius * 2 + 1) - MaxWanderRadius;
                int distance = Math.Abs(dx) + Math.Abs(dy);
                GridPosition candidate = new GridPosition(home.X + dx, home.Y + dy);
                if (distance < MinWanderRadius || distance > MaxWanderRadius || IsBlockedCell(candidate)) continue;
                goal = candidate;
                return true;
            }
            goal = home;
            return false;
        }

        // FNV-1a по байтам seed, координат дома и попытки: паттерн FireSystem.Hash (ADR-009).
        private static uint GoalHash(int seed, GridPosition home, int attempt)
        {
            uint hash = 2166136261u;
            hash = MixByte(hash, (byte)seed);
            hash = MixByte(hash, (byte)(seed >> 8));
            hash = MixByte(hash, (byte)(seed >> 16));
            hash = MixByte(hash, (byte)(seed >> 24));
            hash = MixByte(hash, (byte)home.X);
            hash = MixByte(hash, (byte)(home.X >> 8));
            hash = MixByte(hash, (byte)(home.X >> 16));
            hash = MixByte(hash, (byte)(home.X >> 24));
            hash = MixByte(hash, (byte)home.Y);
            hash = MixByte(hash, (byte)(home.Y >> 8));
            hash = MixByte(hash, (byte)(home.Y >> 16));
            hash = MixByte(hash, (byte)(home.Y >> 24));
            hash = MixByte(hash, (byte)attempt);
            hash = MixByte(hash, (byte)(attempt >> 8));
            hash = MixByte(hash, (byte)(attempt >> 16));
            hash = MixByte(hash, (byte)(attempt >> 24));
            return hash;
        }

        private static uint MixByte(uint hash, byte value) { return (hash ^ value) * 16777619u; }

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
