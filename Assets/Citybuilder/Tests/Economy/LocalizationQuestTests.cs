using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Квесты (issue #100 шаг 2): RU-тексты байт-в-байт совпадают со старыми,
    // генератор вех сохраняет id (сейв v5/v6), а смена языка видна в выданных задачах.
    public class LocalizationQuestTests
    {
        private Func<string> savedLoad;
        private Action<string> savedSave;

        [SetUp]
        public void InjectMemoryStore()
        {
            savedLoad = Localization.LoadPreference;
            savedSave = Localization.SavePreference;
            Localization.LoadPreference = () => string.Empty;
            Localization.SavePreference = value => { };
            Localization.SetLanguage(Language.Ru);
        }

        [TearDown]
        public void RestoreDefaults()
        {
            Localization.SetLanguage(Language.Ru);
            Localization.LoadPreference = savedLoad;
            Localization.SavePreference = savedSave;
        }

        private void AssertRuEn(string key, string ru, string en)
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual(ru, Localization.T(key), key);
            Localization.SetLanguage(Language.En);
            Assert.AreEqual(en, Localization.T(key), key);
        }

        [Test]
        public void QuestPanel_Keys_MatchLegacyRuStrings()
        {
            AssertRuEn("quest_log_title", "=== Задачи города ===", "=== City quests ===");
            AssertRuEn("quest_log_empty", "Задач пока нет.", "No quests yet.");
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("(+200$)", Localization.T("quest_reward", 200));
        }

        [Test]
        public void StarterQuests_MatchLegacyRuStringsAndIds()
        {
            string[] ids = { "q_res_10", "q_grow_5", "q_res_50", "q_park_3", "q_grow_20",
                "q_res_100", "q_fire_cover", "q_police_cover", "q_res_200", "q_health_cover" };
            string[] titles = { "Первые жители", "Застройщик", "Поселение", "Скверы", "Городской район",
                "Растущий город", "Пожарная безопасность", "Правопорядок", "Большой город", "Медицина" };
            for (int i = 0; i < ids.Length; i++)
            {
                Assert.AreEqual(ids[i], QuestCatalog.Starters[i].Id);
                Assert.AreEqual(titles[i], QuestCatalog.Starters[i].Title, ids[i]);
            }
            Assert.AreEqual("Достигни 10 жителей.", QuestCatalog.Starters[0].Description);
            Assert.AreEqual("Вырасти 5 зданий.", QuestCatalog.Starters[1].Description);
            Assert.AreEqual("Построй 3 парка.", QuestCatalog.Starters[3].Description);
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("First residents", QuestCatalog.Starters[0].Title);
            Assert.AreEqual("Reach 10 residents.", QuestCatalog.Starters[0].Description);
        }

        [Test]
        public void MilestoneGenerator_KeepsIdsAndLocalizesBothLanguages()
        {
            HashSet<string> completed = new HashSet<string>();
            for (int i = 0; i < QuestCatalog.Starters.Count; i++) completed.Add(QuestCatalog.Starters[i].Id);
            completed.Add(QuestCatalog.ServicesAllId);
            QuestDefinition quest;
            QuestContext context = new QuestContext();
            context.Residents = 210;
            Assert.IsTrue(QuestCatalog.TryNext(context, completed, new List<string>(), out quest));
            Assert.AreEqual("q_res_ms_250", quest.Id);
            Assert.AreEqual("Веха роста", quest.Title);
            Assert.AreEqual("Достигни 250 жителей.", quest.Description);
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Growth milestone", quest.Title);
            Assert.AreEqual("Reach 250 residents.", quest.Description);
            Assert.IsFalse(QuestCatalog.TryFind(quest.Id, out quest)); // вехи не индексируются сейвом
            Localization.SetLanguage(Language.Ru);

            completed.Add("q_res_ms_250");
            QuestContext rich = new QuestContext();
            rich.Buildings = 12;
            rich.Balance = 6000;
            Assert.IsTrue(QuestCatalog.TryNext(rich, completed, new List<string>(), out quest));
            Assert.AreEqual("q_balance_ms_10000", quest.Id);
            Assert.AreEqual("Достигни баланса $10000.", quest.Description);
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Reach a balance of $10000.", quest.Description);
        }

        [Test]
        public void ProgressLines_MatchLegacyRuStrings()
        {
            QuestContext context = new QuestContext();
            context.Residents = 12;
            QuestDefinition residents = new QuestDefinition("q_res_50", "quest_title_res_50", "quest_desc_residents", 1, c => false);
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Жители 12/50", QuestSystem.ProgressLine(residents, context));
            context.FireCoverage = 0.6f;
            QuestDefinition fire = new QuestDefinition(QuestCatalog.FireCoverId, "t", "d", 1, c => false);
            Assert.AreEqual("Пожарные 60/100", QuestSystem.ProgressLine(fire, context));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Residents 12/50", QuestSystem.ProgressLine(residents, context));
            Assert.AreEqual("Fire 60/100", QuestSystem.ProgressLine(fire, context));
        }
    }
}
