using System;
using UnityEngine;

namespace Citybuilder
{
    // Мост дефицита энергии/воды/мусора в темп роста домов + запись в ленту событий (EPIC-09 шаг 3).
    // Настройка: объект Grid, привязки — существующие Power/Water/WasteController.
    public class SupplyGrowthController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Энергия города. Пусто = штрафа нет.")] public PowerController Power;
        [Tooltip("Вода города. Пусто = штрафа нет.")] public WaterController Water;
        [Tooltip("Мусор города. Пусто = штрафа нет.")] public WasteController Waste;
        [Tooltip("Лента событий города (EPIC-09). Пусто = ищется на этом же объекте.")] public EventLogView EventLog;

        private bool hadDeficit;

        // Во сколько раз дольше растёт дом при худшем дефиците. Без ссылок — 1.
        public int GrowthTickMultiplier
        {
            get
            {
                return SupplyGrowthPolicy.Multiplier(
                    Power != null ? Power.Rate01 : 1f,
                    Water != null ? Water.Rate01 : 1f,
                    Waste != null ? Waste.Rate01 : 1f);
            }
        }

        // Проверка дефицита/восстановления снабжения для ленты событий на границе тика (EPIC-09 шаг 3).
        public void ReportTick(CityEventLog log, int tick)
        {
            if (log == null) return;
            float p = Power != null ? Power.Rate01 : 1f;
            float w = Water != null ? Water.Rate01 : 1f;
            float g = Waste != null ? Waste.Rate01 : 1f;
            string line = EvaluateSupplyEvent(p, w, g, ref hadDeficit);
            if (!string.IsNullOrEmpty(line)) log.Add(EventKind.Supply, line, tick);
        }

        // Чистая логика формирования события снабжения.
        public static string EvaluateSupplyEvent(float power01, float water01, float waste01, ref bool wasInDeficit)
        {
            int mult = SupplyGrowthPolicy.Multiplier(power01, water01, waste01);
            if (mult > 1)
            {
                wasInDeficit = true;
                if (power01 <= water01 && power01 <= waste01)
                {
                    return "Энергия " + (int)Math.Round(power01 * 100f) + "%: рост замедлен";
                }
                if (water01 <= waste01)
                {
                    return "Вода " + (int)Math.Round(water01 * 100f) + "%: рост замедлен";
                }
                return "Мусор " + (int)Math.Round(waste01 * 100f) + "%: рост замедлен";
            }
            if (wasInDeficit)
            {
                wasInDeficit = false;
                return "Снабжение восстановлено: 100%";
            }
            return string.Empty;
        }
    }
}
