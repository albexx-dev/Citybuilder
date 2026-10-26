using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Общий снимок снабжения: городская база + выросшие и ручные здания.
    // Чистый C#, без Unity: контроллеры только передают доступ к моделям.
    public class SupplyFeed
    {
        private readonly SupplyBalance balance;
        private readonly Func<BuildingDefinition, int> demandOf;
        private readonly Func<BuildingDefinition, int> outputOf;
        private EntityId cityGridId;
        private int baseSupply;

        public SupplyFeed(SupplyBalance balance, int baseSupply,
            Func<BuildingDefinition, int> demand, Func<BuildingDefinition, int> output)
        {
            this.balance = balance ?? new SupplyBalance();
            this.baseSupply = baseSupply > 0 ? baseSupply : 0;
            demandOf = demand;
            outputOf = output;
        }

        // Мощность города до появления станций. Отрицательная — 0.
        public int BaseSupply
        {
            get { return baseSupply; }
            set { baseSupply = value > 0 ? value : 0; }
        }

        public float TotalDemand { get { return balance.TotalDemand; } }
        public float TotalSupply { get { return balance.TotalSupply; } }
        public float Rate01 { get { return balance.Rate01; } }

        // Проверка по id из снимка: дом без спроса или вне снимка — false.
        public bool IsSatisfied(EntityId id) { return balance.IsSatisfied(id); }

        public void Clear() { balance.Clear(); }

        // Пересчёт с нуля: снос здания исчезает сам.
        public void Refresh(List<BuildingState> grown, List<BuildingState> manual)
        {
            balance.Clear();
            AddBase();
            AddStates(grown);
            AddStates(manual);
        }

        // Один проход по списку: null-список и null-элементы пропускаются.
        private void AddStates(List<BuildingState> states)
        {
            if (states == null) return;
            for (int i = 0; i < states.Count; i++) AddState(states[i]);
        }

        // Одно здание в баланс: спрос и генерация только больше нуля.
        private void AddState(BuildingState state)
        {
            if (state == null || state.Definition == null) return;
            BuildingDefinition def = state.Definition;
            int demand = demandOf != null ? demandOf(def) : 0;
            int output = outputOf != null ? outputOf(def) : 0;
            if (demand > 0) balance.SetDemand(state.Id, demand);
            if (output > 0) balance.SetSupply(state.Id, output);
        }

        // Городская сеть — ленивый id, создаётся один раз на сессию.
        private void AddBase()
        {
            if (!cityGridId.IsValid) cityGridId = EntityId.New();
            balance.SetSupply(cityGridId, baseSupply);
        }
    }
}
