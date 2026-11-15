namespace Citybuilder
{
    // Снимок метрик города за один игровой час (EPIC-63). Чистый C# без Unity.
    // Поля readonly: снимок неизменяем после записи в историю.
    public struct MetricsSnapshot
    {
        // Игровой час от старта симуляции (SimulationTime.TickCount); 1 тик = 1 час.
        public readonly int Hour;

        public readonly int Population;

        // Баланс казны; минус — допустимое штатное значение (дефицит содержания).
        public readonly int Balance;

        // Среднее удовлетворение потребностей, 0..100. Источник пока закрыт
        // (NeedSystem недоступен через публичный API) — пишется 0, см. MetricsRecorder.
        public readonly float NeedsAvg;

        // Преступность, 0..100. Заглушка 0 до системы преступности (задача #47).
        public readonly float CrimePercent;

        // Покрытие сетей, 0..100 (Rate01 * 100).
        public readonly float PowerPercent;
        public readonly float WaterPercent;
        public readonly float WastePercent;

        public MetricsSnapshot(int hour, int population, int balance,
            float needsAvg, float crimePercent,
            float powerPercent, float waterPercent, float wastePercent)
        {
            Hour = hour;
            Population = population;
            Balance = balance;
            NeedsAvg = needsAvg;
            CrimePercent = crimePercent;
            PowerPercent = powerPercent;
            WaterPercent = waterPercent;
            WastePercent = wastePercent;
        }
    }
}
