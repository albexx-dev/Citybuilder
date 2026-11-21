using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Окно мини-карты (EPIC-67 шаг 2): статический IMGUI-рисовальщик, вызывается из BottomBarView.OnGUI,
    // поэтому новый компонент в сцену не нужен и сцена не правится.
    // Рисует MinimapTexture.Current в HudLayout.MinimapRect (правый верх), регистрирует rect в UiClickGuard
    // (стройка и вращение камеры под окном гасятся) и ловит ЛКМ: точка rect'а → пиксель → клетка → FocusOn.
    // Ориентация: v=0 текстуры рисуется НИЖНЕЙ строкой rect'а, GUI-y растёт вниз — инверсию делает RectToPixel.
    public static class MinimapView
    {
        private const string GuardKey = "minimap";
        private const string TooltipKey = "minimap_tip";

        private static GridVisualizer visualizer;
        private static ZonePaintingController painter;
        private static RoadNetworkStore roads;
        private static SettlementGrowthController growth;
        private static BuildingPlacementController placement;
        private static FreeCameraController camera;

        // Игровой путь из OnGUI: видимость (в правом верху сидят инфоменю и лента), ленивые ссылки, клик.
        public static void Draw(bool infoMenuOpen, bool eventLogOpen)
        {
            Draw(infoMenuOpen, eventLogOpen, false, false);
        }

        // Полный набор флагов из BottomBarView (issue #98): задачи и настройки тоже прячут окно и снимают гард.
        public static void Draw(bool infoMenuOpen, bool eventLogOpen, bool questsOpen, bool settingsOpen)
        {
            if (!MinimapMapping.IsMinimapVisible(infoMenuOpen, eventLogOpen, questsOpen, settingsOpen)) { Hide(); return; }
            EnsureRefs();
            GridSystem grid = visualizer != null ? visualizer.Grid : null;
            if (grid == null || camera == null) { Hide(); return; }
            MinimapTexture.RefreshIfDue(grid, ZoneSystemOrNull(),
                roads != null ? roads.Roads : null,
                growth != null ? growth.Homes : null,
                placement != null ? placement.Placed : null);
            Show(HudLayout.MinimapRect(Screen.width, Screen.height), grid, camera, Event.current);
        }

        // Тестируемый шов (headless без сцены): данные и клик приходят параметрами, ленивый поиск не участвует.
        public static void Draw(Rect rect, GridSystem grid, ZoneSystem zones, List<RoadState> roadStates,
            List<BuildingState> homes, List<BuildingState> placed, FreeCameraController target, Event click)
        {
            if (grid == null) { Hide(); return; }
            MinimapTexture.RefreshIfDue(grid, zones, roadStates, homes, placed);
            Show(rect, grid, target, click);
        }

        // Общая часть: текстура + гард + клик. Без схемы (нет данных) окно не рисуется и гард пуст.
        private static void Show(Rect rect, GridSystem grid, FreeCameraController target, Event click)
        {
            Texture2D map = MinimapTexture.Current;
            if (map == null) { Hide(); return; }
            GUI.DrawTexture(rect, map);
            BottomBarStyle.DrawTooltip(rect, Localization.T(TooltipKey));
            ProcessClick(click, rect, grid, target);
        }

        // Тестируемый шов без IMGUI (GUI доступен только в OnGUI): гард + клик, без отрисовки.
        public static void ProcessClick(Event click, Rect rect, GridSystem grid, FreeCameraController target)
        {
            UiClickGuard.Set(GuardKey, rect);
            HandleClick(click, rect, grid, target);
        }

        private static void HandleClick(Event click, Rect rect, GridSystem grid, FreeCameraController target)
        {
            if (click == null || target == null) return;
            if (click.type != EventType.MouseDown || click.button != 0) return;
            if (!rect.Contains(click.mousePosition)) return;
            float pixelX, pixelY, worldX, worldZ;
            MinimapMapping.RectToPixel(rect, click.mousePosition.x, click.mousePosition.y,
                MinimapTexture.Size, out pixelX, out pixelY);
            int cellX, cellY, unused;
            MinimapMapping.PixelToCell(pixelX, 0f, MinimapTexture.Size, grid.Size.Width, out cellX, out unused);
            MinimapMapping.PixelToCell(0f, pixelY, MinimapTexture.Size, grid.Size.Height, out unused, out cellY);
            MinimapMapping.CellToWorld(cellX, cellY, grid.CellSize, out worldX, out worldZ);
            target.FocusOn(worldX, worldZ);
            click.Use();
        }

        // Окно скрыто: гард обнуляется, иначе прошлый rect навсегда закрыл бы клики по миру.
        public static void Hide()
        {
            UiClickGuard.Set(GuardKey, new Rect());
        }

        // Zones лениво пересоздаёт систему и ругается в консоль без своей сетки — спрашиваем только готовую кисть.
        private static ZoneSystem ZoneSystemOrNull()
        {
            return painter != null && painter.GridVisualizer != null ? painter.Zones : null;
        }

        // Ленивый поиск один раз: пока сетка не найдена, ищем все ссылки снова (прецедент TaxPolicyView.cs:42).
        private static void EnsureRefs()
        {
            if (visualizer != null) return;
            visualizer = Object.FindFirstObjectByType<GridVisualizer>();
            painter = Object.FindFirstObjectByType<ZonePaintingController>();
            roads = Object.FindFirstObjectByType<RoadNetworkStore>();
            growth = Object.FindFirstObjectByType<SettlementGrowthController>();
            placement = Object.FindFirstObjectByType<BuildingPlacementController>();
            camera = Object.FindFirstObjectByType<FreeCameraController>();
        }
    }
}
