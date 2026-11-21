using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Задержка тултипов (хвост #66): подсказка звенит только когда курсор на кнопке
    // >= HoverDelaySeconds. Чистая логика без IMGUI: состояние наведения лежит в словаре
    // по кнопке (ключ — Rect, значения считаются одинаково каждый кадр), реальное время
    // (Time.unscaledTime) вызывающий передаёт параметром — EditMode- и dotnet-тесты.
    public static class TooltipHoverPolicy
    {
        public const float HoverDelaySeconds = 0.4f;

        // Тултип показан, если наведение длится достаточно; старт < 0 — курсора нет.
        public static bool TooltipShown(float hoverStartSeconds, float nowSeconds)
        {
            return hoverStartSeconds >= 0f && nowSeconds - hoverStartSeconds >= HoverDelaySeconds;
        }

        // Обновить наведение и решить, показывать ли тултип: курсор ушёл — запись сбрасывается,
        // возврат на кнопку начинает задержку заново. Кнопки без курсора не трекаются.
        public static bool Track(Dictionary<Rect, float> hoverStarts, Rect rect, Vector2 mouse, float nowSeconds)
        {
            if (hoverStarts == null) return false;
            if (!rect.Contains(mouse))
            {
                hoverStarts.Remove(rect);
                return false;
            }
            float start;
            if (!hoverStarts.TryGetValue(rect, out start)) start = nowSeconds;
            hoverStarts[rect] = start;
            return TooltipShown(start, nowSeconds);
        }
    }
}
