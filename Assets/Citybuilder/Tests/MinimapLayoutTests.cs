using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Геометрия окна мини-карты и телепорт камеры по клику (EPIC-67 шаг 1).
    public class MinimapLayoutTests
    {
        [TestCase(800f, 600f)]
        [TestCase(400f, 300f)]
        [TestCase(1280f, 720f)]
        [TestCase(1920f, 1080f)]
        public void MinimapRect_Is140SquareInTopRight(float width, float height)
        {
            Rect minimap = HudLayout.MinimapRect(width, height);
            Assert.AreEqual(140f, minimap.width, 0.01f);
            Assert.AreEqual(140f, minimap.height, 0.01f);
            Assert.AreEqual(HudLayout.BarPadding, minimap.y, 0.01f);
            Assert.AreEqual(width - 140f - HudLayout.BarPadding, minimap.x, 0.01f);
            Assert.LessOrEqual(minimap.xMax, width + 0.01f);
            Assert.LessOrEqual(minimap.yMax, height + 0.01f);
        }

        [TestCase(200f, 160f)]
        [TestCase(120f, 100f)]
        [TestCase(60f, 50f)]
        public void MinimapRect_NarrowOrLowWindow_ShrinksAndStaysInsideScreen(float width, float height)
        {
            Rect minimap = HudLayout.MinimapRect(width, height);
            Assert.Less(minimap.width, 140f + 0.01f);
            Assert.AreEqual(minimap.width, minimap.height, 0.001f);
            Assert.GreaterOrEqual(minimap.x, 0f);
            Assert.GreaterOrEqual(minimap.y, 0f);
            Assert.LessOrEqual(minimap.xMax, width + 0.01f);
            Assert.LessOrEqual(minimap.yMax, height + 0.01f);
        }

        [TestCase(800f, 600f)]
        [TestCase(400f, 300f)]
        public void MinimapRect_StaysAboveBottomBar(float width, float height)
        {
            Rect minimap = HudLayout.MinimapRect(width, height);
            Rect bar = HudLayout.BottomBarRect(width, height);
            Assert.LessOrEqual(minimap.yMax, bar.y);
        }

        // Правый верх общий с инфоменю: пересечение — норма, поэтому и нужен предикат видимости ниже.
        [Test]
        public void MinimapRect_OverlapsInfoMenu_BecauseTopRightIsShared()
        {
            Assert.IsTrue(HudLayout.InfoMenuRect(800f, 600f).Overlaps(HudLayout.MinimapRect(800f, 600f)));
        }

        [TestCase(false, false, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, true, false)]
        public void IsMinimapVisible_HiddenWhenInfoMenuOrEventLogOpen(bool infoMenuOpen, bool eventLogOpen, bool expected)
        {
            Assert.AreEqual(expected, HudLayout.IsMinimapVisible(infoMenuOpen, eventLogOpen));
        }

        [TestCase(false, false, false, false, true)]
        [TestCase(false, false, true, false, false)]
        [TestCase(false, false, false, true, false)]
        [TestCase(true, true, true, true, false)]
        [TestCase(false, true, false, false, false)]
        public void IsMinimapVisible_QuestsOrSettingsOpen_HidesWindow(bool infoMenuOpen, bool eventLogOpen, bool questsOpen, bool settingsOpen, bool expected)
        {
            Assert.AreEqual(expected, MinimapMapping.IsMinimapVisible(infoMenuOpen, eventLogOpen, questsOpen, settingsOpen));
            Assert.AreEqual(!infoMenuOpen && !eventLogOpen, MinimapMapping.IsMinimapVisible(infoMenuOpen, eventLogOpen));
        }

        [Test]
        public void FocusOn_MovesCameraOverWorldPoint_AndKeepsHeightAndYaw()
        {
            GameObject go = new GameObject("MinimapCameraTest");
            try
            {
                Camera cam = go.AddComponent<Camera>();
                FreeCameraController ctrl = go.AddComponent<FreeCameraController>();
                ctrl.Target = cam;
                ctrl.MapWidth = 200f;
                ctrl.MapHeight = 200f;
                cam.transform.position = new Vector3(3f, 42f, 7f);
                cam.transform.rotation = Quaternion.Euler(35f, 120f, 0f);
                Quaternion before = cam.transform.rotation;

                ctrl.FocusOn(61f, 17.5f);

                Assert.AreEqual(61f, cam.transform.position.x, 0.01f);
                Assert.AreEqual(17.5f, cam.transform.position.z, 0.01f);
                Assert.AreEqual(42f, cam.transform.position.y, 0.01f);
                Assert.Less(Quaternion.Angle(before, cam.transform.rotation), 0.01f);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void FocusOn_PointBeyondMap_ClampsToMapBounds()
        {
            GameObject go = new GameObject("MinimapCameraClampTest");
            try
            {
                Camera cam = go.AddComponent<Camera>();
                FreeCameraController ctrl = go.AddComponent<FreeCameraController>();
                ctrl.Target = cam;
                ctrl.MapWidth = 100f;
                ctrl.MapHeight = 100f;
                ctrl.Margin = 10f;
                cam.transform.position = new Vector3(0f, 30f, 0f);

                ctrl.FocusOn(500f, -300f);

                Assert.AreEqual(110f, cam.transform.position.x, 0.01f);
                Assert.AreEqual(-10f, cam.transform.position.z, 0.01f);
            }
            finally { Object.DestroyImmediate(go); }
        }

        // Весь путь клика: клетка → мир (MinimapMapping) → камера над этой же клеткой реальной сетки.
        [TestCase(0, 0)]
        [TestCase(12, 30)]
        [TestCase(49, 49)]
        public void FocusOn_CellWorldPoint_CameraSitsAboveThatCell(int cellX, int cellY)
        {
            GridSystem grid = new GridSystem(new GridSize(50, 50), 2f);
            GameObject go = new GameObject("MinimapCameraCellTest");
            try
            {
                Camera cam = go.AddComponent<Camera>();
                FreeCameraController ctrl = go.AddComponent<FreeCameraController>();
                ctrl.Target = cam;
                ctrl.MapWidth = 100f;
                ctrl.MapHeight = 100f;
                float worldX, worldZ;
                MinimapMapping.CellToWorld(cellX, cellY, 2f, out worldX, out worldZ);

                ctrl.FocusOn(worldX, worldZ);

                Assert.AreEqual(cellX, grid.WorldToGrid(cam.transform.position.x, cam.transform.position.z).X);
                Assert.AreEqual(cellY, grid.WorldToGrid(cam.transform.position.x, cam.transform.position.z).Y);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
