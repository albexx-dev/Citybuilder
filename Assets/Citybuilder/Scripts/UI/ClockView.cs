using UnityEngine;

namespace Citybuilder
{
    // Показывает игровое время и скорость. V1: штатный OnGUI-лейбл без UI-пакетов.
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
            if (runner == null) return;
            if (runner.Clock == null) return;
            GUI.Label(new Rect(10, 50, 300, 30), Format(runner.Clock.CurrentTime, runner.Speed));
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
