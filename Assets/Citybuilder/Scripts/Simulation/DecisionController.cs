using System;
using System.Collections.Generic;
namespace Citybuilder
{
    // Решения игрока (ADR-020, issue #167): ожидание и применение выбора на границе тика.
    // Plain C#: без Unity-событий и шины, лента пишется прямыми Add; не сериализуется.
    public class DecisionController
    {
        public const int SuppressionCost = 25;
        private readonly CityEventLog log;
        private readonly Treasury treasury;
        private readonly Action<int> extinguish;
        private readonly Dictionary<int, int> pending = new Dictionary<int, int>();
        private readonly HashSet<int> resolved = new HashSet<int>();

        public DecisionController(CityEventLog log, Treasury treasury, Action<int> extinguish)
        {
            this.log = log;
            this.treasury = treasury;
            this.extinguish = extinguish;
        }

        public int PendingCount { get { return pending.Count; } }
        public bool HasPending { get { return pending.Count > 0; } }
        // Первая запись: решение появилось. Один дом — одно ожидание за сессию.
        public bool Expected(int homeId, int tick)
        {
            if (log == null || pending.ContainsKey(homeId) || resolved.Contains(homeId)) return false;
            pending.Add(homeId, tick);
            return log.Add(new CityEvent(EventKind.Fire, EventSubtype.DecisionOffered,
                EventMessageCatalog.DecisionOffered, new object[] { homeId, SuppressionCost }, tick));
        }
        // Вторая запись: выбранный вариант. Другие ключ, аргументы и тик — дедуп не склеит.
        public bool Resolve(int homeId, bool suppress, int tick)
        {
            int offered;
            if (!pending.TryGetValue(homeId, out offered)) return false;
            if (suppress && treasury != null && !treasury.CanAfford(SuppressionCost)) return false;
            pending.Remove(homeId);
            resolved.Add(homeId);
            if (suppress)
            {
                if (treasury != null) treasury.TrySpend(SuppressionCost, "fire suppression");
                if (extinguish != null) extinguish(homeId);
            }
            EventSubtype subtype = suppress ? EventSubtype.DecisionSuppressed : EventSubtype.DecisionIgnored;
            string key = suppress ? EventMessageCatalog.DecisionSuppressed : EventMessageCatalog.DecisionIgnored;
            object[] args = suppress ? new object[] { homeId, SuppressionCost, 1 } : new object[] { homeId, 0 };
            return log.Add(new CityEvent(EventKind.Fire, subtype, key, args, Math.Max(tick, offered + 1)));
        }
        // Первый ожидающий дом для UI: -1, когда решений нет.
        public int FirstPendingHome()
        {
            foreach (int homeId in pending.Keys) return homeId;
            return -1;
        }
        // «Новая игра»/загрузка: отложенных решений после загрузки нет (ADR-020).
        public void Reset()
        {
            pending.Clear();
            resolved.Clear();
        }
    }
}
