using System.Collections.Generic;

namespace Citybuilder
{
    // Правило «может ли вырасти следующий дом»: режим роста, потолок, зоны, казна.
    // Чистый C#, без Unity. Контроллер — тонкий адаптер: тик → policy → действие.
    public class GrowthEligibilityPolicy
    {
        // Причина запрета роста. None — расти можно. NoMoney — нечем платить за рост,
        // Deficit — казна ушла в минус от содержания зданий.
        public enum Reason { None, GrowthOff, Ceiling, NoSpot, NoMoney, Deficit }

        // Итог проверки: запрет с причиной или разрешение с парой «тип + клетка».
        public struct Verdict
        {
            public bool Allowed;
            public Reason BlockedBy;
            public BuildingDefinition Definition;
            public GridPosition Origin;
        }

        // Дешёвая проверка на каждый тик: режим включён и потолок не достигнут.
        public bool CanProgress(bool placementActive, int settledCount, int maxSettlements)
        {
            return placementActive && settledCount < maxSettlements;
        }

        // Полная проверка в момент роста: режим → потолок → место → казна.
        // Treasury null — рост бесплатный; списки не-null (гарантия контроллера).
        public Verdict Evaluate(bool placementActive, SettlementSystem settlement, int maxSettlements,
            List<KeyValuePair<GridPosition, ZoneType>> zonedCells, ZoneType lastGrownZone,
            int grownCount, List<BuildingDefinition> definitions, Treasury treasury)
        {
            if (settlement == null || zonedCells == null || definitions == null)
                return Block(Reason.NoSpot);
            if (!CanProgress(placementActive, settlement.SettledCount, maxSettlements))
                return Block(placementActive ? Reason.Ceiling : Reason.GrowthOff);
            BuildingDefinition definition;
            GridPosition origin;
            if (!FindSpot(zonedCells, lastGrownZone, grownCount, definitions, settlement, out definition, out origin))
                return Block(Reason.NoSpot);
            if (treasury != null && treasury.IsInDeficit) return Block(Reason.Deficit);
            if (treasury != null && !treasury.CanAfford(definition.ConstructionCost))
                return Block(Reason.NoMoney);
            return new Verdict { Allowed = true, Definition = definition, Origin = origin };
        }

        private static Verdict Block(Reason reason) { return new Verdict { BlockedBy = reason }; }

        // Первая зонная клетка и первый подходящий тип под её зону. Чередование:
        // сначала чужие зоны (не как прошлый рост), потом все. Закрытые типы — мимо.
        private static bool FindSpot(List<KeyValuePair<GridPosition, ZoneType>> zonedCells, ZoneType lastGrownZone,
            int grownCount, List<BuildingDefinition> definitions, SettlementSystem settlement,
            out BuildingDefinition definition, out GridPosition origin)
        {
            definition = null;
            origin = default(GridPosition);
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < zonedCells.Count; i++)
                {
                    if (pass == 0 && zonedCells[i].Value == lastGrownZone) continue;
                    for (int d = 0; d < definitions.Count; d++)
                    {
                        BuildingDefinition candidate = definitions[d];
                        if (candidate == null) continue;
                        if (!UnlockPolicy.IsUnlocked(candidate, grownCount)) continue;
                        if (candidate.AllowedZone != ZoneType.None && candidate.AllowedZone != zonedCells[i].Value) continue;
                        if (!settlement.CanSettle(candidate, zonedCells[i].Key)) continue;
                        definition = candidate;
                        origin = zonedCells[i].Key;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
