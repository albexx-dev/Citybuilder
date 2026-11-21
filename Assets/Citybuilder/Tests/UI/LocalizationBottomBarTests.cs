using System;
using NUnit.Framework;

namespace Citybuilder
{
    // Нижняя панель (issue #100 шаг 1): RU-подписи и тултипы байт-в-байт совпадают
    // со старыми захардкоженными строками, у всех новых ключей есть непустой EN.
    public class LocalizationBottomBarTests
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
        public void BottomBar_Labels_MatchLegacyRuStrings()
        {
            AssertRuEn("bottom_speed_pause", "Пауза", "Pause");
            AssertRuEn("bottom_speed_play", "Игра", "Play");
            AssertRuEn("bottom_speed_fast", "x4", "x4");
            AssertRuEn("bottom_mode_off", "Выкл [Esc]", "Off [Esc]");
            AssertRuEn("bottom_mode_houses", "Дома [1]", "Houses [1]");
            AssertRuEn("bottom_mode_roads", "Дороги [2]", "Roads [2]");
            AssertRuEn("bottom_mode_zones", "Зоны [3]", "Zones [3]");
            AssertRuEn("bottom_mode_growth", "Рост [4]", "Growth [4]");
            AssertRuEn("bottom_mode_decor", "Декор [5]", "Decor [5]");
            AssertRuEn("bottom_menu_stats", "Статистика", "Statistics");
            AssertRuEn("bottom_menu_feed", "Лента", "Feed");
            AssertRuEn("bottom_menu_quests", "Задачи", "Quests");
            AssertRuEn("bottom_menu_settings", "Настройки", "Settings");
        }

        [Test]
        public void BottomBar_Tips_MatchLegacyRuStrings()
        {
            AssertRuEn("bottom_tip_speed_pause", "Остановить время", "Stop time");
            AssertRuEn("bottom_tip_speed_play", "Обычная скорость", "Normal speed");
            AssertRuEn("bottom_tip_speed_fast", "Ускорение x4", "Fast forward x4");
            AssertRuEn("bottom_tip_mode_off", "Выйти из режима стройки", "Exit build mode");
            AssertRuEn("bottom_tip_mode_houses", "Строить дома", "Build houses");
            AssertRuEn("bottom_tip_mode_roads", "Строить дороги", "Build roads");
            AssertRuEn("bottom_tip_mode_zones", "Красить зоны", "Paint zones");
            AssertRuEn("bottom_tip_mode_growth", "Ускорять рост домов", "Speed up house growth");
            AssertRuEn("bottom_tip_mode_decor", "Декорации", "Decorations");
            AssertRuEn("bottom_tip_menu_stats", "Сводка города", "City overview");
            AssertRuEn("bottom_tip_menu_feed", "События города", "City events");
            AssertRuEn("bottom_tip_menu_quests", "Задачи-вехи", "Milestone quests");
            AssertRuEn("bottom_tip_menu_settings", "Настройки игры", "Game settings");
            AssertRuEn("bottom_tip_brush_residential", "Жилая зона: растут дома", "Residential zone: houses grow");
            AssertRuEn("bottom_tip_brush_commercial", "Коммерция: магазины", "Commercial: shops");
            AssertRuEn("bottom_tip_brush_industrial", "Промзона: заводы", "Industrial: factories");
        }

        [Test]
        public void BottomBar_AllKeyArrays_AreRegisteredInBothLanguages()
        {
            string[][] arrays = { BottomBarIcons.SpeedLabelKeys, BottomBarIcons.SpeedTipKeys,
                BottomBarIcons.ModeLabelKeys, BottomBarIcons.ModeTipKeys,
                BottomBarIcons.MenuLabelKeys, BottomBarIcons.MenuTipKeys, BottomBarIcons.BrushTipKeys };
            for (int a = 0; a < arrays.Length; a++)
            {
                for (int i = 0; i < arrays[a].Length; i++)
                {
                    string key = arrays[a][i];
                    Assert.IsTrue(Localization.Has(key), key);
                    Localization.SetLanguage(Language.En);
                    Assert.IsFalse(string.IsNullOrEmpty(Localization.T(key)), key);
                }
            }
        }
    }
}
