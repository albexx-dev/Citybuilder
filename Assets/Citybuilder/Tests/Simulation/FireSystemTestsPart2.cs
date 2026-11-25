using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса FireSystemTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class FireSystemTests
    {
        // Прогон дома тиков с заданным покрытием; события собираются в список.
        private static FireSystem Run(BuildingState home, int startTick, int ticks, float coverage,
            List<CityEvent> events)
        {
            FireSystem fires = new FireSystem();
            fires.Fired += delegate (CityEvent cityEvent) { events.Add(cityEvent); };
            List<BuildingState> homes = new List<BuildingState> { home };
            for (int t = startTick; t < startTick + ticks; t++)
            {
                fires.Tick(t, homes, delegate { return coverage; }, delegate { });
            }
            return fires;
        }
    }
}
