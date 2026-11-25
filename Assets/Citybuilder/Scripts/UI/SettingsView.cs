using UnityEngine;

namespace Citybuilder
{
    // Окно настроек (EPIC-69 шаг 2): открывается из меню паузы (план эпика «Пауза → Настройки»)
    // и кнопкой «Настройки» в нижней панели (образец «Задачи» из QuestLogView).
    // Панель меняет GameSettings: каждое реальное изменение persist через GameSettingsStore
    // и применяется к системам с готовыми точками: AudioController (громкость/мут),
    // DayNightController (сутки вкл/выкл, Reapply показывает смену сразу), а в шаге 3 эпика —
    // DecorManager/BuildModeMenu (декор) и AutosaveController (интервал) через SettingsAppliers.
    // Лимит машин VehicleAgentHost читает из Settings.AgentLimit напрямую на каждом Update.
    public class SettingsView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Меню паузы для возврата. Пусто = ищется в сцене.")] public PauseMenuView Pause;
        [Tooltip("Драйвер игровых часов: окно ставит игру на паузу. Пусто = ищется в сцене.")] public SimulationRunner Runner;
        [Tooltip("Контроллер звука: громкость и мут применяются на лету. Пусто = ищется в сцене.")] public AudioController Audio;
        [Tooltip("Цикл дня и ночи: вкл/выкл применяется сразу. Пусто = ищется в сцене.")] public DayNightController DayNight;
        [Tooltip("Декор сцены: вкл/выкл применяется сразу. Пусто = ищется в сцене.")] public DecorManager Decor;
        [Tooltip("Меню режимов: гасит кнопку и режим «Декор». Пусто = ищется в сцене.")] public BuildModeMenu Menu;
        [Tooltip("Автосейв: интервал в игровых часах. Пусто = ищется в сцене.")] public AutosaveController Autosave;
        [Tooltip("Эмбиент: громкость применяется сразу. Пусто = ищется в сцене.")] public AmbientAudioView Ambient;
        [Tooltip("Туториал: кнопка «Повторить обучение». Пусто = ищется в сцене.")] public TutorialView Tutorial;

        private readonly GameSettings settings = new GameSettings();
        private bool isOpen;
        private bool lastDayNightEnabled = true;
        private bool subscribed;
        private SimulationSpeed speedBeforeOpen = SimulationSpeed.Playing;

        public bool IsOpen { get { return isOpen; } }

        // Шов для слушателей шага 3 эпика: тот же профиль, что редактирует окно.
        public GameSettings Settings { get { return settings; } }

        private void OnEnable() { Init(); }

        private void OnDisable()
        {
            if (subscribed && settings != null) settings.Changed -= OnSettingsChanged;
            subscribed = false;
        }

        // Публичная инициализация: в EditMode-тестах AddComponent не вызывает OnEnable
        // (внутри теста нет тика редактора), поэтому тесты зовут Init() вручную.
        public void Init()
        {
            ResolveRefs();
            GameSettingsStore.Load(settings);
            if (!subscribed) settings.Changed += OnSettingsChanged;
            subscribed = true;
            ApplyToListeners(); // профиль из прошлой сессии применяем до первого открытия
        }

        private void Update()
        {
            if (isOpen && Input.GetKeyDown(KeyCode.Escape) && UiEscapeGuard.TryConsume(Time.frameCount)) ReturnToPause();
        }

        public void Open()
        {
            ResolveRefs();
            GameSettingsStore.Load(settings); // подтянуть правки из паузы (слайдер громкости)
            if (!isOpen) AudioController.PlaySwooshGlobal();
            if (!isOpen) speedBeforeOpen = Runner != null ? Runner.Speed : SimulationSpeed.Playing;
            CloseSiblingPanels();
            isOpen = true;
            if (Runner != null) Runner.SetSpeed(SimulationSpeed.Paused);
            ApplyToListeners();
        }

        public void Close()
        {
            if (isOpen) AudioController.PlaySwooshGlobal();
            isOpen = false;
            UiClickGuard.Set("settings", new Rect());
            if (Runner != null) Runner.SetSpeed(speedBeforeOpen);
        }

        // Шов для хоткеев других панелей: закрыть настройки, если открыты (Tab/L поверх окна).
        public static void CloseIfOpen()
        {
            SettingsView settings = FindFirstObjectByType<SettingsView>();
            if (settings != null && settings.IsOpen) settings.Close();
        }

        // Нижняя панель тогглит окно, как «Задачи» и «Лента».
        public void Toggle()
        {
            if (isOpen) Close();
            else Open();
        }

        public void ReturnToPause()
        {
            Close();
            if (Pause != null) Pause.Open();
        }

        // Панели справа взаимоисключающие с настройками (прецедент InfoMenuView.CloseSiblingPanels).
        private void CloseSiblingPanels()
        {
            InfoMenuView info = FindFirstObjectByType<InfoMenuView>();
            EventLogView log = FindFirstObjectByType<EventLogView>();
            QuestLogView quests = FindFirstObjectByType<QuestLogView>();
            if (info != null) info.SetOpen(false);
            if (log != null) log.SetOpen(false);
            if (quests != null) quests.SetOpen(false);
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            ResolveRefs();
            Rect panel = SettingsLayout.PanelRect(Screen.width, Screen.height);
            UiClickGuard.Set("settings", panel);
            BottomBarStyle.DrawBarBackground(panel);
            DrawContent(panel);
        }

        private void DrawContent(Rect panel)
        {
            float x = panel.x + 16f;
            float y = panel.y + 12f;
            HudText.Draw(x, y, Localization.T("settings_title"));
            y += 30f;
            SettingsPanels.DrawSound(settings, x, ref y);
            SettingsPanels.DrawGameplay(settings, x, ref y);
            SettingsPanels.DrawFooter(settings, this, Tutorial, x, y);
        }

        // Каждое реальное изменение: профиль в PlayerPrefs и на лету подключённым системам.
        private void OnSettingsChanged()
        {
            GameSettingsStore.Save(settings);
            ApplyToListeners();
        }

        private void ApplyToListeners()
        {
            ApplyAudio(settings, Audio);
            if (settings.DayNightEnabled != lastDayNightEnabled) { ApplyDayNight(settings, DayNight); lastDayNightEnabled = settings.DayNightEnabled; }
            SettingsAppliers.ApplyDecor(settings, Decor, Menu);
            SettingsAppliers.ApplyAutosave(settings, Autosave);
            SettingsAppliers.ApplyAmbient(settings, Ambient);
            SettingsAppliers.ApplyOverlays(settings, FindFirstObjectByType<GridVisualizer>(), FindFirstObjectByType<RoadView>());
            SettingsAppliers.ApplyGarbageTrucks(settings, FindFirstObjectByType<GarbageTruckView>());
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
