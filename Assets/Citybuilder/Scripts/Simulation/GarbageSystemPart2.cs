using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Хвост класса GarbageSystem (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class GarbageSystem
    {
        // Прирост за тик: WasteOutput здания плюс младший бит хеша — детерминированный джиттер.
        public static int GainPerTick(int tick, EntityId id, int wasteOutput)
        {
            int baseOutput = wasteOutput < 0 ? 0 : wasteOutput;
            return baseOutput + (int)(Hash(tick, id) & 1u);
        }

        private static uint MixByte(uint hash, byte value)
        {
            return (hash ^ value) * 16777619u;
        }
    }
}
