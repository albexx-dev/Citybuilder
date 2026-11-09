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
        [Tooltip("Вода города. Опционален: иначе ищется на этом же объекте.")]
        public WaterController Water;
        [Tooltip("Мусор города. Опционален: иначе ищется на этом же объекте.")]
        public WasteController Waste;
        [Tooltip("Пожарные города. Опционален: иначе ищется на этом же объекте.")]
        public ServiceCoverageController Fire;

        private NeedSystem needs;

        // Строка потребностей для информационного меню (EPIC-14) и проверок.
        public string CurrentText
        {
            get
            {
                PopulationController population = Population != null ? Population : GetComponent<PopulationController>();
                SettlementGrowthController growth = Growth != null ? Growth : GetComponent<SettlementGrowthController>();
                if (population == null || growth == null) return string.Empty;
                EnsureNeeds(population, growth);
                return BuildText(needs);
            }
        }

        // Тихое создание поверх живых симуляций. Систем нет — тихо рисуем нули.
        private void EnsureNeeds(PopulationController population, SettlementGrowthController growth)
        {
            if (needs != null) return;
            if (population == null || growth == null) return;
            if (population.Population == null) return;
            if (growth.Settlement == null) return;
            if (Power == null) Power = GetComponent<PowerController>();
            if (Water == null) Water = GetComponent<WaterController>();
            if (Waste == null) Waste = GetComponent<WasteController>();
            if (Fire == null) Fire = GetComponent<ServiceCoverageController>();
            Func<float> power = Power != null ? (Func<float>)(() => Power.Rate01) : null;
            Func<float> water = Water != null ? (Func<float>)(() => Water.Rate01) : null;
            Func<float> waste = Waste != null ? (Func<float>)(() => Waste.Rate01) : null;
            Func<float> fire = Fire != null ? (Func<float>)(() => Fire.Rate01) : null;
            needs = new NeedSystem(population.Population, growth.Settlement, power, water, waste, fire);
        }

        // Строка потребностей. Провайдеры добавляются только при наличии.
        private static string BuildText(NeedSystem model)
        {
            float housing = model != null ? model.HousingRoomRate : 0f;
            float jobs = model != null ? model.JobsRoomRate : 0f;
            float employment = model != null ? model.EmploymentRate : 0f;
            float average = model != null ? model.AverageSatisfaction : 0f;
            string text = "Потребности: " + ToPercent(average) + "%";
            text += " (жильё " + ToPercent(housing) + "%";
            text += " • работа " + ToPercent(jobs) + "%";
            text += " • занятость " + ToPercent(employment) + "%";
            text = AppendProviders(text, model);
            return text + ")";
        }

        // Подключённые показатели после базовых трёх: энергия, вода, мусор, пожарные.
        private static string AppendProviders(string text, NeedSystem model)
        {
            if (model == null) return text;
            if (model.HasPower) text += " • энергия " + ToPercent(model.PowerRate) + "%";
            if (model.HasWater) text += " • вода " + ToPercent(model.WaterRate) + "%";
            if (model.HasWaste) text += " • мусор " + ToPercent(model.WasteRate) + "%";
            if (model.HasFire) text += " • пожарные " + ToPercent(model.FireRate) + "%";
            return text;
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
