namespace Citybuilder
{
    // Сборщик событий v1.2 для TutorialView (эпик 97): опрашивает доступные системы,
    // запоминает первые события и гасит показанную подсказку по таймеру. Обычный C#
    // без MonoBehaviour: создаётся и обновляется TutorialView.
    public class TutorialEventView
    {
        private const float ShowSeconds = 8f;

        private TutorialEventPolicy.State state;
        private TutorialEventPolicy.Kind shown;
        private float shownAt;
        private int questBaseline;
        private bool wasNight;

        public TutorialEventPolicy.Kind Current { get { return shown; } }

        // Сброс: события забыты, отсчёт квестов и ночи берётся с текущего момента.
        public void Reset(QuestLogView quests, SimulationRunner runner)
        {
            state = default(TutorialEventPolicy.State);
            shown = TutorialEventPolicy.Kind.None;
            shownAt = 0f;
            questBaseline = QuestCount(quests);
            wasNight = IsNight(runner);
        }

        public void Sample(EconomyTicker economy, QuestLogView quests, EventLogView log,
            SimulationRunner runner, bool hasFireStation, float time)
        {
            if (economy != null && economy.Fires.BurningCount > 0) state.FireHappened = true;
            state.HasFireStation = hasFireStation;
            if (QuestCount(quests) > questBaseline) state.QuestRewarded = true;
            if (HasGarbage(log)) state.WasteCollected = true;
            bool night = IsNight(runner);
            if (night && !wasNight) state.NightArrived = true;
            wasNight = night;
            TutorialEventPolicy.Kind next = TutorialEventPolicy.Pick(state);
            if (next != shown) { shown = next; shownAt = time; }
            if (shown != TutorialEventPolicy.Kind.None && time - shownAt >= ShowSeconds) Acknowledge();
        }

        public void Acknowledge()
        {
            if (shown == TutorialEventPolicy.Kind.None) return;
            state = TutorialEventPolicy.Acknowledge(state, shown);
            shown = TutorialEventPolicy.Kind.None;
        }

        private static int QuestCount(QuestLogView quests)
        {
            return quests != null && quests.Quests != null ? quests.Quests.CompletedCount : 0;
        }

        private static bool IsNight(SimulationRunner runner)
        {
            if (runner == null || runner.Clock == null) return false;
            int hour = runner.Clock.CurrentTime.HourOfDay;
            return hour >= 22 || hour < 6;
        }

        private static bool HasGarbage(EventLogView log)
        {
            return log != null && log.Log != null && log.Log.Contains(EventKind.Garbage);
        }
    }
}
