using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса LoadController (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class LoadController : MonoBehaviour
    {
        private List<BuildingState> ApplyBuildings(CitySnapshot snapshot)
        {
            List<BuildingState> homes = new List<BuildingState>();
            if (Growth == null || snapshot.Buildings == null) return homes;
            for (int i = 0; i < snapshot.Buildings.Count; i++)
            {
                if (snapshot.Buildings[i] == null) continue;
                BuildingState state = Growth.RestoreGrown(snapshot.Buildings[i].DefId, new GridPosition(snapshot.Buildings[i].X, snapshot.Buildings[i].Y));
                if (state != null) homes.Add(state);
            }
            return homes;
        }

        private int ApplyManual(CitySnapshot snapshot)
        {
            if (Placement == null || snapshot.ManualBuildings == null) return 0;
            int restored = 0;
            for (int i = 0; i < snapshot.ManualBuildings.Count; i++)
            {
                CitySnapshot.BuildingEntry entry = snapshot.ManualBuildings[i];
                if (entry != null && Placement.RestoreManual(entry.DefId, new GridPosition(entry.X, entry.Y))) restored++;
            }
            return restored;
        }

        // v6 (ADR-014): без событий и наград — очередь, счётчик и стройка продолжаются с файла.
        private void ApplyV6(CitySnapshot snapshot, List<BuildingState> homes)
        {
            GarbageTruckView garbage = FindFirstObjectByType<GarbageTruckView>();
            if (garbage != null) { garbage.Garbage.Restore(SaveV6Snapshot.ReadGarbage(snapshot), homes); garbage.RefreshDepot(); }
            EconomyTicker economy = FindFirstObjectByType<EconomyTicker>();
            if (economy != null) economy.Crimes.RestoreTotal(SaveV6Snapshot.ReadThefts(snapshot));
            ConstructionSnapshot.RestoreAll(snapshot, Growth, Zones == null ? null : Zones.GridVisualizer, Placement);
        }

        private void ApplyMeta(CitySnapshot snapshot)
        {
            if (Treasury != null) Treasury.ResetBalance(snapshot.Balance);
            if (Runner != null && Runner.Clock != null) Runner.Clock.RestoreTime(snapshot.TickCount);
            if (Policy != null) SnapshotTaxRates.ApplyTo(snapshot, Policy.Policy);
            GoalView goal = FindFirstObjectByType<GoalView>();
            if (goal != null) goal.RestoreStage(snapshot.GoalStage);
            // Хвост #48: квесты после сброса в ClearAll — активные и выполненные из слота v5.
            // Пустые списки (старый слот) — цепочка стартует заново на следующем тике.
            QuestLogView questLog = FindFirstObjectByType<QuestLogView>();
            if (questLog != null) QuestSnapshot.Apply(snapshot, questLog.Quests);
        }
    }
}
