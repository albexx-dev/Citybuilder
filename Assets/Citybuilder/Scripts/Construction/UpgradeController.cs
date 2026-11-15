using System;

namespace Citybuilder
{
    // Команды апгрейда станций и служб (EPIC-51): гейт уровня и казны, списание,
    // уровень в state и пересчёт покрытия. Жилые дома не улучшаются — их уровнем
    // ведает HouseLevelPolicy по тикам комфорта (EPIC-59). Чистый C#, без Unity.
    public class UpgradeController
    {
        // Причина отказа: для тестов и отладки. UI молча игнорирует отказ.
        public enum Failure { None, NotUpgradeable, MaxLevel, NotEnoughFunds }

        public struct Result
        {
            public bool Success;
            public Failure Reason;
            public int NewLevel;

            public static Result Ok(int newLevel)
            {
                return new Result { Success = true, Reason = Failure.None, NewLevel = newLevel };
            }

            public static Result Fail(Failure reason)
            {
                return new Result { Success = false, Reason = reason, NewLevel = 0 };
            }
        }

        // Пересчёт снимка покрытия после апгрейда: ServiceCoverageController.RefreshNow.
        private readonly Action coverageRebuild;

        public UpgradeController(Action coverageRebuild)
        {
            this.coverageRebuild = coverageRebuild;
        }

        // Улучшаются только службы с радиусом и станции сетей с полезной мощностью.
        public static bool IsUpgradeable(BuildingDefinition definition)
        {
            if (definition == null || definition.ResidentCapacity > 0) return false;
            return definition.ServiceKind != ServiceKind.None && definition.ServiceRadius > 0
                || definition.PowerOutput > 0 || definition.WaterOutput > 0 || definition.WasteCapacity > 0;
        }

        // Цена кнопки «Улучшить» для текущего уровня. 0 — кнопки нет.
        public static int PriceFor(BuildingDefinition definition, int level)
        {
            if (!IsUpgradeable(definition) || level >= HouseLevelPolicy.MaxLevel) return 0;
            return BuildingLevelPolicy.UpgradeCost(definition.ConstructionCost, level);
        }

        // Гейт → цена → списание из казны → Level++ → пересчёт покрытия.
        public Result TryUpgrade(BuildingState state, Treasury treasury)
        {
            if (state == null || state.Definition == null || !IsUpgradeable(state.Definition))
            {
                return Result.Fail(Failure.NotUpgradeable);
            }
            if (state.Level >= HouseLevelPolicy.MaxLevel)
            {
                return Result.Fail(Failure.MaxLevel);
            }
            int cost = BuildingLevelPolicy.UpgradeCost(state.Definition.ConstructionCost, state.Level);
            if (treasury == null || !treasury.TrySpend(cost, state.Definition.Id).Success)
            {
                return Result.Fail(Failure.NotEnoughFunds);
            }
            state.ApplyUpgrade();
            RebuildCoverage();
            return Result.Ok(state.Level);
        }

        // Снимок пересчитывается только после фактического апгрейда.
        private void RebuildCoverage()
        {
            if (coverageRebuild != null) coverageRebuild();
        }
    }
}
