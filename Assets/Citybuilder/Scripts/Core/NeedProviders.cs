using System;

namespace Citybuilder
{
    // Провайдеры потребностей (EPIC-03): энергия, вода, мусор, пожарные, здоровье и досуг. Чистый C#, без Unity.
    // null — провайдер не подключён: его нет ни в сумме, ни в счётчике (0..6).
    public class NeedProviders
    {
        private readonly Func<float> powerRate;
        private readonly Func<float> waterRate;
        private readonly Func<float> wasteRate;
        private readonly Func<float> fireRate;
        private readonly Func<float> healthRate;
        private readonly Func<float> leisureRate;

        // Ни одного провайдера: все ставки 1, сумма 0, счётчик 0.
        public NeedProviders()
            : this(null, null, null, null, null, null) { }

        public NeedProviders(Func<float> powerRate, Func<float> waterRate)
            : this(powerRate, waterRate, null, null, null, null) { }

        public NeedProviders(Func<float> powerRate, Func<float> waterRate, Func<float> wasteRate)
            : this(powerRate, waterRate, wasteRate, null, null, null) { }

        public NeedProviders(Func<float> powerRate, Func<float> waterRate,
            Func<float> wasteRate, Func<float> fireRate)
            : this(powerRate, waterRate, wasteRate, fireRate, null, null) { }

        public NeedProviders(Func<float> powerRate, Func<float> waterRate,
            Func<float> wasteRate, Func<float> fireRate,
            Func<float> healthRate, Func<float> leisureRate)
        {
            this.powerRate = powerRate;
            this.waterRate = waterRate;
            this.wasteRate = wasteRate;
            this.fireRate = fireRate;
            this.healthRate = healthRate;
            this.leisureRate = leisureRate;
        }

        // Провайдер здоровья из системы служб.
        public static Func<float> Health(ServiceBuildingSystem services)
        {
            if (services == null) return null;
            return () => services.Coverage01(ServiceKind.Health);
        }

        // Провайдер досуга из системы служб.
        public static Func<float> Leisure(ServiceBuildingSystem services)
        {
            if (services == null) return null;
            return () => services.Coverage01(ServiceKind.Leisure);
        }

        // Флаг провайдера: не подключён — показателя нет в среднем.
        public bool HasPower { get { return powerRate != null; } }
        public bool HasWater { get { return waterRate != null; } }
        public bool HasWaste { get { return wasteRate != null; } }
        public bool HasFire { get { return fireRate != null; } }
        public bool HasHealth { get { return healthRate != null; } }
        public bool HasLeisure { get { return leisureRate != null; } }

        public float PowerRate { get { return Rate01(powerRate); } }
        public float WaterRate { get { return Rate01(waterRate); } }
        public float WasteRate { get { return Rate01(wasteRate); } }
        public float FireRate { get { return Rate01(fireRate); } }
        public float HealthRate { get { return Rate01(healthRate); } }
        public float LeisureRate { get { return Rate01(leisureRate); } }

        // Сумма только подключённых показателей.
        public float Sum
        {
            get
            {
                float total = 0f;
                if (HasPower) total += PowerRate;
                if (HasWater) total += WaterRate;
                if (HasWaste) total += WasteRate;
                if (HasFire) total += FireRate;
                if (HasHealth) total += HealthRate;
                if (HasLeisure) total += LeisureRate;
                return total;
            }
        }

        // Число подключённых провайдеров 0..6.
        public int Count
        {
            get
            {
                int count = 0;
                if (HasPower) count++;
                if (HasWater) count++;
                if (HasWaste) count++;
                if (HasFire) count++;
                if (HasHealth) count++;
                if (HasLeisure) count++;
                return count;
            }
        }

        // Ставка провайдера 0..1. Провайдера нет — 1: потребность не ограничивает.
        private static float Rate01(Func<float> provider)
        {
            if (provider == null) return 1f;
            return Clamp01(provider());
        }

        private static float Clamp01(float value)
        {
            // NaN не сравним ни с чем: считаем его «нет ограничения» — как отсутствующий провайдер.
            if (float.IsNaN(value)) return 1f;
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
