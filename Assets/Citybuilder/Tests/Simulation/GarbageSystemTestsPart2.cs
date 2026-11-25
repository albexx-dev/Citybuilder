using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса GarbageSystemTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class GarbageSystemTests
    {
        // Fake-маршрут из двух клеток: ноги конкатенируются без дублей стыковых клеток.
        private static List<GridPosition> FakePath(GridPosition from, GridPosition to)
        {
            return new List<GridPosition> { from, to };
        }

        // Детерминированный поиск тика готовности: первый t, когда сумма прироста ≥ порога.
        private static int ReadyTick(EntityId id)
        {
            int total = 0;
            for (int t = 0; t < 10000; t++)
            {
                total += GarbageSystem.GainPerTick(t, id, 1);
                if (total >= GarbageSystem.ReadyThreshold) return t;
            }
            Assert.Fail("Дом не готов к вывозу за 10000 тиков — проверь GainPerTick.");
            return -1;
        }

        // Прогон тиков; depot/pathFinder опциональны, события собираются в список.
        private static GarbageSystem Run(List<BuildingState> homes, int ticks, GridPosition? depot,
            Func<GridPosition, GridPosition, List<GridPosition>> pathFinder, List<CityEvent> events)
        {
            GarbageSystem garbage = new GarbageSystem();
            garbage.Collected += delegate (CityEvent cityEvent) { events.Add(cityEvent); };
            for (int t = 0; t < ticks; t++) garbage.Tick(t, homes, depot, pathFinder);
            return garbage;
        }
    }
}
