using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Снимок покрытия в тике: TickStarted обновляет его до всех TickCompleted
    // (рост/пожар/кража/квесты). Хеш Fire/Crime детерминирован — результат не зависит от скорости.
    public class CoverageTickTests
    {
        private const int SearchFrom = 3;
        private const int SearchLimit = 200000;
        private static readonly GridPosition Home = new GridPosition(12, 12);
        private static readonly GridPosition Station = new GridPosition(12, 12);

        // Свежий снимок: ignite-результат одинаков при Playing и ×4, хотя стартовый снимок устарел.
        [Test]
        public void FreshSnapshot_IgniteResult_SameAtPlayingAndFast()
        {
            EntityId home = EntityId.New();
            int tick = First(FireSystem.ShouldIgnite, home);
            Assert.IsFalse(ResultAtSpeed(SimulationSpeed.Playing, home, tick, true, ServiceKind.Fire,
                FireSystem.ShouldIgnite));
            Assert.IsFalse(ResultAtSpeed(SimulationSpeed.FastForward, home, tick, true, ServiceKind.Fire,
                FireSystem.ShouldIgnite));
        }

        // Свежий снимок: theft-результат одинаков при Playing и ×4.
        [Test]
        public void FreshSnapshot_TheftResult_SameAtPlayingAndFast()
        {
            EntityId home = EntityId.New();
            int tick = First(CrimeSystem.ShouldSteal, home);
            Assert.IsFalse(ResultAtSpeed(SimulationSpeed.Playing, home, tick, true, ServiceKind.Police,
                CrimeSystem.ShouldSteal));
            Assert.IsFalse(ResultAtSpeed(SimulationSpeed.FastForward, home, tick, true, ServiceKind.Police,
                CrimeSystem.ShouldSteal));
        }

        // Устаревший снимок раз в 2 с real-time: ×4 делает 8 тиков за интервал — кража проходит.
        [Test]
        public void StaleIntervalSnapshot_Result_DiffersAcrossSpeeds()
        {
            EntityId home = EntityId.New();
            Assert.IsFalse(ResultAtSpeed(SimulationSpeed.Playing, home, 5, false, ServiceKind.Fire, Always));
            Assert.IsTrue(ResultAtSpeed(SimulationSpeed.FastForward, home, 5, false, ServiceKind.Fire, Always));
        }

        // Порядок: подписчик TickStarted (coverage) всегда до подписчиков TickCompleted.
        [Test]
        public void TickStarted_PrecedesConsumers()
        {
            SimulationClock clock = new SimulationClock();
            List<string> order = new List<string>();
            clock.TickStarted += t => order.Add("coverage");
            clock.TickCompleted += t => order.Add("economy");
            clock.TickCompleted += t => order.Add("quest");
            clock.Update(1f);
            Assert.AreEqual(3, order.Count);
            Assert.AreEqual("coverage", order[0]);
            Assert.AreEqual("economy", order[1]);
            Assert.AreEqual("quest", order[2]);
        }

        // Результат решения в целевом тике (true = возгорание/кража) при политике обновления снимка.
        private static bool ResultAtSpeed(SimulationSpeed speed, EntityId home, int targetTick, bool fresh,
            ServiceKind kind, Func<int, EntityId, bool> shouldOccur)
        {
            SimulationClock clock = new SimulationClock();
            clock.SetSpeed(speed);
            ServiceBuildingSystem services = new ServiceBuildingSystem();
            services.BuildSnapshot(new List<GridPosition> { Home }); // стартовый снимок без станции
            bool result = false;
            int refreshEvery = 2 * (int)speed;
            clock.TickStarted += t =>
            {
                int tick = (int)t.TickCount;
                if (!fresh && (tick == 0 || tick % refreshEvery != 0)) return;
                services.SetSources(kind, new List<ServiceSource> { new ServiceSource(Station, 5) });
                services.BuildSnapshot(new List<GridPosition> { Home });
            };
            clock.TickCompleted += t =>
            {
                if ((int)t.TickCount != targetTick) return;
                result = services.Coverage01(kind, Home) <= 0f && shouldOccur(targetTick, home);
            };
            float step = SimulationClock.SecondsPerTick / (float)speed;
            for (int i = 0; i < targetTick; i++) clock.Update(step);
            return result;
        }

        private static bool Always(int tick, EntityId id) { return true; }

        private static int First(Func<int, EntityId, bool> predicate, EntityId id)
        {
            for (int t = SearchFrom; t < SearchLimit; t++) if (predicate(t, id)) return t;
            Assert.Fail("Нет события за " + SearchLimit + " тиков.");
            return -1;
        }
    }
}
