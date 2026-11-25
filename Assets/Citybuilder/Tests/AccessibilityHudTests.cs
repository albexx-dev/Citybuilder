using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Доступность HUD (issue #122): кламп масштаба, подложка, ярусы и нижняя панель.
    public class AccessibilityHudTests
    {
        [SetUp]
        public void SetUp()
        {
            HudText.Profile = null;
            PlayerPrefs.DeleteKey(GameSettingsStore.HudFontScaleKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.HighContrastKey);
        }

        [TearDown]
        public void TearDown()
        {
            HudText.Profile = null;
        }

        [Test]
        public void FontScale_ClampsAndFiresOnlyOnRealChange()
        {
            GameSettings settings = new GameSettings();
            int changes = 0;
            settings.Changed += delegate { changes++; };
            settings.HudFontScale = 4f;
            Assert.AreEqual(GameSettings.MaxHudFontScale, settings.HudFontScale, 0.001f);
            settings.HudFontScale = float.NaN;
            Assert.AreEqual(GameSettings.MinHudFontScale, settings.HudFontScale, 0.001f);
            Assert.AreEqual(2, changes);
            settings.HudFontScale = GameSettings.MinHudFontScale;
            Assert.AreEqual(2, changes);
            settings.HudFontScale = 1.25f;
            Assert.AreEqual(3, changes);
        }

        [Test]
        public void ApplyHud_BindsProfileForHudText()
        {
            GameSettings settings = new GameSettings();
            settings.HudFontScale = 1.4f;
            settings.HighContrast = true;
            SettingsAppliers.ApplyHud(settings);
            Assert.AreEqual(settings, HudText.Profile);
            Assert.AreEqual(1.4f, HudText.FontScale, 0.001f);
            Assert.IsTrue(HudText.HighContrast);
            Assert.AreEqual(0.9f, HudText.BackingAlpha, 0.001f);
            settings.HighContrast = false;
            Assert.IsFalse(HudText.HighContrast);
            Assert.AreEqual(0.55f, HudText.BackingAlpha, 0.001f);
        }

        [Test]
        public void WithoutProfile_HudUsesDefaults()
        {
            Assert.AreEqual(1f, HudText.FontScale, 0.001f);
            Assert.IsFalse(HudText.HighContrast);
            Assert.AreEqual(0.55f, HudText.BackingAlpha, 0.001f);
        }

        [Test]
        public void BackingHeight_DefaultMatchesLegacyAndKeepsTiersSeparate()
        {
            SettingsAppliers.ApplyHud(new GameSettings());
            float height = HudLayout.BackingHeight(18f);
            Assert.AreEqual(HudText.BackingRowHeight, height, 0.001f);
            Assert.AreEqual(10f, HudLayout.TopLeftRowY(0), 0.001f);
            Assert.AreEqual(114f, HudLayout.TopLeftRowY(4), 0.001f);
            for (int i = 0; i < 4; i++)
                Assert.LessOrEqual(HudLayout.TopLeftRowY(i) + height, HudLayout.TopLeftRowY(i + 1));
        }

        [Test]
        public void BackingHeight_MaxScale_ClampsToLineStep()
        {
            GameSettings settings = new GameSettings();
            settings.HudFontScale = GameSettings.MaxHudFontScale;
            SettingsAppliers.ApplyHud(settings);
            float height = HudLayout.BackingHeight(40f);
            Assert.AreEqual(HudLayout.TopLeftLineStep - HudLayout.BackingGap, height, 0.001f);
            Assert.Less(height + HudLayout.BackingGap, HudLayout.SubRowOffset);
        }

        [TestCase(600f)]
        [TestCase(800f)]
        [TestCase(1080f)]
        public void BottomHudRows_ClearBottomBar_AtMaxFontScale(float screenHeight)
        {
            GameSettings settings = new GameSettings();
            settings.HudFontScale = GameSettings.MaxHudFontScale;
            SettingsAppliers.ApplyHud(settings);
            Rect bar = HudLayout.BottomBarRect(1280f, screenHeight);
            float backing = HudLayout.BackingHeight(18f);
            for (int i = 0; i < 3; i++)
            {
                float y = HudLayout.FixedOrBottom(454f - i * 24f, i, screenHeight);
                Assert.LessOrEqual(y + backing, bar.y);
            }
            Assert.AreEqual(HudLayout.BarHeight, bar.height, 0.001f);
        }

        [Test]
        public void Store_RoundTripKeepsAccessibilityValues_AndClampsGarbage()
        {
            GameSettings settings = new GameSettings();
            settings.HudFontScale = 1.3f;
            settings.HighContrast = true;
            GameSettingsStore.Save(settings);
            GameSettings loaded = new GameSettings();
            GameSettingsStore.Load(loaded);
            Assert.AreEqual(1.3f, loaded.HudFontScale, 0.001f);
            Assert.IsTrue(loaded.HighContrast);

            PlayerPrefs.SetFloat(GameSettingsStore.HudFontScaleKey, 99f);
            GameSettings garbage = new GameSettings();
            GameSettingsStore.Load(garbage);
            Assert.AreEqual(GameSettings.MaxHudFontScale, garbage.HudFontScale, 0.001f);
        }

        [Test]
        public void ResetToDefaults_ClearsFontScaleAndContrast()
        {
            GameSettings settings = new GameSettings();
            settings.HudFontScale = 1.5f;
            settings.HighContrast = true;
            settings.ResetToDefaults();
            Assert.AreEqual(GameSettings.DefaultHudFontScale, settings.HudFontScale, 0.001f);
            Assert.IsFalse(settings.HighContrast);
        }

        [Test]
        public void Localization_HasRuAndEnStringsForNewControls()
        {
            Dictionary<string, string[]> table = new Dictionary<string, string[]>();
            LocalizationTable.AddTo(table);
            Assert.IsTrue(table.ContainsKey("settings_hud_scale"));
            Assert.IsTrue(table.ContainsKey("settings_high_contrast"));
            Assert.AreEqual("Шрифт 100%", string.Format(table["settings_hud_scale"][0], 100));
            Assert.AreEqual("Font 100%", string.Format(table["settings_hud_scale"][1], 100));
        }
    }
}
