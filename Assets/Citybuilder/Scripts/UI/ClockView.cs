using System.Globalization;
using UnityEngine;

namespace Citybuilder
{
    // Показывает игровое время и скорость. Кнопки скоростей переехали в BottomBarView (EPIC-13 шаг 2).
    // Нет раннера — молча не рисует. Часов нет — нули не рисует.
    // issue #161: тонкий адаптер замера тика — домен знает только Stopwatch, здесь подписка и HUD.
    public class ClockView : MonoBehaviour
    {
        private const int ProfilerRow = 5; // ряд 4 занят кнопкой RU/EN

        [Header("References")]
        [Tooltip("Драйвер игровых часов. Опционален: иначе ищется на этом же объекте.")]
        public SimulationRunner Runner;

        private readonly TickDurationMeter profiler = new TickDurationMeter();

        // Замер тика для HUD и E2E-моста: доступен без правок сцены (issue #161).
        public TickDurationMeter Profiler { get { return profiler; } }

        private SimulationRunner subscribedRunner;

        private void OnEnable()
        {
            subscribedRunner = ResolveRunner();
            if (subscribedRunner == null || subscribedRunner.Clock == null) return;
            subscribedRunner.Clock.TickStarted += OnTickStarted;
            subscribedRunner.Clock.TickCompleted += OnTickCompleted;
        }

        private void OnDisable()
        {
            if (subscribedRunner == null || subscribedRunner.Clock == null) return;
            subscribedRunner.Clock.TickStarted -= OnTickStarted;
            subscribedRunner.Clock.TickCompleted -= OnTickCompleted;
            subscribedRunner = null;
        }

        private void OnTickStarted(SimulationTime time) { profiler.Begin(); }
        private void OnTickCompleted(SimulationTime time) { profiler.End(); }

        private void OnGUI()
        {
            SimulationRunner runner = ResolveRunner();
            if (runner == null || runner.Clock == null) return;
            HudText.Draw(HudLayout.TopLeftX, HudLayout.TopLeftRowY(3),
                Format(runner.Clock.CurrentTime, runner.Speed),
                HudText.TimeColor(runner.Clock.CurrentTime.HourOfDay));
            if (profiler.Count == 0) return;
            string p95 = profiler.P95Milliseconds().ToString("F2", CultureInfo.InvariantCulture);
            HudText.Draw(HudLayout.TopLeftX, HudLayout.TopLeftRowY(ProfilerRow),
                Localization.T("tick_p95_line", p95, profiler.Count));
        }

        private SimulationRunner ResolveRunner()
        {
            if (Runner == null) Runner = GetComponent<SimulationRunner>();
            return Runner;
        }

        // Единая точка смены скорости для E2E-моста.
        private void SetRunnerSpeed(SimulationSpeed speed)
        {
            SimulationRunner runner = ResolveRunner();
            if (runner == null) return;
            runner.SetSpeed(speed);
        }

        private static string Format(SimulationTime time, SimulationSpeed speed)
        {
            string speedName = "x4";
            if (speed == SimulationSpeed.Paused) speedName = Localization.T("clock_paused");
            else if (speed == SimulationSpeed.Playing) speedName = Localization.T("clock_playing");
            return Localization.T("clock_line", time.DayNumber, time.HourOfDay, speedName);
        }
    }
}
