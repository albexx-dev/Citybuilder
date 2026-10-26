using UnityEngine;

namespace Citybuilder
{
    // Проводка воды к выросшим и ручным домам для HUD. Поведения не меняет.
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

        private SupplyFeed feed;
        private float timer;

        // Суммарный спрос домов из последнего снимка. Снимка нет — 0.
        public int Demand { get { EnsureFeed(); return Mathf.RoundToInt(feed.TotalDemand); } }

        // Суммарная мощность сети и генерации домов. Снимка нет — 0.
        public int Supply { get { EnsureFeed(); return Mathf.RoundToInt(feed.TotalSupply); } }

        // Доля покрытия 0..1 из последнего снимка. Снимка нет — 0.
        public float Rate01 { get { EnsureFeed(); return feed.Rate01; } }

        // Дом без спроса считается снабжённым. Нет снимка — false.
        public bool IsHomeSupplied(BuildingState home)
        {
            if (home == null || home.Definition == null) return false;
            if (home.Definition.WaterDemand <= 0) return true;
            EnsureFeed();
            return feed.IsSatisfied(home.Id);
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
            EnsureFeed();
            feed.BaseSupply = BaseSupply;
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
            EnsureFeed();
            if (Growth == null) { feed.Clear(); return; }
            feed.BaseSupply = BaseSupply;
            feed.Refresh(Growth.Homes, Placement != null ? Placement.Placed : null);
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
        private void EnsureFeed()
        {
            if (feed != null) return;
            feed = new SupplyFeed(new SupplyBalance(), BaseSupply, def => def.WaterDemand, def => def.WaterOutput);
        }

        private void ClampInterval()
        {
            if (RefreshIntervalSec < 0.5f) RefreshIntervalSec = 0.5f;
            if (BaseSupply < 0) BaseSupply = 0;
        }
    }
}
