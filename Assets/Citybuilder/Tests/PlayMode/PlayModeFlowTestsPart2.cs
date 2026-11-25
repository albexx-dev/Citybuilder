using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Citybuilder
{
    // Хвост класса PlayModeFlowTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class PlayModeFlowTests
    {
        private void BuildCity()
        {
            runner = root.AddComponent<SimulationRunner>();
            runner.enabled = false;
            GridVisualizer grid = root.AddComponent<GridVisualizer>();
            BuildGrowthChain(grid);
            BuildTickerAndSave(grid);
        }

        // Зоны, рост и определение дома (issue #127).
        private void BuildGrowthChain(GridVisualizer grid)
        {
            zones = root.AddComponent<ZonePaintingController>();
            zones.GridVisualizer = grid;
            zones.placementActive = false;
            TreasuryView treasury = root.AddComponent<TreasuryView>();
            def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house";
            def.Footprint = new Vector2Int(1, 1);
            def.ConstructionCost = 0;
            def.AllowedZone = ZoneType.Residential;
            growth = root.AddComponent<SettlementGrowthController>();
            growth.GridVisualizer = grid;
            growth.ZonePainter = zones;
            growth.Runner = runner;
            growth.Treasury = treasury;
            growth.TicksPerGrowth = 1;
            growth.Definitions.Add(def);
        }

        // Тикер и контроллеры сохранения (issue #127).
        private void BuildTickerAndSave(GridVisualizer grid)
        {
            // OnEnable тикера требует Growth: добавляем на неактивном объекте и включаем после привязки.
            bool rootWasActive = root.activeSelf;
            root.SetActive(false);
            SettlementGrowthTicker ticker = root.AddComponent<SettlementGrowthTicker>();
            ticker.Growth = growth;
            SettlementDemolitionController demolition = root.AddComponent<SettlementDemolitionController>();
            demolition.GridVisualizer = grid;
            demolition.Growth = growth;
            save = root.AddComponent<SaveController>();
            load = root.AddComponent<LoadController>();
            root.SetActive(rootWasActive);
        }
    }
}
