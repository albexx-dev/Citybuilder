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
                // Время отмотано назад без команды сброса (страховка): начинаем цепочку заново.
                // Обычные «Новая игра»/загрузка идут через Reset — см. QuestLogView.ResetQuests.
                quests.Reset();
            }
            lastTick = tick;
            quests.Tick(tick, BuildContext(population, buildings, coverage, treasury), treasury, log);
        }

        // Сброс по команде QuestLogView (LoadController.ClearAll). Обнуляет и lastTick:
        // без этого первый тик после загрузки слота с меньшим номером тика считался бы
        // «отмоткой» и стёр бы цепочку, восстановленную из слота v5 (QuestSnapshot).
        public void Reset()
        {
            quests.Reset();
            lastTick = 0;
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
