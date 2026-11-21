using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты CrimeSystem (EPIC-47): детерминизм двух прогонов, формула порога, покрытие ≥ 0.5
    // и NaN как безопасная сторона, константы эпика, соль хеша против пожаров, запрет
    // повторной кражи, временный −досуг с восстановлением через 12 тиков.
    public class CrimeSystemTests
    {
        private const int ProbeTicks = 200000;

        // Детерминизм: два прогона по одним и тем же входам — одинаковые кражи и события.
        [Test]
        public void TwoRuns_SameInputs_SameTheftsAndEvents()
        {
            BuildingState home = Home(new GridPosition(5, 5));
            int robTick = FindRobTick(home.Id);
            List<CityEvent> firstEvents = new List<CityEvent>();
            List<CityEvent> secondEvents = new List<CityEvent>();
            CrimeSystem first = Run(home, robTick, 3, 0f, firstEvents);
            CrimeSystem second = Run(home, robTick, 3, 0f, secondEvents);
            Assert.AreEqual(1, first.TotalThefts);
            Assert.AreEqual(second.TotalThefts, first.TotalThefts);
            Assert.AreEqual(firstEvents, secondEvents);
            Assert.IsTrue(first.HasActivePenalty(home.Id, robTick));
        }

        // Порог кражи строго по формуле эпика и детерминирован по (tick, EntityId).
        [Test]
        public void ShouldSteal_FollowsFormula()
        {
            EntityId id = EntityId.New();
            for (int t = 0; t < 1000; t++)
            {
                bool expected = (CrimeSystem.Hash(t, id) & 0xFFFF) / 65536.0 < CrimeSystem.TheftChancePerTick;
                Assert.AreEqual(expected, CrimeSystem.ShouldSteal(t, id), "tick " + t);
            }
        }

        // Константы эпика зафиксированы: шанс за тик, порог покрытия, штраф, модификатор.
        [Test]
        public void Constants_EpicValues()
        {
            Assert.AreEqual(0.0005f, CrimeSystem.TheftChancePerTick);
            Assert.AreEqual(0.5f, CrimeSystem.SafeCoverage);
            Assert.AreEqual(20, CrimeSystem.FinePerTheft);
            Assert.AreEqual(0.2f, CrimeSystem.LeisurePenalty);
            Assert.AreEqual(12, CrimeSystem.LeisurePenaltyTicks);
        }

        // Дом с покрытием ≥ 0.5 не обокрадут, даже на тике, где непокрытый бы был обворован.
        [Test]
        public void CoveredHome_NeverRobbed()
        {
            BuildingState home = Home(new GridPosition(6, 6));
            int robTick = FindRobTick(home.Id);
            CrimeSystem crimes = Run(home, robTick, 20, 0.5f, new List<CityEvent>());
            Assert.AreEqual(0, crimes.TotalThefts);
        }

        // NaN в покрытии — безопасная сторона: кражи нет (план тестов эпика).
        [Test]
        public void NaN_Coverage_TreatedAsSafe()
        {
            BuildingState home = Home(new GridPosition(7, 7));
            int robTick = FindRobTick(home.Id);
            CrimeSystem crimes = Run(home, robTick, 20, float.NaN, new List<CityEvent>());
            Assert.AreEqual(0, crimes.TotalThefts);
        }

        // Соль хеша: след краж отличается от следа пожаров — события систем не совпадают.
        [Test]
        public void CrimeHash_DiffersFromFireHash()
        {
            EntityId id = EntityId.New();
            for (int t = 0; t < 1000; t++)
            {
                Assert.AreNotEqual(FireSystem.Hash(t, id), CrimeSystem.Hash(t, id), "tick " + t);
            }
        }

        // Дом с активным модификатором досуга не крадут повторно: дубликатов событий нет.
        [Test]
        public void ActivePenalty_PreventsDuplicateTheft()
        {
            BuildingState home = Home(new GridPosition(8, 8));
            CrimeSystem crimes = new CrimeSystem();
            List<CityEvent> messages = new List<CityEvent>();
            crimes.Fired += delegate (CityEvent cityEvent) { messages.Add(cityEvent); };
            crimes.ForceSteal(home, 100, delegate { });
            crimes.ForceSteal(home, 105, delegate { });
            Assert.AreEqual(1, crimes.TotalThefts);
            Assert.AreEqual(1, messages.Count);
            crimes.ForceSteal(home, 112, delegate { });
            Assert.AreEqual(2, crimes.TotalThefts);
            Assert.AreEqual(2, messages.Count);
        }

        // Временный −досуг: 0.2 сразу после кражи, жив до 11 тиков, восстановился на 12-й.
        [Test]
        public void LeisurePenalty_LastsTwelveTicks_ThenRecovers()
        {
            BuildingState home = Home(new GridPosition(9, 9));
            CrimeSystem crimes = new CrimeSystem();
            crimes.ForceSteal(home, 100, delegate { });
            Assert.AreEqual(0.2f, crimes.LeisurePenalty01(home.Id, 100));
            Assert.AreEqual(0.2f, crimes.LeisurePenalty01(home.Id, 111));
            Assert.AreEqual(0f, crimes.LeisurePenalty01(home.Id, 112));
            Assert.IsFalse(crimes.HasActivePenalty(home.Id, 112));
        }

        private static BuildingState Home(GridPosition at)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house";
            return new BuildingState(EntityId.New(), def, at, new List<GridPosition> { at });
        }

        // Детерминированный поиск тика кражи: первый t с порогом хеша (без System.Random).
        private static int FindRobTick(EntityId id)
        {
            for (int t = 0; t < ProbeTicks; t++)
            {
                if (CrimeSystem.ShouldSteal(t, id)) return t;
            }
            Assert.Fail("Кражи нет за " + ProbeTicks + " тиков — проверь TheftChancePerTick.");
            return -1;
        }

        // Прогон дома тиков с заданным покрытием; события собираются в список.
        private static CrimeSystem Run(BuildingState home, int startTick, int ticks, float coverage,
            List<CityEvent> events)
        {
            CrimeSystem crimes = new CrimeSystem();
            crimes.Fired += delegate (CityEvent cityEvent) { events.Add(cityEvent); };
            List<BuildingState> homes = new List<BuildingState> { home };
            for (int t = startTick; t < startTick + ticks; t++)
            {
                crimes.Tick(t, homes, delegate { return coverage; }, 1f, delegate { });
            }
            return crimes;
        }
    }
}
