using UnityEngine;

namespace Citybuilder
{
    // Общий скролл-механизм каталога (задача 173): колесо мыши, ЛКМ-перетаскивание и стрелки «‹» «›».
    // Используется и вкладками, и лентой карточек; чистая математика ClampScroll вынесена для тестов.
    public static class CatalogScroll
    {
        private const float ArrowWidth = 26f;
        private const float WheelStep = 48f;
        private const float DragThreshold = 6f;
        private static int dragOwner = -1;
        private static bool dragging;
        private static bool dragConsumed;
        private static float dragStartX;
        private static float dragStartOffset;

        // Смещение контента: 0..contentWidth-viewWidth; при переполнении возвращается 0.
        public static float ClampScroll(float offset, float contentWidth, float viewWidth)
        {
            float max = Mathf.Max(0f, contentWidth - viewWidth);
            return Mathf.Clamp(offset, 0f, max);
        }

        // Готовит полосу к рисованию: стрелки, колесо, перетаскивание. Возвращает видимую часть контента.
        // dragId различает вкладки и ленту: одно перетаскивание двигает только свою полосу.
        public static Rect Layout(Rect strip, float contentWidth, int dragId, ref float offset)
        {
            Rect view = Viewport(strip, contentWidth);
            offset = ClampScroll(offset, contentWidth, view.width);
            HandleWheel(view, contentWidth, dragId, ref offset);
            HandleDrag(view, contentWidth, dragId, ref offset);
            DrawArrows(strip, view, contentWidth, ref offset);
            return view;
        }

        // Видимая часть: полоса без стрелочных зон, если контент шире полосы; иначе вся полоса.
        public static Rect Viewport(Rect strip, float contentWidth)
        {
            bool scrollable = contentWidth > strip.width + 0.01f;
            float inset = scrollable ? ArrowWidth + HudLayout.ButtonGap : 0f;
            return new Rect(strip.x + inset, strip.y, Mathf.Max(0f, strip.width - 2f * inset), strip.height);
        }

        private static void HandleWheel(Rect view, float contentWidth, int dragId, ref float offset)
        {
            Event e = Event.current;
            if (e == null || e.type != EventType.ScrollWheel || !view.Contains(e.mousePosition)) return;
            float delta = Mathf.Abs(e.delta.x) > Mathf.Abs(e.delta.y) ? e.delta.x : e.delta.y;
            offset = ClampScroll(offset + delta * WheelStep, contentWidth, view.width);
            e.Use();
        }

        private static void HandleDrag(Rect view, float contentWidth, int dragId, ref float offset)
        {
            Event e = Event.current;
            if (e == null) return;
            if (e.type == EventType.MouseDown && view.Contains(e.mousePosition))
            {
                dragOwner = dragId;
                dragging = true;
                dragConsumed = false;
                dragStartX = e.mousePosition.x;
                dragStartOffset = offset;
            }
            else if (e.type == EventType.MouseDrag && dragging && dragOwner == dragId)
            {
                // Перетаскивание продолжается за пределами полосы: мышь может уйти на карту.
                float shift = dragStartX - e.mousePosition.x;
                if (Mathf.Abs(shift) > DragThreshold) dragConsumed = true;
                if (dragConsumed)
                {
                    offset = ClampScroll(dragStartOffset + shift, contentWidth, view.width);
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp && dragging && dragOwner == dragId)
            {
                // Потащили — глотаем MouseUp, чтобы карточка под курсором не считалась кликом.
                if (dragConsumed) e.Use();
                dragging = false;
            }
        }

        private static void DrawArrows(Rect strip, Rect view, float contentWidth, ref float offset)
        {
            if (view.width >= strip.width - 0.01f) return;
            float step = Mathf.Max(96f, view.width * 0.5f);
            Rect left = new Rect(strip.x, strip.y, ArrowWidth, strip.height);
            Rect right = new Rect(strip.xMax - ArrowWidth, strip.y, ArrowWidth, strip.height);
            bool saved = GUI.enabled;
            GUI.enabled = offset > 0.01f;
            if (BottomBarStyle.DrawButton(left, "‹", false)) offset = ClampScroll(offset - step, contentWidth, view.width);
            GUI.enabled = offset + 0.01f < Mathf.Max(0f, contentWidth - view.width);
            if (BottomBarStyle.DrawButton(right, "›", false)) offset = ClampScroll(offset + step, contentWidth, view.width);
            GUI.enabled = saved;
        }
    }
}
