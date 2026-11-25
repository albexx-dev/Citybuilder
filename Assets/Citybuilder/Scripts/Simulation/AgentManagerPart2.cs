using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Хвост класса AgentManager (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class AgentManager
    {
        // Движение по реальным секундам; на паузе хост просто не вызывает Advance.
        public void Advance(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            for (int i = 0; i < MaxAgents; i++)
            {
                if (!agents[i].Active) continue;
                VehicleAgent agent = agents[i];
                float factor = CellSpeedFactor != null ? CellSpeedFactor(agent.Path[agent.CellIndex]) : 1f;
                agent.Progress += CellsPerSecond * factor * deltaTime;
                int count = agent.Path.Count;
                int cellIndex = (int)agent.Progress;
                if (cellIndex > count - 1) cellIndex = count - 1;
                if (cellIndex > agent.CellIndex)
                {
                    agent.CellIndex = cellIndex;
                    FeedCongestion(agent.Path[cellIndex]);
                }
                if (agent.Progress >= count - 1)
                {
                    agents[i] = default(VehicleAgent);
                    continue;
                }
                agents[i] = agent;
            }
        }

        // Фид пробок: нагрузка текущей клетки агента как одноклеточная «поездка».
        private void FeedCongestion(GridPosition cell)
        {
            if (congestion == null) return;
            tripCell.Clear();
            tripCell.Add(cell);
            congestion.TrackTrip(tripCell);
        }
    }
}
