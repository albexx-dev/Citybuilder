namespace Citybuilder
{
    // Правило уровня дома (EPIC-59): 1..3 по комфорту с гистерезисом, чтобы уровень не мигал.
    // Чистый C#, без Unity: вход — уровень, серия тиков и комфорт 0..1; выход — действие и новое состояние.
    // Серию тиков хранит BuildingState.ComfortStreak: >0 — хорошие тики подряд, <0 — плохие.
    public class HouseLevelPolicy
    {
        public const int MinLevel = 1;
        public const int MaxLevel = 3;
        public const int RaiseTicks = 20; // хороших тиков подряд до повышения
        public const int LowerTicks = 40; // плохих тиков подряд до понижения
        public const float RaiseComfort = 0.8f; // комфорт ≥ порога — хороший тик
        public const float LowerComfort = 0.4f; // комфорт ≤ порога — плохой тик

        // Действие тика: Hold — ничего, Raise/Lower — смена уровня на 1.
        public enum LevelAction { Hold, Raise, Lower }

        // Итог тика: действие, уровень и серия после него (уровень уже клампнут 1..3).
        public struct Verdict
        {
            public LevelAction Action;
            public int Level;
            public int Streak;
        }

        // Один тик комфорта. NaN — данных нет: Hold без изменения уровня и серии,
        // чтобы потеря данных не двигала уровень и не обнуляла накопленный прогресс.
        public Verdict Evaluate(int level, int streak, float comfort)
        {
            if (float.IsNaN(comfort)) return Hold(level, streak);
            if (comfort >= RaiseComfort) return Step(level, streak >= 0 ? streak + 1 : 1);
            if (comfort <= LowerComfort) return Step(level, streak <= 0 ? streak - 1 : -1);
            return Hold(level, 0); // середина шкалы сбрасывает обе серии
        }

        // Серия доросла до порога — ровно одно действие и сброс серии; иначе Hold с накопленной серией.
        private static Verdict Step(int level, int streak)
        {
            if (streak >= RaiseTicks)
            {
                return level < MaxLevel
                    ? new Verdict { Action = LevelAction.Raise, Level = level + 1, Streak = 0 }
                    : Hold(level, 0); // потолок: серия гаснет, чтобы не копить впустую
            }
            if (streak <= -LowerTicks)
            {
                return level > MinLevel
                    ? new Verdict { Action = LevelAction.Lower, Level = level - 1, Streak = 0 }
                    : Hold(level, 0); // пол: аналогично
            }
            return Hold(level, streak);
        }

        private static Verdict Hold(int level, int streak)
        {
            return new Verdict { Action = LevelAction.Hold, Level = ClampLevel(level), Streak = streak };
        }

        private static int ClampLevel(int level)
        {
            return level < MinLevel ? MinLevel : (level > MaxLevel ? MaxLevel : level);
        }
    }
}
