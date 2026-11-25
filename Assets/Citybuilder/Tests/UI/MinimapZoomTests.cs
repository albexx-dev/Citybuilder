using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Зум мини-карты (issue #119): границы, UV-окно, маппинг клика, гард и видимость панелей.
    public class MinimapZoomTests
    {
        private const int TextureSize = MinimapTexture.Size;
        private const int Cells = 50;
        private Rect window;
        private GameObject go;
        private Camera camera;
        private FreeCameraController cam;

        [SetUp]
        public void SetUp()
        {
            MinimapView.ResetZoom();
            window = HudLayout.MinimapRect(1920f, 1080f);
            go = new GameObject("MinimapZoomCamera");
            camera = go.AddComponent<Camera>(); cam = go.AddComponent<FreeCameraController>();
            cam.Target = camera; cam.MapWidth = Cells * 2f; cam.MapHeight = Cells * 2f;
            camera.transform.position = new Vector3(1f, 40f, 1f);
        }

        [TearDown]
        public void TearDown()
        {
            MinimapView.Hide();
            MinimapView.ResetZoom();
            Object.DestroyImmediate(go);
        }

        private void ClickAt(float x, float y, int button)
        {
            Event click = new Event();
            click.type = EventType.MouseDown;
            click.button = button;
            click.mousePosition = new Vector2(x, y);
            MinimapView.ProcessClick(click, window, new GridSystem(new GridSize(Cells, Cells), 2f), cam);
        }

        private void WheelAt(float x, float y, float delta)
        {
            Event wheel = new Event();
            wheel.type = EventType.ScrollWheel;
            wheel.mousePosition = new Vector2(x, y);
            wheel.delta = new Vector2(0f, delta);
            MinimapView.ProcessClick(wheel, window, new GridSystem(new GridSize(Cells, Cells), 2f), cam);
        }

        [TestCase(-9, 1)]
        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(3, 4)]
        [TestCase(99, 4)]
        public void Step_ClampsLevelAndKeepsViewportInsideTexture(int steps, int expectedLevel)
        {
            MinimapZoom zoom = new MinimapZoom(TextureSize);
            zoom.Step(steps, TextureSize);
            Assert.AreEqual(expectedLevel, zoom.Level);
            Rect uv = zoom.TexCoords(TextureSize);
            Assert.GreaterOrEqual(uv.x, 0f); Assert.GreaterOrEqual(uv.y, 0f);
            Assert.LessOrEqual(uv.xMax, 1f + 0.001f); Assert.LessOrEqual(uv.yMax, 1f + 0.001f);
        }

        [Test]
        public void Focus_AtMaxZoom_ClampsCenterToHalfViewport()
        {
            MinimapZoom zoom = new MinimapZoom(TextureSize);
            zoom.Step(3, TextureSize);
            int view = zoom.ViewSize(TextureSize);
            zoom.Focus(-20f, 900f, TextureSize);
            Assert.AreEqual(view * 0.5f, zoom.CenterX, 0.01f);
            Assert.AreEqual(TextureSize - view * 0.5f, zoom.CenterY, 0.01f);
            Assert.AreEqual(MinimapZoom.MaxLevel, (float)TextureSize / view, 0.001f, "уровень 4 уменьшает окно вчетверо");
        }

        [Test]
        public void RectToPixel_AtLevelOne_MatchesPlainMapping()
        {
            MinimapZoom zoom = new MinimapZoom(TextureSize);
            float px, py, plainX, plainY;
            zoom.RectToPixel(window, window.x + 1f, window.y + 1f, TextureSize, out px, out py);
            MinimapMapping.RectToPixel(window, window.x + 1f, window.y + 1f, TextureSize, out plainX, out plainY);
            Assert.AreEqual(plainX, px, 0.001f);
            Assert.AreEqual(plainY, py, 0.001f);
        }

        [Test]
        public void ZoomedCenterPixel_MapsBackToFocusedCell()
        {
            MinimapZoom zoom = new MinimapZoom(TextureSize);
            zoom.Step(3, TextureSize);
            zoom.Focus(10f, 10f, TextureSize);
            float px, py;
            zoom.RectToPixel(window, window.center.x, window.center.y, TextureSize, out px, out py);
            int cellX, cellY;
            MinimapMapping.PixelToCell(px, py, TextureSize, Cells, out cellX, out cellY);
            Assert.AreEqual(6, cellX, "центр окна после зума = клетка под курсором");
            Assert.AreEqual(6, cellY);
        }

        [Test]
        public void ZoomButtonAndWheel_ChangeLevel_WithoutMovingCamera()
        {
            Rect plus, minus;
            MinimapZoom.ButtonRects(window, out plus, out minus);
            ClickAt(plus.center.x, plus.center.y, 0);
            Assert.AreEqual(2, MinimapView.ZoomLevel);
            Assert.AreEqual(1f, camera.transform.position.x, 0.01f);
            WheelAt(window.center.x, window.center.y, -1f);
            Assert.AreEqual(3, MinimapView.ZoomLevel);
            WheelAt(window.center.x, window.center.y, 1f);
            Assert.AreEqual(2, MinimapView.ZoomLevel);
            ClickAt(minus.center.x, minus.center.y, 0);
            Assert.AreEqual(1, MinimapView.ZoomLevel);
        }

        [Test]
        public void ClickToFocus_WorksAtZoomAndRepositionsViewport()
        {
            WheelAt(window.center.x, window.center.y, -1f);
            ClickAt(window.center.x, window.y + 10f, 0);
            Assert.AreNotEqual(1f, camera.transform.position.z, "клик по зумованной карте телепортирует");
            GridSystem grid = new GridSystem(new GridSize(Cells, Cells), 2f);
            GridPosition cell = grid.WorldToGrid(camera.transform.position.x, camera.transform.position.z);
            Assert.AreEqual(25, cell.X, "клик по центру окна после зума — центр сетки по X");
            Assert.AreEqual(35, cell.Y, "клик у верха окна после зума — клетка 35 по Y");
        }

        [Test]
        public void GuardAndVisibility_HiddenPanelsKeepZoomAndFreeTheWorld()
        {
            ClickAt(window.center.x, window.center.y, -1);
            Vector2 screen = new Vector2(window.center.x, Screen.height - window.center.y);
            Assert.IsTrue(UiClickGuard.IsOverMenu(screen), "под окном мир не кликается");
            WheelAt(window.center.x, window.center.y, -1f);
            int before = MinimapView.ZoomLevel;
            MinimapView.Draw(true, false, true, false);
            Assert.IsFalse(UiClickGuard.IsOverMenu(screen), "со скрытым окном мир кликается");
            Assert.AreEqual(before, MinimapView.ZoomLevel, "панели не сбрасывают масштаб");
            Assert.IsTrue(MinimapMapping.IsMinimapVisible(false, false, false, false));
            Assert.IsFalse(MinimapMapping.IsMinimapVisible(false, false, false, true));
        }
    }
}
