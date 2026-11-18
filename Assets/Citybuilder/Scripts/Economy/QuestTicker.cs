namespace Citybuilder
{
    // Связка QuestSystem с городом на границе тика (EPIC-48, ADR-012): собирает снимок
    // QuestContext из plain-систем (население, счётчик роста, службы, казна) и зовёт
    // проверки домена. Plain C#: как FireTicker и CrimeTicker, ссылки подаёт QuestLogView.
    public class QuestTicker
    {
        private readonly QuestSystem quests = new QuestSystem();
        private int lastTick;

        // Домен квестов: для панели QuestLogView и тестов.
        public QuestSystem Quests
        {
            get { return quests; }
        }

        // Один тик: снимок города, проверка активных задач, награды казне, лента.
        public void Tick(int tick, PopulationSystem population, int buildings,
            ServiceBuildingSystem coverage, Treasury treasury, CityEventLog log)
        {
            if (tick < lastTick)
            {
                // Время отмотано назад («Новая игра»/загрузка): квесты не сериализуются
                // до v5 (#64) — начинаем цепочку заново.
                quests.Reset();
            }
            lastTick = tick;
            quests.Tick(tick, BuildContext(population, buildings, coverage, treasury), treasury, log);
        }

        // Явный сброс «Новая игра» (резервный путь, без отмотки времени).
        public void Reset()
        {
            quests.Reset();
        }

        // Снимок города: нет системы — безопасные нули, покрытие без снимка — 1.
        private static QuestContext BuildContext(PopulationSystem population, int buildings,
            ServiceBuildingSystem coverage, Treasury treasury)
        {
            QuestContext context = new QuestContext();
            context.Residents = population != null ? population.TotalResidents : 0;
            context.Buildings = buildings;
            context.Balance = treasury != null ? treasury.Balance : 0;
            context.Parks = coverage != null ? coverage.SourcesCount(ServiceKind.Leisure) : 0;
            context.FireCoverage = CityCoverage(coverage, ServiceKind.Fire);
            context.PoliceCoverage = CityCoverage(coverage, ServiceKind.Police);
            context.HealthCoverage = CityCoverage(coverage, ServiceKind.Health);
            context.LeisureCoverage = CityCoverage(coverage, ServiceKind.Leisure);
            return context;
        }

        // Доля покрытых домов службы (0..1); без снимка — 1 (потребность не блокирует город).
        private static float CityCoverage(ServiceBuildingSystem coverage, ServiceKind kind)
        {
            if (coverage == null) return 1f;
            return coverage.Coverage01(kind);
        }
    }
}
