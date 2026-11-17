using System;

namespace Citybuilder
{
    // Налоговая политика города (эпик #49): три зоны — Жилая/Коммерция/Промзона, у каждой
    // своя лестница 2/4/6 за работающего (ребаланс 171), дефолт 4/4/4. Чистая логика без Unity.
    // Ставки живут в памяти сцены. Сейв v5 (эпик #64) хранит три ставки раздельно, они и встают
    // по зонам при загрузке; слот v4 хранит одну ставку — тогда все три зоны равны ей.
    public class TaxPolicy
    {
        public enum Level
        {
            Low,
            Normal,
            High
        }

        private Level residential = Level.Normal;
        private Level commercial = Level.Normal;
        private Level industrial = Level.Normal;

        // Уровень зоны. None и битые значения — обычный.
        public Level LevelFor(ZoneType zone)
        {
            if (zone == ZoneType.Residential) return residential;
            if (zone == ZoneType.Commercial) return commercial;
            if (zone == ZoneType.Industrial) return industrial;
            return Level.Normal;
        }

        // Ставка за одного работающего в час: низкая 2, обычная 4, высокая 6. Вне зон — 0.
        public int RateFor(ZoneType zone)
        {
            if (zone == ZoneType.None) return 0;
            switch (LevelFor(zone))
            {
                case Level.Low:
                    return 2;
                case Level.High:
                    return 6;
                default:
                    return 4;
            }
        }

        // Сменить уровень зоны: битый уровень — обычный, None игнорируется.
        public void SetZone(ZoneType zone, Level level)
        {
            Level safe = IsKnown(level) ? level : Level.Normal;
            if (zone == ZoneType.Residential) residential = safe;
            else if (zone == ZoneType.Commercial) commercial = safe;
            else if (zone == ZoneType.Industrial) industrial = safe;
        }

        // Следующий уровень зоны по кругу: низкий → обычный → высокий → низкий.
        public Level NextZone(ZoneType zone)
        {
            SetZone(zone, Step(LevelFor(zone), 1));
            return LevelFor(zone);
        }

        // Сбор за час с зоны: занятые зоны × ставка. Работающих нет или ставка 0 — 0.
        public int IncomeForZone(ZoneType zone, int zoneEmployed)
        {
            if (zoneEmployed <= 0) return 0;
            return zoneEmployed * RateFor(zone);
        }

        // Строка бюджета для зоны: «Жилая: обычный (4) +8 $/ч».
        public string ZoneLabel(ZoneType zone, int zoneEmployed)
        {
            return ZoneName(zone) + ": " + NameOf(LevelFor(zone)) + " (" + RateFor(zone) + ") "
                + "+" + IncomeForZone(zone, zoneEmployed) + " $/ч";
        }

        // === Совместимость с одной ставкой (сейв v4 и старый HUD) ===

        // Уровень жилой зоны: в сейв v4 пишется одна ставка (v5 — эпик #64).
        public Level Current
        {
            get { return residential; }
        }

        // Старое свойство: ставка жилой зоны.
        public int PerWorker
        {
            get { return RateFor(ZoneType.Residential); }
        }

        // Один уровень всем зонам: новая игра и чтение v4 («старая ставка на всё»).
        public void Set(Level level)
        {
            Level safe = IsKnown(level) ? level : Level.Normal;
            residential = safe;
            commercial = safe;
            industrial = safe;
        }

        private static Level Step(Level level, int delta)
        {
            Level safe = IsKnown(level) ? level : Level.Normal;
            return (Level)(((int)safe + delta + 3) % 3);
        }

        private static bool IsKnown(Level level)
        {
            return level == Level.Low || level == Level.Normal || level == Level.High;
        }

        private static string NameOf(Level level)
        {
            if (level == Level.Low) return "низкий";
            if (level == Level.High) return "высокий";
            return "обычный";
        }

        private static string ZoneName(ZoneType zone)
        {
            if (zone == ZoneType.Commercial) return "Коммерция";
            if (zone == ZoneType.Industrial) return "Промзона";
            return "Жилая";
        }
    }
}
