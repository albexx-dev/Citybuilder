namespace Citybuilder
{
    // Три налоговые ставки в слоте v5 (EPIC-64 шаг 1): запись из TaxPolicy и чтение обратно.
    // Порядок: [0] жилая, [1] коммерция, [2] промзона; значения 1..3, как в SaveSystem.EncodeTaxLevel.
    // Пустой или битый массив = слот v4: все три зоны берутся из одного поля TaxLevel.
    public static class SnapshotTaxRates
    {
        public const int Count = 3;
        private const int MinRate = 1;
        private const int MaxRate = 3;

        // Три расшифрованные ставки: порядок тот же, что в массиве слота.
        public struct Levels
        {
            public TaxPolicy.Level Residential;
            public TaxPolicy.Level Commercial;
            public TaxPolicy.Level Industrial;
        }

        // Запись в снапшот: политики нет — пустой массив, слот всё равно читается.
        public static void Write(CitySnapshot snapshot, TaxPolicy policy)
        {
            if (snapshot == null) return;
            if (policy == null)
            {
                snapshot.TaxRates = new int[0];
                return;
            }
            int[] rates = new int[Count];
            rates[0] = SaveSystem.EncodeTaxLevel(policy.LevelFor(ZoneType.Residential));
            rates[1] = SaveSystem.EncodeTaxLevel(policy.LevelFor(ZoneType.Commercial));
            rates[2] = SaveSystem.EncodeTaxLevel(policy.LevelFor(ZoneType.Industrial));
            snapshot.TaxRates = rates;
        }

        // Чтение трёх ставок: годный набор — по зонам, иначе одна ставка TaxLevel на все зоны.
        public static Levels Read(CitySnapshot snapshot)
        {
            int[] rates = snapshot == null ? null : snapshot.TaxRates;
            if (IsSet(rates))
            {
                return new Levels
                {
                    Residential = SaveSystem.DecodeTaxLevel(rates[0]),
                    Commercial = SaveSystem.DecodeTaxLevel(rates[1]),
                    Industrial = SaveSystem.DecodeTaxLevel(rates[2])
                };
            }
            int taxLevel = snapshot == null ? 0 : snapshot.TaxLevel;
            TaxPolicy.Level single = SaveSystem.DecodeTaxLevel(taxLevel);
            return new Levels
            {
                Residential = single,
                Commercial = single,
                Industrial = single
            };
        }

        // Проверка поля слота: null/пусто допустимо (слот v4), иначе ровно три ставки 1..3.
        public static bool IsValid(int[] rates)
        {
            return rates == null || rates.Length == 0 || IsSet(rates);
        }

        // Применение ставок политике после загрузки (EPIC-64 шаг 2): политики нет — ничего не делаем.
        // Годный набор — раздельные ставки по зонам; пустой или битый (слот v4) — одна TaxLevel на все зоны.
        public static void ApplyTo(CitySnapshot snapshot, TaxPolicy policy)
        {
            if (policy == null) return;
            int[] rates = snapshot == null ? null : snapshot.TaxRates;
            if (!IsSet(rates))
            {
                policy.Set(SaveSystem.DecodeTaxLevel(snapshot == null ? 0 : snapshot.TaxLevel));
                return;
            }
            Levels levels = Read(snapshot);
            policy.SetZone(ZoneType.Residential, levels.Residential);
            policy.SetZone(ZoneType.Commercial, levels.Commercial);
            policy.SetZone(ZoneType.Industrial, levels.Industrial);
        }

        // Годный набор из трёх ставок: только тогда зоны читаются раздельно.
        private static bool IsSet(int[] rates)
        {
            if (rates == null || rates.Length != Count) return false;
            for (int i = 0; i < rates.Length; i++)
            {
                if (rates[i] < MinRate || rates[i] > MaxRate) return false;
            }
            return true;
        }
    }
}
