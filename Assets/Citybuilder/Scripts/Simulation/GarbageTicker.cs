using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Связка GarbageSystem с городом на границе тика (EPIC-52): подаёт дома и позицию
    // полигона, пишет события вывоза в ленту. Plain C#: как FireTicker (EPIC-43).
    // Мусор НЕ сериализуется — Reset при отмотке времени назад (как QuestTicker, EPIC-48).
    public class GarbageTicker
    {
        private readonly GarbageSystem garbage = new GarbageSystem();
        private CityEventLog log;
        private int lastTick = -1;

        public GarbageTicker()
        {
            garbage.Collected += OnGarbageEvent;
        }

        // Домен вывоза: для GarbageTruckView и тестов.
        public GarbageSystem Garbage
        {
            get { return garbage; }
        }

        // Один тик: дома, полигон (null = вывоза нет, мусор копится), лента событий.
        public void Tick(int tick, List<BuildingState> homes, GridPosition? depot,
            CityEventLog log, Func<GridPosition, GridPosition, List<GridPosition>> pathFinder = null)
        {
            if (tick < lastTick)
            {
                // Время отмотано назад («Новая игра»/загрузка): мусор не сериализуется до v5 (#64).
                garbage.Reset();
            }
            lastTick = tick;
            this.log = log;
            garbage.Tick(tick, homes, depot, pathFinder);
        }

        // Сброс по команде хоста (LoadController.ClearAll); также срабатывает при отмотке.
        public void Reset()
        {
            lastTick = -1;
            garbage.Reset();
        }

        private void OnGarbageEvent(CityEvent cityEvent)
        {
            if (log != null) log.Add(cityEvent);
        }
    }
}
