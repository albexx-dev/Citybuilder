using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Проводка воды к выросшим домам для HUD. Только чтение Homes, поведения не меняет.
    // Настройка: объект Grid, Growth обязателен. RefreshNow — для загрузок и мостов.
    public class WaterController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Рост поселений. Обязателен: даёт Homes для спроса и генерации.")]
        public SettlementGrowthController Growth;

        [Header("Manual buildings")]
        [Tooltip("Ручная стройка. Пусто = только выросшие дома.")]
        public BuildingPlacementController Placement;

        [Header("City Grid")]
        [Tooltip("Мощность городского водопровода до появления водонапорных зданий.")]
        public int BaseSupply = 12;

        [Header("Refresh")]
        [Tooltip("Пауза между пересчётами в секундах. Минимум 0.5.")]
        public float RefreshIntervalSec = 2f;

        private WaterSystem water;
        private EntityId cityGridId;
        private float timer;

        // Суммарный спрос домов из последнего снимка. Системы нет — 0.
        public int Demand
        {
            get
            {
                EnsureSystem();
                return Mathf.RoundToInt(water.TotalDemand);
            }
        }

        // Суммарная мощность сети и генерации домов. Системы нет — 0.
        public int Supply
        {
            get
            {
                EnsureSystem();
                return Mathf.RoundToInt(water.TotalSupply);
            }
        }

        // Доля покрытия 0..1 из последнего снимка. Системы нет — 0.
        public float Rate01
        {
            get
            {
                EnsureSystem();
                return water.SatisfactionRate01;
            }
        }

        // Дом без спроса считается снабжённым. Нет снимка — false.
        public bool IsHomeSupplied(BuildingState home)
        {
            if (home == null || home.Definition == null) return false;
            if (home.Definition.WaterDemand <= 0) return true;
            EnsureSystem();
            return water.IsSatisfied(home.Id);
        }

        private void Start()
        {
            ClampInterval();
            if (!EnsureReady()) return;
            RefreshNow();
        }

        private void OnValidate()
        {
            ClampInterval();
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
            EnsureSystem();
            water.Clear();
            if (Growth == null) return;
            RegisterCityGrid();
            AddStates(Growth.Homes);
            if (Placement == null) return;
            AddStates(Placement.Placed);
        }

        // Один проход по списку: null-список и null-элементы пропускаются.
        private void AddStates(List<BuildingState> homes)
        {
            if (homes == null) return;
            for (int i = 0; i < homes.Count; i++) AddHome(homes[i]);
        }

        // Один дом в баланс: спрос и генерация только при значениях больше нуля.
        private void AddHome(BuildingState home)
        {
            if (home == null || home.Definition == null) return;
            BuildingDefinition def = home.Definition;
            if (def.WaterDemand > 0) water.AddConsumer(home.Id, def.WaterDemand);
            if (def.WaterOutput > 0) water.AddSource(home.Id, def.WaterOutput);
        }

        // Городская сеть — ленивый id, создаётся один раз на сессию.
        private void RegisterCityGrid()
        {
            if (!cityGridId.IsValid) cityGridId = EntityId.New();
            water.AddSource(cityGridId, BaseSupply);
        }

        // Чинит ссылки после перезагрузки домена. Без Growth гаснет с ошибкой.
        private bool EnsureReady()
        {
            if (Growth == null)
            {
                Debug.LogError("WaterController: задайте Growth в Inspector.");
                enabled = false;
                return false;
            }
            return true;
        }

        // Тихое создание: вид опрашивает каждый кадр.
        private void EnsureSystem()
        {
            if (water != null) return;
            water = new WaterSystem();
        }

        private void ClampInterval()
        {
            if (RefreshIntervalSec < 0.5f) RefreshIntervalSec = 0.5f;
            if (BaseSupply < 0) BaseSupply = 0;
        }
    }
}
