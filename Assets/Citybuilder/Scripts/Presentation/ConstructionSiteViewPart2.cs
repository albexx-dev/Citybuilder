using System;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса ConstructionSiteView (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class ConstructionSiteView : MonoBehaviour
    {
        // Скачок к накопленным тикам v6 без перепроигрывания событий.
        public void SetElapsed(int ticks)
        {
            if (!isConstructing) return;
            ticksElapsed = ConstructionProgress.ClampTicks(ticks);
            if (ticksElapsed > totalTicks) ticksElapsed = totalTicks;
            if (ConstructionProgress.IsDone(ticksElapsed, totalTicks)) Finish();
            else ApplyProgress();
        }
        private void OnDestroy()
        {
            ConstructionSnapshot.Unregister(this);
            UnsubscribeClock();
            scaffold.Clear();
        }
    }
}
