using System;

namespace Citybuilder
{
    // Настройки игрока (EPIC-69 шаг 1): профиль, а не состояние партии — в сейв города не пишется.
    // Чистый C#: значения, клампы и событие Changed для систем-слушателей.
    // Чтение/запись PlayerPrefs — GameSettingsStore; окно настроек — следующий шаг.
    public class GameSettings
    {
        public const float DefaultVolume = 0.5f;
        public const float DefaultAmbientVolume = 1f;
        public const int DefaultAgentLimit = 12;
        public const int MinAgentLimit = 8;
        public const int MaxAgentLimit = 120;
        public const int DefaultAutosaveHours = 24;
        public const int MinAutosaveHours = 5;
        public const int MaxAutosaveHours = 120;

        private float volume = DefaultVolume;
        private float ambientVolume = DefaultAmbientVolume;
        private bool muted;
        private int agentLimit = DefaultAgentLimit;
        private bool decorEnabled = true;
        private bool dayNightEnabled = true;
        private int autosaveHours = DefaultAutosaveHours;

        // Системы переприменяют значения на лету; вызывается только при реальном изменении.
        public event Action Changed;

        public float Volume
        {
            get { return volume; }
            set { Set(ref volume, ClampVolume(value)); }
        }

        public bool Muted
        {
            get { return muted; }
            set { Set(ref muted, value); }
        }

        // Множитель громкости эмбиента AmbientAudioView (0..1), дефолт 1 — как в EPIC-71.
        public float AmbientVolume
        {
            get { return ambientVolume; }
            set { Set(ref ambientVolume, ClampVolume(value)); }
        }

        public int AgentLimit
        {
            get { return agentLimit; }
            set { Set(ref agentLimit, ClampAgentLimit(value)); }
        }

        public bool DecorEnabled
        {
            get { return decorEnabled; }
            set { Set(ref decorEnabled, value); }
        }

        public bool DayNightEnabled
        {
            get { return dayNightEnabled; }
            set { Set(ref dayNightEnabled, value); }
        }

        // Интервал автосейва в игровых часах (1 тик = 1 час), 5..120.
        public int AutosaveHours
        {
            get { return autosaveHours; }
            set { Set(ref autosaveHours, ClampAutosaveHours(value)); }
        }

        public static float ClampVolume(float value)
        {
            if (float.IsNaN(value) || value < 0f) return 0f;
            return value > 1f ? 1f : value;
        }

        public static int ClampAgentLimit(int value)
        {
            if (value < MinAgentLimit) return MinAgentLimit;
            return value > MaxAgentLimit ? MaxAgentLimit : value;
        }

        public static int ClampAutosaveHours(int value)
        {
            if (value < MinAutosaveHours) return MinAutosaveHours;
            return value > MaxAutosaveHours ? MaxAutosaveHours : value;
        }

        // Кнопка «По умолчанию»: одно событие на весь сброс, если что-то изменилось.
        public void ResetToDefaults()
        {
            bool changed = volume != DefaultVolume || ambientVolume != DefaultAmbientVolume || muted
                || agentLimit != DefaultAgentLimit || !decorEnabled || !dayNightEnabled
                || autosaveHours != DefaultAutosaveHours;
            volume = DefaultVolume;
            ambientVolume = DefaultAmbientVolume;
            muted = false;
            agentLimit = DefaultAgentLimit;
            decorEnabled = true;
            dayNightEnabled = true;
            autosaveHours = DefaultAutosaveHours;
            if (changed && Changed != null) Changed();
        }

        private void Set(ref float field, float value)
        {
            if (field == value) return;
            field = value;
            if (Changed != null) Changed();
        }

        private void Set(ref int field, int value)
        {
            if (field == value) return;
            field = value;
            if (Changed != null) Changed();
        }

        private void Set(ref bool field, bool value)
        {
            if (field == value) return;
            field = value;
            if (Changed != null) Changed();
        }
    }
}