using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса GarbageFleetTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class GarbageFleetTests
    {
        private static GarbageSystem Run(List<BuildingState> homes, int ticks, GridPosition? depot,
            Func<GridPosition, GridPosition, List<GridPosition>> pathFinder)
        {
            GarbageSystem garbage = new GarbageSystem();
            for (int t = 0; t < ticks; t++) garbage.Tick(t, homes, depot, pathFinder);
            return garbage;
        }
    }
}
