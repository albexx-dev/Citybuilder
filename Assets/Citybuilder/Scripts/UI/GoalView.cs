using UnityEngine;

namespace Citybuilder
{
    // HUD цели. V1: штатный OnGUI, только рисует.
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
            HudText.Draw(10, 148, BuildText());
        }

        // Имя метода не HudText: оно скрывало бы одноимённый статический хелпер.
        private string BuildText()
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
    }
}
