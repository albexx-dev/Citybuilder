using System;

namespace Citybuilder
{
    // Размер карты в клетках. Начало координат (0, 0).
    // Допустимые позиции: от (0, 0) до (Width - 1, Height - 1).
    public struct GridSize : IEquatable<GridSize>
    {
        public readonly int Width;
        public readonly int Height;

        public GridSize(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException("width", "Width must be positive.");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("height", "Height must be positive.");
            }
            Width = width;
            Height = height;
        }

        // Проверка границ карты.
        public bool Contains(GridPosition position)
        {
            return position.X >= 0
                && position.Y >= 0
                && position.X < Width
                && position.Y < Height;
        }

        public bool Equals(GridSize other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is GridSize other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Width * 397 ^ Height;
        }

        public override string ToString()
        {
            return Width + "x" + Height;
        }
    }
}
