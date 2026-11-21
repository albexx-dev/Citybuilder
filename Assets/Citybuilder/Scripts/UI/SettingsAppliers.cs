namespace Citybuilder
{
    // Применение настроек к системам шага 3 (EPIC-69): образец — SettingsView.ApplyAudio/ApplyDayNight.
    // Зовётся из SettingsView.ApplyToListeners при каждом реальном изменении профиля.
    public static class SettingsAppliers
    {
        // Готовые точки декора: DecorManager прячет корень ручного декора,
        // BuildModeMenu гасит режим стройки декора и кнопку «Декор [5]» (образец StatsEnabled).
        public static void ApplyDecor(GameSettings source, DecorManager decor, BuildModeMenu menu)
        {
            if (source == null) return;
            if (decor != null) decor.SetDecorVisible(source.DecorEnabled);
            if (menu != null) menu.SetDecorBuildEnabled(source.DecorEnabled);
        }

        // Готовая точка AutosaveController: интервал в тиках, 1 тик = 1 игровой час.
        public static void ApplyAutosave(GameSettings source, AutosaveController autosave)
        {
            if (source == null || autosave == null) return;
            autosave.AutosaveIntervalTicks = source.AutosaveHours;
        }

        // Эмбиент (issue #95): множитель громкости кроссфейда AmbientAudioView.
        // Применяется из SettingsView.ApplyToListeners, без чтения профиля каждый кадр.
        public static void ApplyAmbient(GameSettings source, AmbientAudioView ambient)
        {
            if (source == null || ambient == null) return;
            ambient.AmbientVolume = source.AmbientVolume;
        }
    }
}
