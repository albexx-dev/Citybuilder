using System;

namespace Citybuilder
{
    // Чистый расчёт экономики города за час и за период. Без Unity: для сценарных тестов ребаланса.
    public static class EconomyMath
    {
        // Правило занятости EmploymentSystem: работает не больше жителей и не больше мест.
        public static int Employed(int residents, int jobs)
        {
            return Math.Min(Math.Max(residents, 0), Math.Max(jobs, 0));
        }

        // Ставка 0 и ниже — сбор выключен, как в TaxSystem.ComputeTax.
        public static int IncomePerHour(int employed, int taxPerWorker)
        {
            if (taxPerWorker <= 0)
            {
                return 0;
            }
            return employed * taxPerWorker;
        }

        // Итог часа: налоги работающих минус содержание зданий.
        public static int NetPerHour(int residents, int jobs, int taxPerWorker, int upkeepPerHour)
        {
            return IncomePerHour(Employed(residents, jobs), taxPerWorker) - upkeepPerHour;
        }

        // Баланс через N часов при постоянных ставках: старт плюс итог часа × часы.
        public static int BalanceAfterHours(int startBalance, int residents, int jobs,
            int taxPerWorker, int upkeepPerHour, int hours)
        {
            int net = NetPerHour(residents, jobs, taxPerWorker, upkeepPerHour);
            return startBalance + net * hours;
        }
    }
}
