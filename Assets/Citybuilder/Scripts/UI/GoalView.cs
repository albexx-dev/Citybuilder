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
        [Tooltip("Казна для наград за ступени. Опционален: иначе ищется на этом же объекте.")]
        public TreasuryView Treasury;

        private GoalSystem goals;
        // Ступень цели, которая пишется в слот v4 и восстанавливается при загрузке.
        private readonly GoalStages stages = new GoalStages();
        private readonly GoalRewardController rewards = new GoalRewardController();
        private TreasuryView treasuryCache;
        private bool treasurySearched;

        // Ступень для слота: 0..3.
        public int Stage
        {
            get { return stages.Stage; }
        }

        // Текущая строка цели для информационного меню (EPIC-14).
        public string CurrentText
        {
            get
            {
                PopulationController population = Population != null ? Population : GetComponent<PopulationController>();
                SimulationRunner simulation = Simulation != null ? Simulation : GetComponent<SimulationRunner>();
                if (population == null || simulation == null) return string.Empty;
                EnsureGoals(population, simulation);
                return goals != null ? BuildText() : string.Empty;
            }
        }

        public void RestoreStage(int stage)
        {
            stages.Restore(stage);
        }

        // Награды начисляются в Update: OnGUI больше не меняет состояние города.
        private void Update()
        {
            PopulationController population = Population;
            if (population == null) population = GetComponent<PopulationController>();
            SimulationRunner simulation = Simulation;
            if (simulation == null) simulation = GetComponent<SimulationRunner>();
            if (population == null || simulation == null) return;
            EnsureGoals(population, simulation);
            TreasuryView treasury = ResolveTreasury();
            if (goals == null || treasury == null) return;
            // Ступень хранится в слоте v4: после загрузки награда повторно не начисляется.
            rewards.Claim(goals, stages, treasury.Treasury);
        }

        private void OnGUI()
        {
            string text = CurrentText;
            if (string.IsNullOrEmpty(text)) return;
            HudText.Draw(10, 148, text);
        }

        // Ленивый кеш: казна может жить на отдельном объекте, а поле в сцене пустое.
        // Повторный поиск по сцене каждый OnGUI-эвент не нужен.
        private TreasuryView ResolveTreasury()
        {
            if (Treasury != null) return Treasury;
            if (treasuryCache != null) return treasuryCache;
            if (treasurySearched) return null;
            treasuryCache = FindFirstObjectByType<TreasuryView>();
            treasurySearched = true;
            return treasuryCache;
        }

        // Имя метода не HudText: оно скрывало бы одноимённый статический хелпер.
        private string BuildText()
        {
            if (stages.AllDone)
            {
                return "Цель выполнена: " + goals.Residents + " жителей";
            }
            int step = stages.Stage + 1;
            return "Цель: " + goals.Residents + "/" + stages.Target + " жителей (ступень "
                + step + "/" + GoalStages.StageCount + ")";
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
