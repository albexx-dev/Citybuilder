using UnityEngine;

namespace Citybuilder
{
    // Профиль настроек в PlayerPrefs (EPIC-69 шаг 1): ключи, чтение с дефолтами, запись.
    // Отдельно от GameSettings, чтобы значения и клампы оставались чистым C# без Unity.
    // Ключи громкости/мута совпадают с AudioController (EPIC-19) — старые настройки не теряются.
    // Литералы продублированы, чтобы Core не ссылался на Presentation (без цикла модулей).
    public static class GameSettingsStore
    {
        public const string VolumeKey = "citybuilder_volume";
        public const string AmbientVolumeKey = "citybuilder_ambient";
        public const string MuteKey = "citybuilder_muted";
        public const string AgentLimitKey = "citybuilder_agent_limit";
        public const string DecorKey = "citybuilder_decor";
        public const string DayNightKey = "citybuilder_daynight";
        public const string AutosaveHoursKey = "citybuilder_autosave_hours";
        public const string ServiceCoverageOverlayKey = "citybuilder_overlay_coverage";
        public const string CongestionOverlayKey = "citybuilder_overlay_congestion";
        public const string GridOverlayKey = "citybuilder_overlay_grid";
        public const string GarbageTrucksKey = "citybuilder_garbage_trucks";

        // Прочитать профиль: отсутствующие ключи → дефолты, мусор → клампы GameSettings.
        public static void Load(GameSettings settings)
        {
            if (settings == null) return;
            settings.Volume = PlayerPrefs.GetFloat(VolumeKey, GameSettings.DefaultVolume);
            settings.AmbientVolume = PlayerPrefs.GetFloat(AmbientVolumeKey, GameSettings.DefaultAmbientVolume);
            settings.Muted = PlayerPrefs.GetInt(MuteKey, 0) != 0;
            settings.AgentLimit = PlayerPrefs.GetInt(AgentLimitKey, GameSettings.DefaultAgentLimit);
            settings.DecorEnabled = PlayerPrefs.GetInt(DecorKey, 1) != 0;
            settings.DayNightEnabled = PlayerPrefs.GetInt(DayNightKey, 1) != 0;
            settings.AutosaveHours = PlayerPrefs.GetInt(AutosaveHoursKey, GameSettings.DefaultAutosaveHours);
            settings.ServiceCoverageOverlayEnabled = PlayerPrefs.GetInt(ServiceCoverageOverlayKey, 1) != 0;
            settings.CongestionOverlayEnabled = PlayerPrefs.GetInt(CongestionOverlayKey, 1) != 0;
            settings.GridOverlayEnabled = PlayerPrefs.GetInt(GridOverlayKey, 1) != 0;
            settings.ShowGarbageTrucks = PlayerPrefs.GetInt(GarbageTrucksKey, 0) != 0;
        }

        // Записать профиль. Значения уже клампнуты сеттерами GameSettings.
        public static void Save(GameSettings settings)
        {
            if (settings == null) return;
            PlayerPrefs.SetFloat(VolumeKey, settings.Volume);
            PlayerPrefs.SetFloat(AmbientVolumeKey, settings.AmbientVolume);
            PlayerPrefs.SetInt(MuteKey, settings.Muted ? 1 : 0);
            PlayerPrefs.SetInt(AgentLimitKey, settings.AgentLimit);
            PlayerPrefs.SetInt(DecorKey, settings.DecorEnabled ? 1 : 0);
            PlayerPrefs.SetInt(DayNightKey, settings.DayNightEnabled ? 1 : 0);
            PlayerPrefs.SetInt(AutosaveHoursKey, settings.AutosaveHours);
            PlayerPrefs.SetInt(ServiceCoverageOverlayKey, settings.ServiceCoverageOverlayEnabled ? 1 : 0);
            PlayerPrefs.SetInt(CongestionOverlayKey, settings.CongestionOverlayEnabled ? 1 : 0);
            PlayerPrefs.SetInt(GridOverlayKey, settings.GridOverlayEnabled ? 1 : 0);
            PlayerPrefs.SetInt(GarbageTrucksKey, settings.ShowGarbageTrucks ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}