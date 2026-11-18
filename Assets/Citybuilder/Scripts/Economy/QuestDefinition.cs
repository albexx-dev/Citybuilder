using System;

namespace Citybuilder
{
    // Снимок города для проверки квестов (EPIC-48, ADR-012). Plain-данные без Unity:
    // жители, выросшие здания, покрытие служб 0..1, баланс казны и число парков.
    public struct QuestContext
    {
        public int Residents;
        public int Buildings;
        public float FireCoverage;
        public float PoliceCoverage;
        public float HealthCoverage;
        public float LeisureCoverage;
        public int Balance;
        public int Parks;
    }

    // Определение квеста-вехи (ADR-012): уникальный id, тексты, награда казне и
    // предикат выполнения по снимку города. Выполняется ровно один раз: id уходит
    // в выполненные (QuestSystem) и больше не предлагается генератором.
    public struct QuestDefinition
    {
        public readonly string Id;
        public readonly string Title;
        public readonly string Description;
        public readonly int Reward;
        private readonly Func<QuestContext, bool> isSatisfied;

        public QuestDefinition(string id, string title, string description, int reward,
            Func<QuestContext, bool> isSatisfied)
        {
            Id = id;
            Title = title;
            Description = description;
            Reward = reward;
            this.isSatisfied = isSatisfied;
        }

        // Выполнен ли квест при данном снимке города. Без предиката — никогда.
        public bool IsSatisfied(QuestContext context)
        {
            return isSatisfied != null && isSatisfied(context);
        }
    }
}
