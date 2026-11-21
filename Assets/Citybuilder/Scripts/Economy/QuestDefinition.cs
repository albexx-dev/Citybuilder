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
        public readonly int Reward;
        private readonly string titleKey;
        private readonly string descriptionKey;
        private readonly object[] descriptionArgs;
        private readonly Func<QuestContext, bool> isSatisfied;

        // Тексты вычисляются при чтении: смена языка видна в уже выданных задачах (issue #100).
        public string Title { get { return Localization.T(titleKey); } }
        public string Description { get { return Localization.T(descriptionKey, descriptionArgs); } }

        public QuestDefinition(string id, string titleKey, string descriptionKey, int reward,
            Func<QuestContext, bool> isSatisfied)
            : this(id, titleKey, descriptionKey, null, reward, isSatisfied)
        {
        }

        public QuestDefinition(string id, string titleKey, string descriptionKey, object[] args, int reward,
            Func<QuestContext, bool> isSatisfied)
        {
            Id = id;
            this.titleKey = titleKey;
            this.descriptionKey = descriptionKey;
            descriptionArgs = args;
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
