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
            HudText.Draw(10, 50, Format(runner.Clock.CurrentTime, runner.Speed));
            DrawSpeedRow(runner);
        }

        // Ряд кнопок скорости под лейблом. Клик — через приватный сеттер для E2E.
        private void DrawSpeedRow(SimulationRunner runner)
        {
            float buttonWidth = 90f;
            float buttonHeight = 26f;
            float startX = 10f;
            float startY = 84f;
            float gap = 4f;
            UiClickGuard.Set("speeds", new Rect(startX, startY, 3 * buttonWidth + 2 * gap, buttonHeight));
            SimulationSpeed[] speeds = { SimulationSpeed.Paused, SimulationSpeed.Playing, SimulationSpeed.FastForward };
            string[] labels = { "Пауза", "Игра", "x4" };
            for (int i = 0; i < speeds.Length; i++)
            {
                Rect rect = new Rect(startX + i * (buttonWidth + gap), startY, buttonWidth, buttonHeight);
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
