using System;
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
        [Tooltip("Энергия города. Опционален: иначе ищется на этом же объекте.")]
        public PowerController Power;

        private NeedSystem needs;

        private void OnGUI()
        {
            PopulationController population = Population;
            if (population == null) population = GetComponent<PopulationController>();
            SettlementGrowthController growth = Growth;
            if (growth == null) growth = GetComponent<SettlementGrowthController>();
            if (population == null || growth == null) return;
            EnsureNeeds(population, growth);
            string text = BuildText(needs);
            // y=114: ниже часов (y=50) и кнопок скорости (y=84-110), чтобы не налезать.
            GUI.Label(new Rect(10, 114, 400, 30), text);
        }

        // Тихое создание поверх живых симуляций. Систем нет — тихо рисуем нули.
        private void EnsureNeeds(PopulationController population, SettlementGrowthController growth)
        {
            if (needs != null) return;
            if (population == null || growth == null) return;
            if (population.Population == null) return;
            if (growth.Settlement == null) return;
            if (Power == null) Power = GetComponent<PowerController>();
            Func<float> rate = Power != null ? (Func<float>)(() => Power.Rate01) : null;
            needs = new NeedSystem(population.Population, growth.Settlement, rate);
        }

        // Строка потребностей. Энергия добавляется только при наличии провайдера.
        private static string BuildText(NeedSystem model)
        {
            float housing = 0f;
            float jobs = 0f;
            float employment = 0f;
            float average = 0f;
            bool hasPower = false;
            float power = 1f;
            if (model != null)
            {
                housing = model.HousingRoomRate;
                jobs = model.JobsRoomRate;
                employment = model.EmploymentRate;
                average = model.AverageSatisfaction;
                hasPower = model.HasPower;
                power = model.PowerRate;
            }
            string text = "Потребности: " + ToPercent(average) + "%";
            text += " (жильё " + ToPercent(housing) + "%";
            text += " • работа " + ToPercent(jobs) + "%";
            text += " • занятость " + ToPercent(employment) + "%";
            if (hasPower) text += " • энергия " + ToPercent(power) + "%";
            return text + ")";
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
