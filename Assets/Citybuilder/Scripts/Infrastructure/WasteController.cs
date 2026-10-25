using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Мусор города: спрос от выросших зданий, предложение — их утилизация плюс городская база.
    // Считаем прямо на SupplyBalance. Настройка: объект Grid, Growth обязателен.
    public class WasteController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Рост поселений. Обязателен: даёт Homes для спроса и утилизации.")]
        public SettlementGrowthController Growth;

        [Header("City Grid")]
        [Tooltip("Городская база утилизации до появления перерабатывающих зданий.")]
        public int BaseCapacity = 12;

        [Header("Refresh")]
        [Tooltip("Пауза между пересчётами в секундах. Минимум 0.5.")]
        public float RefreshIntervalSec = 2f;

        private SupplyBalance balance;
        private EntityId cityGridId;
        private float timer;

        // Суммарный спрос зданий из последнего снимка. Снимка нет — 0.
        public int Demand
        {
            get { return Mathf.RoundToInt(EnsureBalance().TotalDemand); }
        }

        // Суммарная утилизация: городская база и ёмкости зданий. Снимка нет — 0.
        public int Supply
        {
            get { return Mathf.RoundToInt(EnsureBalance().TotalSupply); }
        }

        // Доля покрытия 0..1 из последнего снимка. Снимка нет — 1.
        public float Rate01
        {
            get { return EnsureBalance().Rate01; }
        }

        private void Start()
        {
            ClampSettings();
            if (!EnsureReady()) return;
            RefreshNow();
        }

        private void OnValidate()
        {
            ClampSettings();
        }

        private void Update()
        {
            if (!EnsureReady()) return;
            timer += Time.deltaTime;
            if (timer < RefreshIntervalSec) return;
            timer = 0f;
            RefreshNow();
        }

        // Пересчитать снимок сейчас. Clear каждый раз: снос домов исчезает сам.
        public void RefreshNow()
        {
            SupplyBalance current = EnsureBalance();
            current.Clear();
            if (Growth == null) return;
            RegisterCityGrid(current);
            List<BuildingState> homes = Growth.Homes;
            if (homes == null) return;
            for (int i = 0; i < homes.Count; i++) AddHome(current, homes[i]);
        }

        // Одно здание в баланс: спрос и утилизация только при значениях больше нуля.
        private void AddHome(SupplyBalance current, BuildingState home)
        {
            if (home == null || home.Definition == null) return;
            BuildingDefinition def = home.Definition;
            if (def.WasteOutput > 0) current.SetDemand(home.Id, def.WasteOutput);
            if (def.WasteCapacity > 0) current.SetSupply(home.Id, def.WasteCapacity);
        }

        // Городская база — ленивый id, создаётся один раз на сессию.
        private void RegisterCityGrid(SupplyBalance current)
        {
            if (!cityGridId.IsValid) cityGridId = EntityId.New();
            current.SetSupply(cityGridId, BaseCapacity);
        }

        // Без Growth гаснет с ошибкой: данные для снимка взять негде.
        private bool EnsureReady()
        {
            if (Growth == null)
            {
                Debug.LogError("WasteController: задайте Growth в Inspector.");
                enabled = false;
                return false;
            }
            return true;
        }

        // Тихое создание: вид опрашивает каждый кадр и не должен падать.
        private SupplyBalance EnsureBalance()
        {
            if (balance == null) balance = new SupplyBalance();
            return balance;
        }

        private void ClampSettings()
        {
            if (RefreshIntervalSec < 0.5f) RefreshIntervalSec = 0.5f;
            if (BaseCapacity < 0) BaseCapacity = 0;
        }
    }
}
