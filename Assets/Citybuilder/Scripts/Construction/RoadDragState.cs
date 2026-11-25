using System.Collections.Generic;

namespace Citybuilder
{
    // Состояние drag-стройки дорог (задача 183): превью отрезка и переходы drag-цикла.
    // Чистый C# без Unity: ввод передаётся параметрами, мышь остаётся в RoadPlacementController.
    // Геометрия отрезка — в RoadPlacementInput.
    public class RoadDragState
    {
        private readonly List<GridPosition> previewCells = new List<GridPosition>();
        private GridPosition dragStart;
        private GridPosition lastHover;

        public bool Dragging { get; private set; }
        public List<GridPosition> PreviewCells { get { return previewCells; } }

        // Старт drag по ЛКМ: фиксирует начало отрезка и одноклеточное превью.
        public bool TryBegin(bool clickPressed, bool hovered, bool overMenu, GridPosition cell)
        {
            if (!clickPressed || !hovered || overMenu) return false;
            Dragging = true;
            dragStart = cell;
            lastHover = cell;
            SetLine(cell, cell);
            return true;
        }

        // Программная трасса (PlaceLine): отрезок from-to без ввода мыши.
        public void BeginLine(GridPosition from, GridPosition to)
        {
            Dragging = true;
            dragStart = from;
            lastHover = to;
            SetLine(from, to);
        }

        // Движение при зажатой ЛКМ: перестраивает превью, если клетка сменилась.
        public bool TryPreview(bool buttonHeld, bool hovered, GridPosition cell)
        {
            if (!Dragging || !buttonHeld || !hovered || cell == lastHover) return false;
            lastHover = cell;
            SetLine(dragStart, cell);
            return true;
        }

        // Релиз ЛКМ при активном drag: пора коммитить.
        public bool ShouldCommit(bool buttonReleased)
        {
            return Dragging && buttonReleased;
        }

        // Завершить drag, сохранив превью до коммита.
        public void Stop()
        {
            Dragging = false;
        }

        // Полный сброс: выключение компонента (в том числе по Esc через арбитр) или отмена коммита.
        public void Reset()
        {
            Dragging = false;
            previewCells.Clear();
        }

        private void SetLine(GridPosition from, GridPosition to)
        {
            previewCells.Clear();
            List<GridPosition> line = RoadPlacementInput.GetLineCells(from, to);
            for (int i = 0; i < line.Count; i++) previewCells.Add(line[i]);
        }
    }
}
