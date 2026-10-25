using System;

namespace Citybuilder
{
    // Провайдеры потребностей: энергия, вода, мусор. Чистый C#, без Unity.
    // null — провайдер не подключён: его нет ни в сумме, ни в счётчике.
    public class NeedProviders
    {
        private readonly Func<float> powerRate;
        private readonly Func<float> waterRate;
        private readonly Func<float> wasteRate;

        // Ни одного провайдера: все ставки 1, сумма 0, счётчик 0.
        public NeedProviders()
            : this(null, null, null) { }

        public NeedProviders(Func<float> powerRate, Func<float> waterRate)
            : this(powerRate, waterRate, null) { }

        public NeedProviders(Func<float> powerRate, Func<float> waterRate, Func<float> wasteRate)
        {
            this.powerRate = powerRate;
            this.waterRate = waterRate;
            this.wasteRate = wasteRate;
        }

        // Флаг провайдера: не подключён — показателя нет в среднем.
        public bool HasPower
        {
            get { return powerRate != null; }
        }
        public bool HasWater
        {
            get { return waterRate != null; }
        }
        public bool HasWaste
        {
            get { return wasteRate != null; }
        }

        public float PowerRate
        {
            get { return Rate01(powerRate); }
        }
        public float WaterRate
        {
            get { return Rate01(waterRate); }
        }
        public float WasteRate
        {
            get { return Rate01(wasteRate); }
        }

        // Сумма только подключённых показателей.
        public float Sum
        {
            get
            {
                float total = 0f;
                if (HasPower) total += PowerRate;
                if (HasWater) total += WaterRate;
                if (HasWaste) total += WasteRate;
                return total;
            }
        }

        // Число подключённых провайдеров 0..3.
        public int Count
        {
            get
            {
                int count = 0;
                if (HasPower) count++;
                if (HasWater) count++;
                if (HasWaste) count++;
                return count;
            }
        }

        // Ставка провайдера 0..1. Провайдера нет — 1: потребность не ограничивает.
        private static float Rate01(Func<float> provider)
        {
            if (provider == null)
            {
                return 1f;
            }
            return Clamp01(provider());
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }
            if (value > 1f)
            {
                return 1f;
            }
            return value;
        }
    }
}
