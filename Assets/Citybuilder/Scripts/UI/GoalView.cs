using UnityEngine;

namespace Citybuilder
{
    // HUD цели и экраны победы/поражения. V1: штатный OnGUI, только рисует.
    // Нет контроллеров — молча не рисует. Системы не готовы — только HUD-строка.
    public class GoalView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер населения. Опционален: иначе ищется на этом же объекте.")]
        public PopulationController Population;
        [Tooltip("Драйвер игровых часов. Опционален: иначе ищется на этом же объекте.")]
        public SimulationRunner Simulation;

        private GoalSystem goals;

        private void OnGUI()
        {
            PopulationController population = Population;
            if (population == null) population = GetComponent<PopulationController>();
            SimulationRunner simulation = Simulation;
            if (simulation == null) simulation = GetComponent<SimulationRunner>();
            if (population == null || simulation == null) return;
            EnsureGoals(population, simulation);
            if (goals == null) return;
            // y=148: ниже потребностей (y=114), чтобы не налезать на левую колонку.
            GUI.Label(new Rect(10, 148, 400, 30), HudText());
            if (goals.Won) DrawCenter("Победа! Город вырос.");
            else if (goals.Lost) DrawCenter("Поражение. Город пуст.");
        }

        private string HudText()
        {
            return "Цель: " + goals.Residents + "/" + GoalSystem.GoalResidents + " жителей";
        }

        // Тихое создание поверх живых симуляций. Систем нет — рисуем только HUD.
        private void EnsureGoals(PopulationController population, SimulationRunner simulation)
        {
            if (goals != null) return;
            if (population.Population == null) return;
            if (simulation.Clock == null) return;
            goals = new GoalSystem(population.Population, simulation.Clock);
        }

        // Центральный бокс с сообщением. Кнопок нет, игру не блокирует.
        private static void DrawCenter(string message)
        {
            float width = 320f;
            float height = 60f;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;
            GUI.Box(new Rect(x, y, width, height), "");
            GUI.Label(new Rect(x, y + 18f, width, 24f), message);
        }
    }
}
