using UnityEngine;

namespace Citybuilder
{
    // Хвост класса SettingsView (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class SettingsView : MonoBehaviour
    {
        private void ApplyToListeners()
        {
            ApplyAudio(settings, Audio);
            if (settings.DayNightEnabled != lastDayNightEnabled) { ApplyDayNight(settings, DayNight); lastDayNightEnabled = settings.DayNightEnabled; }
            SettingsAppliers.ApplyDecor(settings, Decor, Menu);
            SettingsAppliers.ApplyAutosave(settings, Autosave);
            SettingsAppliers.ApplyAmbient(settings, Ambient);
            SettingsAppliers.ApplyOverlays(settings, FindFirstObjectByType<GridVisualizer>(), FindFirstObjectByType<RoadView>());
            SettingsAppliers.ApplyGarbageTrucks(settings, FindFirstObjectByType<GarbageTruckView>());
            SettingsAppliers.ApplyHud(settings);
        }

        // Готовая точка AudioController: SetVolume/SetMuted сами ведут свои ключи PlayerPrefs.
        public static void ApplyAudio(GameSettings source, AudioController audio)
        {
            if (source == null || audio == null) return;
            audio.SetVolume(source.Volume);
            audio.SetMuted(source.Muted);
        }

        // Готовая точка DayNightController: поле читается на каждом тике, Reapply — смена видна на паузе.
        public static void ApplyDayNight(GameSettings source, DayNightController dayNight)
        {
            if (source == null || dayNight == null) return;
            dayNight.DayNightEnabled = source.DayNightEnabled;
            dayNight.Reapply();
        }

        private void ResolveRefs()
        {
            if (Pause == null) Pause = GetComponent<PauseMenuView>() ?? FindFirstObjectByType<PauseMenuView>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Audio == null) Audio = FindFirstObjectByType<AudioController>();
            if (DayNight == null) DayNight = FindFirstObjectByType<DayNightController>();
            if (Decor == null) Decor = FindFirstObjectByType<DecorManager>();
            if (Menu == null) Menu = FindFirstObjectByType<BuildModeMenu>();
            if (Autosave == null) Autosave = FindFirstObjectByType<AutosaveController>();
            if (Ambient == null) Ambient = FindFirstObjectByType<AmbientAudioView>();
            if (Tutorial == null) Tutorial = FindFirstObjectByType<TutorialView>();
        }
    }
}
