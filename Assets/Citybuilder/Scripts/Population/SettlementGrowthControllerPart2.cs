using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса SettlementGrowthController (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class SettlementGrowthController : MonoBehaviour
    {
        // Снос одного выросшего дома: занятость, вид, список, сетка. Выселение — в Demolition.
        public void DemolishState(BuildingState state)
        {
            if (state == null || settlement == null) return;
            settlement.Demolish(state);
            View.RemoveBuilding(state);
            states.Remove(state);
            GridVisualizer.RefreshOccupied();
            if (Save != null) Save.MarkDirty();
        }
    }
}
