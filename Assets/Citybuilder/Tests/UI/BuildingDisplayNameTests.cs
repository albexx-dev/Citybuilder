using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты имени здания (задача 89): RU/EN по ключу ассета и фолбэк на DisplayName/Id.
    public class BuildingDisplayNameTests
    {
        private Language savedLanguage;
        private System.Func<string> savedLoad;
        private System.Action<string> savedSave;
        private string stored;

        [SetUp]
        public void InjectMemoryStore()
        {
            savedLanguage = Localization.Language;
            savedLoad = Localization.LoadPreference;
            savedSave = Localization.SavePreference;
            stored = string.Empty;
            Localization.LoadPreference = () => stored;
            Localization.SavePreference = value => stored = value;
            Localization.SetLanguage(Language.Ru);
        }

        [TearDown]
        public void RestoreDefaults()
        {
            Localization.SetLanguage(savedLanguage);
            Localization.LoadPreference = savedLoad;
            Localization.SavePreference = savedSave;
        }

        [Test]
        public void Definition_WithKey_SwitchingLanguage_ChangesName()
        {
            BuildingDefinition def = Create();
            def.DisplayNameKey = "building_house";

            Assert.AreEqual("Дом", BuildingDisplayName.Of(def));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("House", BuildingDisplayName.Of(def));
            Object.DestroyImmediate(def);
        }

        [Test]
        public void Definition_WithoutKey_FallsBackToDisplayName()
        {
            BuildingDefinition def = Create();
            def.DisplayName = "Waste Depot";

            Assert.AreEqual("Waste Depot", BuildingDisplayName.Of(def));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Waste Depot", BuildingDisplayName.Of(def));
            Object.DestroyImmediate(def);
        }

        [Test]
        public void Definition_WithUnknownKey_FallsBackToDisplayName()
        {
            BuildingDefinition def = Create();
            def.DisplayNameKey = "missing_key";
            def.DisplayName = "Factory";

            Assert.AreEqual("Factory", BuildingDisplayName.Of(def));
            Object.DestroyImmediate(def);
        }

        [Test]
        public void Definition_WithoutName_FallsBackToId()
        {
            BuildingDefinition def = Create();
            def.DisplayName = string.Empty;

            Assert.AreEqual("house", BuildingDisplayName.Of(def));
            Object.DestroyImmediate(def);
        }

        [Test]
        public void InfoSnapshot_WithKey_UsesLocalizedName()
        {
            BuildingInfo info = new BuildingInfo();
            info.Id = "power_station";
            info.DisplayName = "Power Station";
            info.DisplayNameKey = "building_power_station";

            Assert.AreEqual("Электростанция", BuildingDisplayName.Of(info));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Power Station", BuildingDisplayName.Of(info));
        }

        [Test]
        public void DecorDefinition_WithKey_SwitchingLanguage_ChangesName()
        {
            DecorDefinition def = ScriptableObject.CreateInstance<DecorDefinition>();
            def.Id = "decor_oak";
            def.DisplayName = "Дуб";
            def.DisplayNameKey = "decor_oak";

            Assert.AreEqual("Дуб", BuildingDisplayName.Of(def));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Oak", BuildingDisplayName.Of(def));
            Object.DestroyImmediate(def);
        }

        [Test]
        public void NullDefinition_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, BuildingDisplayName.Of((BuildingDefinition)null));
        }

        private static BuildingDefinition Create()
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house";
            def.DisplayName = "House";
            return def;
        }
    }
}
