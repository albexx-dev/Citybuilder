using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Каталог постройки (задача 173): вкладки категорий и лента карточек над нижней панелью.
    // Статический, как DecorPaletteRow; карточки рисует CatalogCards, скролл-ввод — CatalogScroll.
    public static class BuildCatalogView
    {
        private const float TabWidth = 96f;
        private const int TabsDragId = 0;
        private const string TabsGuard = "catalog_tabs";
        // Ярлыки вкладок по значению BuildingCategory (текстовые, иконки — задача 66).
        private static readonly string[] Labels = { "Дороги", "Дома", "Коммерция", "Службы", "Декор", "Транспорт" };
        private static readonly List<BuildingCategory> tabs = new List<BuildingCategory>();
        private static readonly List<BuildingDefinition> cards = new List<BuildingDefinition>();
        private static BuildingCategory active = BuildingCategory.Residential;
        private static float tabsScroll;

        // Рисует каталог режима «Дома»: вкладки по категориям + карточки активной категории.
        public static void Draw(float sw, float sh, List<BuildingDefinition> defs, BuildingPlacementController placement, SettlementGrowthController growth)
        {
            CollectTabs(defs);
            if (defs == null || defs.Count == 0 || tabs.Count == 0)
            {
                cards.Clear();
                CatalogCards.Draw(sw, sh, cards, placement, growth);
                return;
            }
            if (!tabs.Contains(active)) active = tabs[0];
            DrawTabs(sw, sh);
            FilterByCategory(defs, active, cards);
            CatalogCards.Draw(sw, sh, cards, placement, growth);
        }

        // Границы каталога для гарда кликов. Вызывается каждый OnGUI-кадр перед DrawSubRow:
        // каталог рисуется только в режиме «Дома», в остальных режимах зоны освобождаются.
        public static void ResetGuards()
        {
            UiClickGuard.Set(TabsGuard, new Rect());
            CatalogCards.ResetGuard();
        }

        // Заполняет переиспользуемый список вкладок: только категории со зданиями + заглушка «Декор».
        private static void CollectTabs(List<BuildingDefinition> defs)
        {
            tabs.Clear();
            if (defs == null) return;
            for (int i = 0; i < defs.Count; i++)
            {
                BuildingDefinition def = defs[i];
                if (def == null || tabs.Contains(def.Category)) continue;
                tabs.Add(def.Category);
            }
            if (ShouldShowDecorTab(defs)) tabs.Add(BuildingCategory.Decor);
        }

        private static void DrawTabs(float sw, float sh)
        {
            Rect strip = HudLayout.CatalogTabsRect(sw, sh);
            UiClickGuard.Set(TabsGuard, strip);
            BottomBarStyle.DrawPopupBackground(strip);
            float content = tabs.Count * TabWidth + (tabs.Count - 1) * HudLayout.ButtonGap;
            Rect view = CatalogScroll.Layout(strip, content, TabsDragId, ref tabsScroll);
            GUI.BeginGroup(view);
            for (int i = 0; i < tabs.Count; i++)
            {
                float x = i * (TabWidth + HudLayout.ButtonGap) - tabsScroll;
                Rect rect = new Rect(x, 0f, TabWidth, view.height);
                if (BottomBarStyle.DrawButton(rect, Labels[(int)tabs[i]], tabs[i] == active)) Select(tabs[i]);
            }
            GUI.EndGroup();
        }

        // Смена вкладки сбрасывает ленту: новый список всегда показывается с начала.
        private static void Select(BuildingCategory category)
        {
            if (active == category) return;
            active = category;
            CatalogCards.ResetScroll();
        }

        // Чистый хелпер: карточки выбранной категории в переиспользуемый список, null-элементы пропускаются.
        public static void FilterByCategory(List<BuildingDefinition> defs, BuildingCategory category, List<BuildingDefinition> results)
        {
            results.Clear();
            if (defs == null) return;
            for (int i = 0; i < defs.Count; i++)
            {
                BuildingDefinition def = defs[i];
                if (def != null && def.Category == category) results.Add(def);
            }
        }

        // Есть ли в списке здания категории (null-элементы игнорируются).
        public static bool HasCategory(List<BuildingDefinition> defs, BuildingCategory category)
        {
            if (defs == null) return false;
            for (int i = 0; i < defs.Count; i++)
            {
                if (defs[i] != null && defs[i].Category == category) return true;
            }
            return false;
        }

        // Заглушка под будущий режим постройки декора: вкладка «Декор» видна, даже если зданий пока нет,
        // и ведёт в пустую ленту вместо исчезающей вкладки.
        public static bool ShouldShowDecorTab(List<BuildingDefinition> defs)
        {
            return !HasCategory(defs, BuildingCategory.Decor);
        }
    }
}
