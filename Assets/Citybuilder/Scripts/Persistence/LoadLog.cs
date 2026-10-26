using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Строка лога загрузки слота. Вынесена из LoadController, чтобы тот остался в лимите 150.
    public static class LoadLog
    {
        public static void Report(string path, CitySnapshot snapshot, List<BuildingState> homes, int manual)
        {
            int restored = homes == null ? 0 : homes.Count;
            Debug.Log("LoadController: слот загружен: домов=" + restored + ", ручных=" + manual
                + ", домохозяйств=" + snapshot.Households.Count + ", зон=" + snapshot.Zones.Count
                + ", дорог=" + snapshot.Roads.Count + ", баланс=" + snapshot.Balance
                + ", налог=" + snapshot.TaxLevel + ", цель=" + snapshot.GoalStage + ", тик=" + snapshot.TickCount + ", файл=" + path);
        }
    }
}
