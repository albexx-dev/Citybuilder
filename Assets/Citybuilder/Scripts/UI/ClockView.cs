using UnityEngine;

namespace Citybuilder
{
    // Показывает игровое время и скорость. Кнопки скоростей переехали в BottomBarView (EPIC-13 шаг 2).
    // Нет раннера — молча не рисует. Часов нет — нули не рисует.
    public class ClockView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Драйвер игровых часов. Опционален: иначе ищется на этом же объекте.")]
        public SimulationRunner Runner;

        private void OnGUI()
        {
            SimulationRunner runner = Runner;
            if (runner == null) runner = GetComponent<SimulationRunner>();
            if (runner == null || runner.Clock == null) return;
            HudText.Draw(10, 50, Format(runner.Clock.CurrentTime, runner.Speed));
        }

        // Единая точка смены скорости для E2E-моста.
        private void SetRunnerSpeed(SimulationSpeed speed)
        {
            SimulationRunner runner = Runner;
            if (runner == null) runner = GetComponent<SimulationRunner>();
            if (runner == null) return;
            runner.SetSpeed(speed);
        }

        private static string Format(SimulationTime time, SimulationSpeed speed)
        {
            string speedName = "x4";
            if (speed == SimulationSpeed.Paused) speedName = "Пауза";
            else if (speed == SimulationSpeed.Playing) speedName = "Игра";
            return "Day " + time.DayNumber + ", Hour " + time.HourOfDay + " — " + speedName;
        }
    }
}
