using System;

namespace Citybuilder
{
    // Снимок домохозяйства: в каком доме живёт и сколько жителей. Чистый C#, без Unity.
    public class HouseholdState
    {
        public EntityId Id { get; private set; }
        public BuildingState Home { get; private set; }
        public int Size { get; private set; }

        public HouseholdState(EntityId id, BuildingState home, int size)
        {
            if (home == null)
            {
                throw new ArgumentNullException("home");
            }
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            Id = id;
            Home = home;
            Size = size;
        }
    }
}
