using UnityEngine;

namespace Citybuilder
{
    // Показывает удовлетворённость потребностей. V1: штатный OnGUI-лейбл под строкой населения.
    // Нет контроллеров — молча не рисует. Есть, но системы не готовы — рисует нули.
    public class NeedsView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер населения. Опционален: иначе ищется на этом же объекте.")]
        public PopulationController Population;
        [Tooltip("Рост поселений. Опционален: иначе ищется на этом же объекте.")]
        public SettlementGrowthController Growth;

        private NeedSystem needs;

        private void OnGUI()
        {
            PopulationController population = Population;
            if (population == null) population = GetComponent<PopulationController>();
            SettlementGrowthController growth = Growth;
            if (growth == null) growth = GetComponent<SettlementGrowthController>();
            if (population == null || growth == null) return;
            EnsureNeeds(population, growth);
            float housing = 0f;
            float jobs = 0f;
            float employment = 0f;
            float average = 0f;
            if (needs != null)
            {
                housing = needs.HousingRoomRate;
                jobs = needs.JobsRoomRate;
                employment = needs.EmploymentRate;
                average = needs.AverageSatisfaction;
            }
            string text = "Потребности: " + ToPercent(average) + "%";
            text += " (жильё " + ToPercent(housing) + "%";
            text += " • работа " + ToPercent(jobs) + "%";
            text += " • занятость " + ToPercent(employment) + "%)";
            GUI.Label(new Rect(10, 50, 400, 30), text);
        }

        // Тихое создание поверх живых симуляций. Систем нет — тихо рисуем нули.
        private void EnsureNeeds(PopulationController population, SettlementGrowthController growth)
        {
            if (needs != null) return;
            if (population == null || growth == null) return;
            if (population.Population == null) return;
            if (growth.Settlement == null) return;
            needs = new NeedSystem(population.Population, growth.Settlement);
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
