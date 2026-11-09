using UnityEngine;

namespace Citybuilder
{
    // Автосейв каждые 24 игровых часа в autosave.json (EPIC-11 шаг 2, ADR-008).
    // Подписка на TickCompleted: в паузе не тикает; пропускает тик, если на нём уже был ручной сейв.
    public class AutosaveController : MonoBehaviour
    {
        public const int AutosaveIntervalTicks = 24;

        [Header("References")]
        [Tooltip("Контроллер сохранения. Пусто = ищется на этом же объекте.")]
        public SaveController Save;
        [Tooltip("Драйвер часов. Пусто = ищется в сцене.")]
        public SimulationRunner Runner;

        private int ticksSinceSave;

        public int TicksSinceSave
        {
            get { return ticksSinceSave; }
        }

        private void OnEnable()
        {
            ResolveRefs();
            if (Runner != null && Runner.Clock != null) Runner.Clock.TickCompleted += OnClockTick;
        }

        private void OnDisable()
        {
            if (Runner != null && Runner.Clock != null) Runner.Clock.TickCompleted -= OnClockTick;
        }

        public void OnClockTick(SimulationTime time)
        {
            ResolveRefs();
            long manualTick = Save != null ? Save.LastManualSaveTick : -1;
            if (!StepCounter(ref ticksSinceSave, AutosaveIntervalTicks, time.TickCount, manualTick)) return;
            if (Save != null) Save.SaveNow(SaveSlotIndex.AutosaveSlot, SaveSlotIndex.DefaultName(SaveSlotIndex.AutosaveSlot));
        }

        // Чистая логика счёта тиков автосейва для тестирования без сцены.
        public static bool StepCounter(ref int counter, int interval, long currentTick, long lastManualSaveTick)
        {
            int safeInterval = interval < 1 ? 1 : interval;
            counter++;
            if (counter < safeInterval) return false;
            counter = 0;
            return currentTick != lastManualSaveTick;
        }

        private void ResolveRefs()
        {
            if (Save == null) Save = GetComponent<SaveController>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
        }
    }
}
