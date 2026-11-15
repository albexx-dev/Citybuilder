using System.Collections.Generic;

namespace Citybuilder
{
    // Собирает рабочие места по зонам из списков зданий (эпик #49). Чистая логика без Unity:
    // выросшие здания (SettlementGrowthController.Homes) и ручные постройки (Placement.Placed).
    public static class ZonalJobSource
    {
        // Считает места обоих списков. Пустые списки и null — пропуск.
        public static ZonalJobs Count(List<BuildingState> grown, List<BuildingState> manual)
        {
            ZonalJobs jobs = new ZonalJobs();
            AddAll(jobs, grown);
            AddAll(jobs, manual);
            return jobs;
        }

        private static void AddAll(ZonalJobs jobs, List<BuildingState> buildings)
        {
            if (buildings == null)
            {
                return;
            }
            for (int i = 0; i < buildings.Count; i++)
            {
                jobs.AddBuilding(buildings[i]);
            }
        }
    }
}
