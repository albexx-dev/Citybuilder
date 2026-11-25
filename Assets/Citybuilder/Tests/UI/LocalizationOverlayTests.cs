using NUnit.Framework;

namespace Citybuilder
{
    // RU/EN-ключи тумблеров оверлеев (issue #114): оба языка дают готовые подписи IMGUI.
    public class LocalizationOverlayTests
    {
        private Language saved;

        [SetUp]
        public void RememberLanguage() { saved = Localization.Language; }

        [TearDown]
        public void RestoreLanguage() { Localization.SetLanguage(saved); }

        [Test]
        public void OverlayToggles_HaveRuAndEnStrings()
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Покрытие", Localization.T("settings_overlay_coverage"));
            Assert.AreEqual("Пробки", Localization.T("settings_overlay_congestion"));
            Assert.AreEqual("Сетка", Localization.T("settings_overlay_grid"));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Coverage", Localization.T("settings_overlay_coverage"));
            Assert.AreEqual("Traffic", Localization.T("settings_overlay_congestion"));
            Assert.AreEqual("Grid", Localization.T("settings_overlay_grid"));
        }
    }
}
