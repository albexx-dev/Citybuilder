using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты FireSystem (EPIC-43, ADR-009): детерминизм двух прогонов, порог хеша,
    // покрытие и NaN, тушение станцией за 1 тик, разрушение с выселением, форс-хук.
    public class FireSystemTests
    {
        private const int ProbeTicks = 100000;

        // Детерминизм: два прогона по одним и тем же входам — одинаковое решение и события.
        [Test]
        public void TwoRuns_SameInputs_SameOutcomesAndEvents()
        {
            BuildingState home = Home(new GridPosition(5, 5));
            int igniteTick = FindIgniteTick(home.Id);
            List<string> firstEvents = new List<string>();
            List<string> secondEvents = new List<string>();
            FireSystem first = Run(home, igniteTick, 3, 0f, firstEvents);
            FireSystem second = Run(home, igniteTick, 3, 0f, secondEvents);
            Assert.IsTrue(first.IsBurning(home.Id));
            Assert.AreEqual(second.BurningCount, first.BurningCount);
            Assert.AreEqual(firstEvents, secondEvents);
        }

        // Порог возгорания строго по формуле ADR-009 и детерминирован по (tick, EntityId).
        [Test]
        public void ShouldIgnite_FollowsAdrFormula()
        {
            EntityId id = EntityId.New();
            for (int t = 0; t < 1000; t++)
            {
                bool expected = (FireSystem.Hash(t, id) & 0xFFFF) / 65536.0 < FireSystem.IgniteChancePerTick;
                Assert.AreEqual(expected, FireSystem.ShouldIgnite(t, id), "tick " + t);
            }
        }

        // Константы ADR-009 зафиксированы: шанс за тик и срок горения.
        [Test]
        public void Constants_AdrValues()
        {
            Assert.AreEqual(0.002f, FireSystem.IgniteChancePerTick);
            Assert.AreEqual(6, FireSystem.BurnTicks);
        }

        // Покрытый дом не загорается вовсе, даже на тике, где непокрытый бы вспыхнул.
        [Test]
        public void CoveredHome_NeverIgnites()
        {
            BuildingState home = Home(new GridPosition(6, 6));
            int igniteTick = FindIgniteTick(home.Id);
            FireSystem fires = Run(home, igniteTick, 20, 1f, new List<string>());
            Assert.AreEqual(0, fires.BurningCount);
        }

        // NaN в покрытии — безопасная сторона: дом считается накрытым (план тестов эпика).
        [Test]
        public void NaN_Coverage_TreatedAsCovered()
        {
            BuildingState home = Home(new GridPosition(7, 7));
            int igniteTick = FindIgniteTick(home.Id);
            FireSystem fires = Run(home, igniteTick, 20, float.NaN, new List<string>());
            Assert.AreEqual(0, fires.BurningCount);
        }

        // Тушение за 1 тик: станция в радиусе гасит горящий дом, здание живо.
        [Test]
        public void CoveredBurningHome_Extinguished_NoDemolish()
        {
            BuildingState home = Home(new GridPosition(8, 8));
            FireSystem fires = new FireSystem();
            List<string> messages = new List<string>();
            fires.Fired += delegate (EventKind kind, string message, int tick) { messages.Add(message); };
            fires.ForceIgnite(home, 100);
            int demolished = 0;
            fires.Tick(101, new List<BuildingState> { home }, delegate { return 1f; },
                delegate { demolished++; });
            Assert.IsFalse(fires.IsBurning(home.Id));
            Assert.AreEqual(0, demolished);
            Assert.AreEqual(2, messages.Count);
            Assert.IsTrue(messages[1].Contains("потушен"));
        }

        // Разрушение: 6 тиков горения без станции — снос, жители выселены, 2 события (старт и сгорание).
        [Test]
        public void UncoveredBurningHome_DemolishedAfterBurnTicks()
        {
            BuildingState home = Home(new GridPosition(9, 9));
            PopulationSystem population = new PopulationSystem();
            population.MoveIn(home, 4);
            FireSystem fires = new FireSystem();
            List<string> messages = new List<string>();
            fires.Fired += delegate (EventKind kind, string message, int tick) { messages.Add(message); };
            fires.ForceIgnite(home, 100);
            List<BuildingState> burned = new List<BuildingState>();
            for (int t = 101; t <= 106; t++)
            {
                fires.Tick(t, new List<BuildingState> { home }, delegate { return 0f; },
                    delegate (BuildingState b) { burned.Add(b); population.EvictHome(b); });
            }
            Assert.AreEqual(1, burned.Count);
            Assert.AreEqual(home, burned[0]);
            Assert.IsFalse(fires.IsBurning(home.Id));
            Assert.AreEqual(0, population.TotalResidents);
            Assert.AreEqual(2, messages.Count);
            Assert.IsTrue(messages[0].Contains("Пожар в доме #"));
            Assert.IsTrue(messages[1].Contains("сгорел дотла"));
        }

        // Хук форс-возгорания: поджигает один раз, повторный вызов не дублирует.
        [Test]
        public void ForceIgnite_IgnitesOnce()
        {
            BuildingState home = Home(new GridPosition(14, 14));
            FireSystem fires = new FireSystem();
            List<string> messages = new List<string>();
            fires.Fired += delegate (EventKind kind, string message, int tick) { messages.Add(message); };
            fires.ForceIgnite(home, 1);
            fires.ForceIgnite(home, 2);
            Assert.IsTrue(fires.IsBurning(home.Id));
            Assert.AreEqual(1, messages.Count);
        }

        private static BuildingState Home(GridPosition at)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house";
            return new BuildingState(EntityId.New(), def, at, new List<GridPosition> { at });
        }

        // Детерминированный поиск тика возгорания: первый t с порогом хеша (без System.Random).
        private static int FindIgniteTick(EntityId id)
        {
            for (int t = 0; t < ProbeTicks; t++)
            {
                if (FireSystem.ShouldIgnite(t, id)) return t;
            }
            Assert.Fail("Возгорания нет за " + ProbeTicks + " тиков — проверь IgniteChancePerTick.");
            return -1;
        }

        // Прогон дома тиков с заданным покрытием; события собираются в список.
        private static FireSystem Run(BuildingState home, int startTick, int ticks, float coverage,
            List<string> events)
        {
            FireSystem fires = new FireSystem();
            fires.Fired += delegate (EventKind kind, string message, int tick) { events.Add(message); };
            List<BuildingState> homes = new List<BuildingState> { home };
            for (int t = startTick; t < startTick + ticks; t++)
            {
                fires.Tick(t, homes, delegate { return coverage; }, delegate { });
            }
            return fires;
        }
    }
}
