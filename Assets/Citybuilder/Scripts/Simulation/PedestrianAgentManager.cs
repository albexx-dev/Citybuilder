using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Пешеходы (EPIC-46): декоративное блуждание по свободным клеткам, 1 агент ≈ 5 жителей.
    // Домен про агентов не знает; спавн у жилых домов, движение — по правилу PedestrianWanderPolicy.
    public class PedestrianAgentManager
    {
        public const int MaxPedestrians = 30;
        public const int ResidentsPerAgent = 5;
        public const float CellsPerSecond = 0.8f;
        public const int MaxStuckSteps = 3;

        // Правило блуждания: тесты подменяют IsBlocked и seed через него.
        public readonly PedestrianWanderPolicy Wander = new PedestrianWanderPolicy();

        // Пешеход: сегмент Current→Next, Progress 0..1, цель вокруг дома спавна.
        public struct PedestrianAgent
        {
            public bool Active;
            public GridPosition Home;
            public GridPosition Current;
            public GridPosition Next;
            public GridPosition Goal;
            public float Progress;
            public int StuckSteps;
        }

        private readonly PedestrianAgent[] agents = new PedestrianAgent[MaxPedestrians];
        private int spawnCursor;

        public int ActiveCount
        {
            get { int count = 0; for (int i = 0; i < MaxPedestrians; i++) if (agents[i].Active) count++; return count; }
        }

        // Агент по индексу слота 0..MaxPedestrians-1 (для видов). Вне диапазона — неактивный.
        public PedestrianAgent AgentAt(int index) => index >= 0 && index < MaxPedestrians ? agents[index] : default(PedestrianAgent);

        // Пересчёт числа агентов от населения (1:5). Вызывается при изменении населения, не каждый тик.
        public void OnPopulationChanged(int residents, IReadOnlyList<GridPosition> homes)
        {
            int desired = Math.Min(Math.Max(residents, 0) / ResidentsPerAgent, MaxPedestrians);
            int active = ActiveCount;
            while (active < desired && TrySpawn(homes)) active++;
            while (active > desired) { DespawnAny(); active--; }
        }

        // Движение по реальным секундам; на паузе хост просто не вызывает Advance.
        public void Advance(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            for (int i = 0; i < MaxPedestrians; i++)
            {
                if (!agents[i].Active) continue;
                PedestrianAgent agent = agents[i];
                agent.Progress += CellsPerSecond * deltaTime;
                agents[i] = agent.Progress < 1f || StepAgent(ref agent) ? agent : default(PedestrianAgent);
            }
        }

        // Полная очистка пула («Новая игра»).
        public void Clear()
        {
            for (int i = 0; i < MaxPedestrians; i++) agents[i] = default(PedestrianAgent);
            spawnCursor = 0;
            Wander.Reset();
        }

        // Спавн у жилого дома: свободная клетка рядом и цель в радиусе. Неудача — тихий пропуск.
        private bool TrySpawn(IReadOnlyList<GridPosition> homes)
        {
            if (homes == null || homes.Count == 0) return false;
            int slot = -1;
            for (int s = 0; s < MaxPedestrians && slot < 0; s++) if (!agents[s].Active) slot = s;
            if (slot < 0) return false;
            for (int i = 0; i < homes.Count; i++)
            {
                GridPosition home = homes[(spawnCursor + i) % homes.Count];
                GridPosition spawn, goal;
                if (!Wander.TrySpawnCell(home, out spawn) || !Wander.TryPickGoal(home, out goal)) continue;
                agents[slot] = new PedestrianAgent { Active = true, Home = home, Current = spawn, Next = spawn, Goal = goal };
                spawnCursor = (spawnCursor + i + 1) % homes.Count;
                return true;
            }
            return false;
        }

        private void DespawnAny()
        {
            for (int i = 0; i < MaxPedestrians; i++)
                if (agents[i].Active) { agents[i] = default(PedestrianAgent); return; }
        }

        // Сегмент завершён: шаг к цели; MaxStuckSteps промахов — новая цель; цели нет — агент исчезает.
        private bool StepAgent(ref PedestrianAgent agent)
        {
            agent.Current = agent.Next;
            if (agent.Current == agent.Goal && !Wander.TryPickGoal(agent.Home, out agent.Goal)) return false;
            GridPosition step;
            if (!Wander.TryStep(agent.Current, agent.Goal, out step))
            {
                agent.StuckSteps++;
                agent.Progress = 0f;
            }
            else
            {
                agent.Next = step;
                agent.StuckSteps = 0;
                agent.Progress -= 1f;
            }
            if (agent.StuckSteps < MaxStuckSteps) return true;
            return Wander.TryPickGoal(agent.Home, out agent.Goal);
        }
    }
}
