using System;
namespace Citybuilder
{
    // Ключ клетки дороги с уровнем: GridPosition остаётся без высоты (ADR-018/022).
    public struct RoadCellKey : IEquatable<RoadCellKey>
    {
        public const int StreetLevel = 1;
        public const int BridgeLevel = 2;
        public readonly int X;
        public readonly int Y;
        public readonly int Level;
        public RoadCellKey(int x, int y, int level = 1) { X = x; Y = y; Level = level; }
        public GridPosition Position { get { return new GridPosition(X, Y); } }
        public bool Equals(RoadCellKey other) { return X == other.X && Y == other.Y && Level == other.Level; }
        public override bool Equals(object obj) { return obj is RoadCellKey other && Equals(other); }
        public override int GetHashCode() { return X * 397 ^ Y * 31 ^ Level; }
        public static bool operator ==(RoadCellKey a, RoadCellKey b) { return a.Equals(b); }
        public static bool operator !=(RoadCellKey a, RoadCellKey b) { return !a.Equals(b); }
    }
}
