using System;

namespace Citybuilder
{
    // Игровое время в тиках. V1: 1 тик = 1 игровой час, сутки = 24 тика.
    // Неизменяемый тип: Advance возвращает новое значение.
    public struct SimulationTime : IEquatable<SimulationTime>, IComparable<SimulationTime>
    {
        public const int TicksPerDay = 24;

        public readonly long TickCount;

        public SimulationTime(long tickCount)
        {
            if (tickCount < 0)
            {
                throw new ArgumentOutOfRangeException("tickCount", "TickCount cannot be negative.");
            }
            TickCount = tickCount;
        }

        public int DayNumber
        {
            get { return (int)(TickCount / TicksPerDay); }
        }

        public int HourOfDay
        {
            get { return (int)(TickCount % TicksPerDay); }
        }

        public SimulationTime Advance(int steps)
        {
            if (steps < 0)
            {
                throw new ArgumentOutOfRangeException("steps", "Steps cannot be negative.");
            }
            return new SimulationTime(TickCount + steps);
        }

        public bool Equals(SimulationTime other)
        {
            return TickCount == other.TickCount;
        }

        public int CompareTo(SimulationTime other)
        {
            return TickCount.CompareTo(other.TickCount);
        }

        public override bool Equals(object obj)
        {
            return obj is SimulationTime other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TickCount.GetHashCode();
        }

        public override string ToString()
        {
            return "Day " + DayNumber + ", Hour " + HourOfDay;
        }
    }
}
