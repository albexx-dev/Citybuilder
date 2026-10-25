namespace Citybuilder
{
    // Баланс воды (V1, этап 063). Тонкая обёртка над SupplyBalance.
    // Публичный API сохранён: логика и числа живут в SupplyBalance.
    // Ничем не используется: поведение игры не меняет.
    public class WaterSystem
    {
        private readonly SupplyBalance balance = new SupplyBalance();

        // Суммарный спрос потребителей.
        public float TotalDemand
        {
            get { return balance.TotalDemand; }
        }

        // Суммарная мощность источников воды.
        public float TotalSupply
        {
            get { return balance.TotalSupply; }
        }

        // Доля покрытия города: 0..1.
        public float SatisfactionRate01
        {
            get { return balance.Rate01; }
        }

        // Дубликат id перезаписывает значение.
        public void AddConsumer(EntityId id, float demand)
        {
            balance.SetDemand(id, demand);
        }

        // Дубликат id перезаписывает значение.
        public void AddSource(EntityId id, float capacity)
        {
            balance.SetSupply(id, capacity);
        }

        public void Remove(EntityId id)
        {
            balance.Remove(id);
        }

        public void Clear()
        {
            balance.Clear();
        }

        // Доля конкретного потребителя. Не найден — 0.
        public float SatisfactionOf(EntityId id)
        {
            return balance.RateOf(id);
        }

        public bool IsSatisfied(EntityId id)
        {
            return balance.IsSatisfied(id);
        }
    }
}
