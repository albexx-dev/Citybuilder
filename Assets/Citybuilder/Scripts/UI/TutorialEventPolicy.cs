namespace Citybuilder
{
    // Подсказки по событиям v1.2 (эпик 97): первый пожар, награда квеста, первый вывоз
    // мусора, наступление ночи. Отделены от цепочки 1–7 TutorialPolicy, поэтому её
    // RU-тексты остаются байт-в-байт. Чистый C#: тестируется через dotnet без Unity.
    public static class TutorialEventPolicy
    {
        public enum Kind
        {
            None = 0,
            FirstFire = 1,
            QuestReward = 2,
            FirstWaste = 3,
            FirstNight = 4
        }

        // Флаги-события и биты подтверждения (Acknowledged: 1 << (Kind - 1)).
        public struct State
        {
            public bool FireHappened;
            public bool HasFireStation;
            public bool QuestRewarded;
            public bool WasteCollected;
            public bool NightArrived;
            public int Acknowledged;
        }

        // Приоритет: пожар, награда, вывоз, ночь. Пожар сам проходит при станции.
        public static Kind Pick(State state)
        {
            if (Pending(state, Kind.FirstFire)) return Kind.FirstFire;
            if (Pending(state, Kind.QuestReward)) return Kind.QuestReward;
            if (Pending(state, Kind.FirstWaste)) return Kind.FirstWaste;
            if (Pending(state, Kind.FirstNight)) return Kind.FirstNight;
            return Kind.None;
        }

        // Текст баннера. Стрелка нужна только для пожара — на карточку каталога.
        public static string FormatText(Kind kind)
        {
            switch (kind)
            {
                case Kind.FirstFire:
                    return Localization.T("tutorial_event_fire");
                case Kind.QuestReward:
                    return Localization.T("tutorial_event_reward");
                case Kind.FirstWaste:
                    return Localization.T("tutorial_event_waste");
                case Kind.FirstNight:
                    return Localization.T("tutorial_event_night");
                default:
                    return string.Empty;
            }
        }

        // Цель стрелки: карточка пожарной станции в каталоге, остальным — без стрелки.
        public static string TargetBuildingId(Kind kind)
        {
            return kind == Kind.FirstFire ? "fire_station" : null;
        }

        public static bool IsAcknowledged(State state, Kind kind)
        {
            return kind != Kind.None && (state.Acknowledged & (1 << ((int)kind - 1))) != 0;
        }

        // Подтверждение: копия состояния с выставленным битом kind.
        public static State Acknowledge(State state, Kind kind)
        {
            if (kind != Kind.None) state.Acknowledged |= 1 << ((int)kind - 1);
            return state;
        }

        private static bool Pending(State state, Kind kind)
        {
            if (IsAcknowledged(state, kind)) return false;
            switch (kind)
            {
                case Kind.FirstFire: return state.FireHappened && !state.HasFireStation;
                case Kind.QuestReward: return state.QuestRewarded;
                case Kind.FirstWaste: return state.WasteCollected;
                case Kind.FirstNight: return state.NightArrived;
                default: return false;
            }
        }
    }
}
