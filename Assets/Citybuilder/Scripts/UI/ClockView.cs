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
            DrawSpeedRow(runner);
        }

        // Ряд кнопок скорости под лейблом. Клик — через приватный сеттер для E2E.
        private void DrawSpeedRow(SimulationRunner runner)
        {
            SimulationSpeed[] speeds = { SimulationSpeed.Paused, SimulationSpeed.Playing, SimulationSpeed.FastForward };
            string[] labels = { "Пауза", "Игра", "x4" };
            for (int i = 0; i < speeds.Length; i++)
            {
                Rect rect = new Rect(10 + i * (90 + 4), 84, 90, 26);
                if (DrawButton(rect, labels[i], runner.Speed == speeds[i]))
                {
                    SetRunnerSpeed(speeds[i]);
                }
            }
        }

        // Единая точка смены скорости для кнопок и E2E-моста.
        private void SetRunnerSpeed(SimulationSpeed speed)
        {
            SimulationRunner runner = Runner;
            if (runner == null) runner = GetComponent<SimulationRunner>();
            if (runner == null) return;
            runner.SetSpeed(speed);
        }

        // Одна кнопка: подсвечена жёлтым, цвет восстанавливается. Возвращает клик.
        private bool DrawButton(Rect rect, string label, bool highlighted)
        {
            Color saved = GUI.backgroundColor;
            if (highlighted)
            {
                GUI.backgroundColor = Color.yellow;
            }
            bool clicked = GUI.Button(rect, label);
            GUI.backgroundColor = saved;
            return clicked;
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
