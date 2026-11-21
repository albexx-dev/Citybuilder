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
        private int lastBlackoutCount;

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
            ReportTick(log, tick,
                Power != null ? Power.Rate01 : 1f,
                Water != null ? Water.Rate01 : 1f,
                Waste != null ? Waste.Rate01 : 1f);
        }

        // Шов для тестов: тот же путь записи, но с явными уровнями систем вместо ссылок сцены.
        public void ReportTick(CityEventLog log, int tick, float power01, float water01, float waste01)
        {
            ReportTick(log, tick, power01, water01, waste01, ref hadDeficit);
        }

        // Статический шов: не инстанцирует MonoBehaviour в EditMode-тестах.
        public static void ReportTick(CityEventLog log, int tick, float power01, float water01,
            float waste01, ref bool inDeficit)
        {
            if (log == null) return;
            CityEvent supplyEvent = EvaluateSupplyEvent(power01, water01, waste01, ref inDeficit, tick);
            if (!string.IsNullOrEmpty(supplyEvent.MessageKey)) log.Add(supplyEvent);
        }

        // Блэкаут (EPIC-50): в ленту только при изменении числа отключённых — дедуп по ключу
        // не спасает от меняющегося N (issue #63), поэтому гейт по изменению, без троттлинга.
        public void ReportBlackout(CityEventLog log, int tick)
        {
            if (log == null || Power == null) return;
            int count = Power.UnpoweredCount;
            if (count == lastBlackoutCount) return;
            lastBlackoutCount = count;
            log.Add(EventKind.Blackout,
                count > 0 ? EventMessageCatalog.Blackout : EventMessageCatalog.BlackoutRestored,
                count > 0 ? new object[] { count } : null, tick);
        }

        // Чистая логика формирования события снабжения: ключ + аргументы, без языка.
        public static CityEvent EvaluateSupplyEvent(float power01, float water01, float waste01,
            ref bool wasInDeficit, int tick)
        {
            int mult = SupplyGrowthPolicy.Multiplier(power01, water01, waste01);
            if (mult > 1)
            {
                wasInDeficit = true;
                if (power01 <= water01 && power01 <= waste01)
                    return Deficit(EventMessageCatalog.SupplyPowerDeficit, power01, tick);
                if (water01 <= waste01)
                    return Deficit(EventMessageCatalog.SupplyWaterDeficit, water01, tick);
                return Deficit(EventMessageCatalog.SupplyWasteDeficit, waste01, tick);
            }
            if (wasInDeficit)
            {
                wasInDeficit = false;
                return new CityEvent(EventKind.Supply, EventMessageCatalog.SupplyRestored, null, tick);
            }
            return new CityEvent(EventKind.Supply, string.Empty, null, tick);
        }

        private static CityEvent Deficit(string messageKey, float rate01, int tick)
        {
            return new CityEvent(EventKind.Supply, messageKey,
                new object[] { (int)Math.Round(rate01 * 100f) }, tick);
        }
    }
}
