using System;

namespace Citybuilder
{
    // Налоговая политика города: три уровня и ставка за работающего.
    // Чистая логика без Unity: текущий уровень хранит сам объект.
    public class TaxPolicy
    {
        public enum Level
        {
            Low,
            Normal,
            High
        }

        private Level current = Level.Normal;

        // Текущий уровень. Неизвестное значение не хранится: всегда Normal.
        public Level Current
        {
            get { return current; }
        }

        // Ставка за одного работающего за час: низкий 1, обычный 2, высокий 3.
        public int PerWorker
        {
            get
            {
                switch (current)
                {
                    case Level.Low:
                        return 1;
                    case Level.High:
                        return 3;
                    default:
                        return 2;
                }
            }
        }

        // Текст кнопки HUD: «Налог: обычный (2)».
        public string Label
        {
            get { return "Налог: " + NameOf(current) + " (" + PerWorker + ")"; }
        }

        // Следующий уровень по кругу: низкий → обычный → высокий → низкий.
        public Level Next()
        {
            Set(Step(current, 1));
            return current;
        }

        // Предыдущий уровень по кругу: низкий → высокий → обычный → низкий.
        public Level Previous()
        {
            Set(Step(current, -1));
            return current;
        }

        // Неизвестное значение enum (например, из старого слота) — обычный налог.
        public void Set(Level level)
        {
            current = IsKnown(level) ? level : Level.Normal;
        }

        private static Level Step(Level level, int delta)
        {
            Level safe = IsKnown(level) ? level : Level.Normal;
            int count = 3;
            int index = ((int)safe + delta + count) % count;
            return (Level)index;
        }

        private static bool IsKnown(Level level)
        {
            return level == Level.Low || level == Level.Normal || level == Level.High;
        }

        private static string NameOf(Level level)
        {
            switch (level)
            {
                case Level.Low:
                    return "низкий";
                case Level.High:
                    return "высокий";
                default:
                    return "обычный";
            }
        }
    }
}
