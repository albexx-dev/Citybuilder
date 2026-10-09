using System;

namespace Citybuilder
{
    // Уникальный номер экземпляра в текущем городе.
    // Выдаётся через New(). Счётчик сбрасывается при перезапуске — нормально для V1.
    public struct EntityId : IEquatable<EntityId>
    {
        private static int nextValue = 1;

        public readonly int Value;

        private EntityId(int value)
        {
            Value = value;
        }

        public static EntityId New()
        {
            EntityId id = new EntityId(nextValue);
            nextValue = nextValue + 1;
            return id;
        }

        public bool IsValid
        {
            get { return Value > 0; }
        }

        public bool Equals(EntityId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return "Entity#" + Value;
        }
    }
}
