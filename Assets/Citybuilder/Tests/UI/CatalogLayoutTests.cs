using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты каталога постройки (задача 173): геометрия ленты и вкладок, скролл-математика и фильтр категорий.
    public class CatalogLayoutTests
    {
        [TestCase(1024f, 600f)]
        [TestCase(1280f, 720f)]
        [TestCase(1920f, 1080f)]
        public void CatalogRects_StayInsideScreen_AboveBottomBar(float width, float height)
        {
            Rect bar = HudLayout.BottomBarRect(width, height);
            Rect tabs = HudLayout.CatalogTabsRect(width, height);
            Rect cards = HudLayout.CatalogCardsRect(width, height, 7);

            Assert.LessOrEqual(tabs.xMax, width - HudLayout.BarPadding + 0.01f);
            Assert.LessOrEqual(cards.xMax, width - HudLayout.BarPadding + 0.01f);
            Assert.LessOrEqual(cards.yMax, bar.y);
            Assert.IsFalse(cards.Overlaps(bar));
            Assert.LessOrEqual(tabs.yMax, cards.y);
            Assert.IsFalse(tabs.Overlaps(cards));
        }

        [TestCase(1024f, 600f, 1)]
        [TestCase(1024f, 600f, 7)]
        [TestCase(1280f, 720f, 12)]
        [TestCase(1920f, 1080f, 20)]
        public void CardsStrip_ManyItems_NeverWiderThanScreen(float width, float height, int count)
        {
            Rect cards = HudLayout.CatalogCardsRect(width, height, count);
            Assert.Greater(cards.width, 0f);
            Assert.LessOrEqual(cards.xMax, width - HudLayout.BarPadding + 0.01f);
            Assert.AreEqual(HudLayout.CatalogCardHeight, cards.height, 0.01f);
        }

        [Test]
        public void CardsStrip_ZeroItems_ReturnsEmptyRect()
        {
            Assert.AreEqual(0f, HudLayout.CatalogCardsRect(1920f, 1080f, 0).width);
        }

        [Test]
        public void ClampScroll_BoundsOffsetWithinContent()
        {
            Assert.AreEqual(0f, CatalogScroll.ClampScroll(-5f, 800f, 400f));
            Assert.AreEqual(0f, CatalogScroll.ClampScroll(10f, 300f, 400f));
            Assert.AreEqual(400f, CatalogScroll.ClampScroll(900f, 800f, 400f));
            Assert.AreEqual(123f, CatalogScroll.ClampScroll(123f, 800f, 400f));
        }

        [Test]
        public void Viewport_InsetsArrowZones_OnlyWhenContentOverflows()
        {
            Rect strip = new Rect(100f, 200f, 600f, 96f);
            Rect fitting = CatalogScroll.Viewport(strip, 500f);
            Assert.AreEqual(strip, fitting);
            Rect overflow = CatalogScroll.Viewport(strip, 1200f);
            Assert.Greater(strip.width, overflow.width);
            Assert.AreEqual(strip.y, overflow.y, 0.01f);
        }

        [Test]
        public void FilterByCategory_ReturnsOnlyRequestedCategory()
        {
            List<BuildingDefinition> defs = new List<BuildingDefinition> { Create(BuildingCategory.Roads), Create(BuildingCategory.Service), null, Create(BuildingCategory.Service) };
            List<BuildingDefinition> results = new List<BuildingDefinition>();
            try
            {
                BuildCatalogView.FilterByCategory(defs, BuildingCategory.Service, results);
                Assert.AreEqual(2, results.Count);
                Assert.AreEqual(BuildingCategory.Service, results[0].Category);
                Assert.AreEqual(BuildingCategory.Service, results[1].Category);
                BuildCatalogView.FilterByCategory(defs, BuildingCategory.Roads, results);
                Assert.AreEqual(1, results.Count);
                BuildCatalogView.FilterByCategory(null, BuildingCategory.Roads, results);
                Assert.AreEqual(0, results.Count);
            }
            finally
            {
                for (int i = 0; i < defs.Count; i++) { if (defs[i] != null) Object.DestroyImmediate(defs[i]); }
            }
        }

        private static BuildingDefinition Create(BuildingCategory category)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Category = category;
            return def;
        }
    }
}
