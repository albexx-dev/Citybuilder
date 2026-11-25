using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Rect-тесты нижней панели (EPIC-13): группы не пересекаются в диапазоне 1024..2560 x 600..1440.
    public partial class BottomBarLayoutTests
    {
        [TestCase(1024f, 600f)]
        [TestCase(1280f, 600f)]
        [TestCase(1280f, 700f)]
        [TestCase(1280f, 800f)]
        [TestCase(1920f, 1080f)]
        [TestCase(2560f, 1440f)]
        public void Groups_DoNotOverlap_AndStayInsideBar(float width, float height)
        {
            Rect bar = HudLayout.BottomBarRect(width, height);
            Rect speed = HudLayout.SpeedGroupRect(width, height);
            Rect mode = HudLayout.ModeGroupRect(width, height);
            Rect menu = SettingsLayout.MenuButtonsRect(width, height);

            Assert.AreEqual(width, bar.width, 0.01f);
            Assert.AreEqual(height - HudLayout.BarHeight, bar.y, 0.01f);
            Assert.Less(speed.xMax, mode.x);
            Assert.Less(mode.xMax, menu.x);
            Assert.LessOrEqual(menu.xMax, bar.xMax);
            Assert.IsFalse(speed.Overlaps(mode));
            Assert.IsFalse(mode.Overlaps(menu));
        }
        [Test]
        public void ModeGroupRect_FitsSixModeButtons_AtMinimumWidth()
        {
            Rect mode = HudLayout.ModeGroupRect(1024f, 600f);
            Assert.AreEqual(6f * HudLayout.ModeButtonWidth + 5f * HudLayout.ButtonGap, mode.width, 0.01f);
            Assert.LessOrEqual(mode.xMax, 1024f - HudLayout.BarPadding);
            Assert.LessOrEqual(mode.xMax, SettingsLayout.MenuButtonsRect(1024f, 600f).x);
        }
        [TestCase(800f, 420f)]
        [TestCase(1100f, 420f)]
        public void ModeLabels_FitOneLine_AtNarrowWidth(float width, float height)
        {
            float textWidth = BottomBarStyle.LabelWidth(HudLayout.ModeButtonWidth, HudLayout.ButtonHeight);
            string[] labels = { "Выкл [Esc]", "Здания [1]", "Дороги [2]", "Зоны [3]", "Рост [4]", "Декор [5]",
                "Off [Esc]", "Buildings [1]", "Roads [2]", "Zones [3]", "Growth [4]", "Decor [5]" };
            for (int i = 0; i < labels.Length; i++)
                Assert.LessOrEqual(BottomBarStyle.EstimateLabelWidth(labels[i], BottomBarStyle.ButtonFontSize), textWidth, labels[i]);
            Assert.LessOrEqual(SettingsLayout.MenuButtonRect(width, height, 0).xMax, width);
        }
        [TestCase(1024f, 600f)]
        [TestCase(1920f, 1080f)]
        public void PalettePopup_StaysAboveBottomBar(float width, float height)
        {
            Rect bar = HudLayout.BottomBarRect(width, height);
            Rect popup = HudLayout.PaletteRect(width, height, 6);
            Assert.Greater(popup.width, 0f);
            Assert.LessOrEqual(popup.yMax, bar.y);
            Assert.IsFalse(popup.Overlaps(bar));
        }

        [Test]
        public void PalettePopup_ZeroItems_ReturnsEmptyRect()
        {
            Rect popup = HudLayout.PaletteRect(1920f, 1080f, 0);
            Assert.AreEqual(0f, popup.width);
        }

        [TestCase(1024f, 600f, 8)]
        [TestCase(1024f, 600f, 16)]
        [TestCase(1280f, 720f, 12)]
        [TestCase(1920f, 1080f, 20)]
        public void PalettePopup_ManyItems_ClampsToScreenWidth(float width, float height, int count)
        {
            Rect bar = HudLayout.BottomBarRect(width, height);
            Rect popup = HudLayout.PaletteRect(width, height, count);
            Assert.Greater(popup.width, 0f);
            Assert.LessOrEqual(popup.xMax, width - HudLayout.BarPadding + 0.01f);
            Assert.LessOrEqual(popup.yMax, bar.y);
            Assert.IsFalse(popup.Overlaps(bar));
        }

        [TestCase(1024f, 200f)]
        [TestCase(1024f, 240f)]
        [TestCase(1024f, 600f)]
        [TestCase(1920f, 1080f)]
        public void InfoPanel_DoesNotOverlapPaletteOrBottomBar(float width, float height)
        {
            Rect bar = HudLayout.BottomBarRect(width, height);
            Rect popup = HudLayout.PaletteRect(width, height, 6);
            Rect panel = HudLayout.InfoPanelRect(width, height);
            Assert.LessOrEqual(panel.xMax, width);
            Assert.IsFalse(panel.Overlaps(bar));
            Assert.IsFalse(panel.Overlaps(popup));
            if (panel.height > 0f) Assert.LessOrEqual(panel.yMax, popup.y);
        }
        [TestCase(600f)]
        [TestCase(700f)]
        [TestCase(800f)]
        [TestCase(1080f)]
        public void BottomHudRows_DoNotOverlapBottomBar(float height)
        {
            Rect bar = HudLayout.BottomBarRect(1280f, height);
            float y0 = HudLayout.FixedOrBottom(454f, 0, height);
            float y1 = HudLayout.FixedOrBottom(430f, 1, height);
            float y2 = HudLayout.FixedOrBottom(406f, 2, height);
            Assert.Less(y2, y1);
            Assert.Less(y1, y0);
            Assert.LessOrEqual(y0 + 24f, bar.y);
        }

        [Test]
        public void LowWindow_HidesCrushedRows_WhileKeepingBottomBarVisible()
        {
            Assert.IsFalse(HudLayout.IsHudRowVisible(454f, 0, 140f));
            Assert.IsTrue(HudLayout.IsHudRowVisible(454f, 0, 600f));
            Assert.IsTrue(HudLayout.IsHudRowVisible(406f, 2, 600f));
            Assert.AreEqual(HudLayout.BarHeight, HudLayout.BottomBarRect(1024f, 440f).height, 0.01f);
        }

        [Test]
        public void RebuildRadius_WhenOccupiedOverlayExists_FindsAndAssignsRadiusRenderer()
        {
            GameObject go = new GameObject("GridTest");
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                GridMeshBuilder builder = go.AddComponent<GridMeshBuilder>();
                builder.Visualizer = viz;
                viz.Builder = builder;
                builder.RebuildOccupied();
                Assert.IsNotNull(go.transform.Find("OccupiedOverlay"));

                List<GridPosition> cells = new List<GridPosition> { new GridPosition(1, 1) };
                builder.RebuildRadius(cells, Color.cyan);
                Transform radiusChild = go.transform.Find("RadiusOverlay");
                Assert.IsNotNull(radiusChild);
                Assert.IsNotNull(builder.RadiusRenderer);
                Assert.AreEqual(Color.cyan, builder.RadiusRenderer.sharedMaterial.color);

                builder.RebuildRadius(cells, Color.magenta);
                Assert.AreEqual(Color.magenta, builder.RadiusRenderer.sharedMaterial.color);
            }
            finally { Object.DestroyImmediate(go); }
        }

    }
}
