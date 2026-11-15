using UnityEngine;

namespace Citybuilder
{
    // Писатель истории метрик (EPIC-63): подписан на TickCompleted и раз в игровой
    // час кладёт снимок в MetricsHistory. Тонкий адаптер — домен историю не знает.
    // Шаг 2 эпика: в сцену добавляет оркестратор на приёмке (объект Simulation).
    public class MetricsRecorder : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Драйвер часов. Обязателен: даёт TickCompleted раз в игровой час.")] public SimulationRunner Runner;
        [Tooltip("Население. Пусто = ищется в сцене.")] public PopulationController Population;
        [Tooltip("Вид казны. Пусто = ищется в сцене.")] public TreasuryView Treasury;
        [Tooltip("Рост поселений. Пусто = ищется в сцене: даёт контроллеры сетей.")] public SettlementGrowthController Growth;
        [Tooltip("Энергия. Пусто = ищется на объекте Growth.")] public PowerController Power;
        [Tooltip("Вода. Пусто = ищется на объекте Growth.")] public WaterController Water;
        [Tooltip("Мусор. Пусто = ищется на объекте Growth.")] public WasteController Waste;

        private readonly MetricsHistory history = new MetricsHistory();
        private long lastRecordedTick = -1; // тик последней записи: повтор/откат времени — мимо

        // История для графиков (#68): читать через Snapshot(), запись — только здесь.
        public MetricsHistory History { get { return history; } }

        // «Новая игра»/загрузка: история начинается заново (issue #63, шаг интеграции).
        public void Clear() { history.Clear(); }

        private void OnEnable()
        {
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Runner == null)
            {
                Debug.LogError("MetricsRecorder: задайте Runner в Inspector.");
                enabled = false;
                return;
            }
            Runner.Clock.TickCompleted += OnClockTick;
        }

        private void OnDisable()
        {
            if (Runner != null && Runner.Clock != null) Runner.Clock.TickCompleted -= OnClockTick;
        }

        private void OnClockTick(SimulationTime time)
        {
            // Тик = игровой час: пишем каждый тик, но только строго новые часы.
            // Не «hour == lastHour + 1»: после тихого RestoreTime загрузки записка
            // застряла бы навсегда; «строго больше» даёт тот же запрет повторов.
            // Скачок времени назад (Новая игра / загрузка старого сейва): история
            // начинается заново, иначе она замирает до отмотанного тика (issue #63).
            if (time.TickCount < lastRecordedTick)
            {
                history.Clear();
            }
            if (time.TickCount == lastRecordedTick) return;
            lastRecordedTick = time.TickCount;
            Record((int)time.TickCount);
        }

        private void Record(int hour)
        {
            ResolveCityRefs();
            history.Record(new MetricsSnapshot(
                hour,
                Population != null ? Population.Population.TotalResidents : 0,
                Treasury != null && Treasury.Treasury != null ? Treasury.Treasury.Balance : 0,
                0f, // NeedSystem не открыт наружу (PopulationController/NeedsView держат его приватно)
                0f, // преступность — заглушка до системы #47
                Power != null ? Power.Rate01 * 100f : 0f,
                Water != null ? Water.Rate01 * 100f : 0f,
                Waste != null ? Waste.Rate01 * 100f : 0f));
        }

        private void ResolveCityRefs()
        {
            if (Population == null) Population = FindFirstObjectByType<PopulationController>();
            if (Treasury == null) Treasury = FindFirstObjectByType<TreasuryView>();
            if (Growth == null) Growth = FindFirstObjectByType<SettlementGrowthController>();
            if (Growth == null) return;
            if (Power == null) Power = Growth.GetComponent<PowerController>();
            if (Water == null) Water = Growth.GetComponent<WaterController>();
            if (Waste == null) Waste = Growth.GetComponent<WasteController>();
        }
    }
}
