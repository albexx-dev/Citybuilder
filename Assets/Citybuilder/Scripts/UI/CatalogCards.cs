using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Лента карточек каталога (задача 173): имя сверху, цветная плитка категории в центре, цена снизу.
    // Прокрутка — колесо мыши, ЛКМ-перетаскивание и стрелки через CatalogScroll.
    public static class CatalogCards
    {
        private const string CardsGuard = "catalog_cards";
        private const int CardsDragId = 1;
        private static float scroll;
        private static readonly GUIStyle CenterLabel = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12
        };

        // Рисует карточки списка над нижней панелью. Пустой список снимает гард и ничего не рисует.
        public static void Draw(float sw, float sh, List<BuildingDefinition> cards, BuildingPlacementController placement, SettlementGrowthController growth)
        {
            if (cards == null || cards.Count == 0)
            {
                UiClickGuard.Set(CardsGuard, new Rect());
                return;
            }
            Rect strip = HudLayout.CatalogCardsRect(sw, sh, cards.Count);
            UiClickGuard.Set(CardsGuard, strip);
            BottomBarStyle.DrawPopupBackground(strip);
            float content = cards.Count * HudLayout.CatalogCardWidth + (cards.Count - 1) * HudLayout.ButtonGap;
            Rect view = CatalogScroll.Layout(strip, content, CardsDragId, ref scroll);
            int grown = growth != null ? growth.GrownCount : int.MaxValue;
            GUI.BeginGroup(view);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == null) continue;
                float x = i * (HudLayout.CatalogCardWidth + HudLayout.ButtonGap) - scroll;
                DrawCard(new Rect(x, 0f, HudLayout.CatalogCardWidth, view.height), cards[i], placement, grown);
            }
            GUI.EndGroup();
        }

        // Сброс ленты при смене вкладки каталога.
        public static void ResetScroll()
        {
            scroll = 0f;
        }

        // Очистка гарда кликов ленты: каталог виден только в режиме «Дома» (BuildCatalogView.ResetGuards).
        public static void ResetGuard()
        {
            UiClickGuard.Set(CardsGuard, new Rect());
        }

        // Карточка: фон-кнопка на весь размер, поверх — имя, плитка и цена; заблокированная — серая.
        private static void DrawCard(Rect card, BuildingDefinition def, BuildingPlacementController placement, int grown)
        {
            if (!UnlockPolicy.IsUnlocked(def, grown))
            {
                BottomBarStyle.DrawDisabledButton(card, def.DisplayName + "\n— " + UnlockPolicy.Remaining(def, grown) + " зд.");
                return;
            }
            bool selected = placement != null && placement.Definition == def;
            if (BottomBarStyle.DrawButton(card, string.Empty, selected) && placement != null) placement.SetDefinition(def);
            GUI.Label(NameRect(card), def.DisplayName, CenterLabel);
            DrawTile(card, def.Category);
            GUI.Label(PriceRect(card), "$" + def.ConstructionCost, CenterLabel);
        }

        private static void DrawTile(Rect card, BuildingCategory category)
        {
            Color saved = GUI.color;
            GUI.color = CategoryColor(category);
            GUI.DrawTexture(TileRect(card), Texture2D.whiteTexture);
            GUI.color = saved;
        }

        // Цвет плитки категории — визуальный якорь вкладки и карточек (иконки появятся в задаче 66).
        public static Color CategoryColor(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Roads: return new Color(0.36f, 0.38f, 0.44f);
                case BuildingCategory.Residential: return new Color(0.45f, 0.72f, 0.35f);
                case BuildingCategory.Commercial: return new Color(0.95f, 0.62f, 0.25f);
                case BuildingCategory.Service: return new Color(0.35f, 0.62f, 0.85f);
                case BuildingCategory.Transport: return new Color(0.70f, 0.45f, 0.30f);
                default: return new Color(0.30f, 0.55f, 0.30f);
            }
        }

        private static Rect NameRect(Rect card)
        {
            return new Rect(card.x + 4f, card.y + 4f, card.width - 8f, 18f);
        }

        private static Rect TileRect(Rect card)
        {
            return new Rect(card.x + 8f, card.y + 26f, card.width - 16f, card.height - 54f);
        }

        private static Rect PriceRect(Rect card)
        {
            return new Rect(card.x + 4f, card.yMax - 22f, card.width - 8f, 18f);
        }
    }
}
