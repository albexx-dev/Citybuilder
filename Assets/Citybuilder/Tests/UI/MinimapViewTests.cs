using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Окно мини-карты целиком (EPIC-67 шаг 2): заполнение схемы, клик-телепорт с конвенцией Y, гард кликов.
    // GUI доступен только внутри OnGUI, поэтому клики идут через шов ProcessClick, скрытие — через Hide.
    public class MinimapViewTests
    {
        private const int Cells = 50;
        private const float Step = 2f;
        private GridSystem grid;
        private ZoneSystem zones;
        private readonly List<RoadState> roads = new List<RoadState>();
        private readonly List<BuildingState> homes = new List<BuildingState>();
        private readonly List<BuildingState> placed = new List<BuildingState>();
        private Rect window;
        private GameObject go;
        private Camera camera;
        private FreeCameraController cam;
        [SetUp]
        public void SetUp()
        {
            grid = new GridSystem(new GridSize(Cells, Cells), Step); zones = new ZoneSystem(grid);
            roads.Clear(); homes.Clear(); placed.Clear();
            window = HudLayout.MinimapRect(1920f, 1080f);
            MinimapTexture.Reset();
            go = new GameObject("MinimapViewCamera");
            camera = go.AddComponent<Camera>(); cam = go.AddComponent<FreeCameraController>();
            cam.Target = camera; cam.MapWidth = Cells * Step; cam.MapHeight = Cells * Step;
            camera.transform.position = new Vector3(1f, 40f, 1f);
        }
        [TearDown]
        public void TearDown() { MinimapView.Hide(); Object.DestroyImmediate(go); }
        private void Refresh() { MinimapTexture.RefreshIfDue(grid, zones, roads, homes, placed); }
        // RGBA32 квантует каналы: сравнение с допуском (1/255 дрейф) вместо точного Color.
        private static bool Same(Color pixel, Color expected)
        {
            return Mathf.Abs(pixel.r - expected.r) < 0.004f && Mathf.Abs(pixel.g - expected.g) < 0.004f
                && Mathf.Abs(pixel.b - expected.b) < 0.004f;
        }

        // Пиксель клетки по тому же маппингу, что и клик: CellToPixel даёт центр её блока.
        private Color PixelOfCell(int cellX, int cellY)
        {
            int px, py;
            MinimapMapping.CellToPixel(cellX, cellY, Cells, MinimapTexture.Size, out px, out py);
            return MinimapTexture.Current.GetPixel(px, py);
        }
        private void ClickAt(float guiX, float guiY, int button)
        {
            Event click = new Event();
            click.type = EventType.MouseDown; click.button = button;
            click.mousePosition = new Vector2(guiX, guiY);
            MinimapView.ProcessClick(click, window, grid, cam);
        }

        // RGB зон взяты из 3D-кистей ZoneView.cs:50-52, а не из констант теста: цвет не уедет незамеченным.
        [Test]
        public void Refresh_ZoneRoadAndBuilding_PaintTheirCellsAndLeaveNeighbours()
        {
            List<GridPosition> footprint = new List<GridPosition>();
            zones.SetZone(new GridPosition(7, 11), ZoneType.Residential);
            zones.SetZone(new GridPosition(8, 12), ZoneType.Commercial);
            zones.SetZone(new GridPosition(9, 13), ZoneType.Industrial);
            roads.Add(new RoadState(EntityId.New(), new GridPosition(3, 40)));
            homes.Add(new BuildingState(EntityId.New(), null, new GridPosition(20, 21), footprint));
            placed.Add(new BuildingState(EntityId.New(), null, new GridPosition(22, 23), footprint));
            Refresh();
            Assert.IsTrue(Same(PixelOfCell(7, 11), new Color(0.1f, 0.45f, 0.1f)), "жилая зона");
            Assert.IsTrue(Same(PixelOfCell(8, 12), new Color(0.1f, 0.2f, 0.6f)), "коммерция");
            Assert.IsTrue(Same(PixelOfCell(9, 13), new Color(0.6f, 0.3f, 0.05f)), "промзона");
            Assert.IsTrue(Same(PixelOfCell(3, 40), new Color(0.45f, 0.45f, 0.45f)), "дорога — серый без тона");
            Assert.IsTrue(Same(PixelOfCell(20, 21), MinimapTexture.BuildingColor), "выросший дом");
            Assert.IsTrue(Same(PixelOfCell(22, 23), MinimapTexture.BuildingColor), "построенное вручную");
            Assert.IsTrue(Same(PixelOfCell(21, 22), MinimapTexture.BackgroundColor), "соседняя клетка осталась фоном");
        }
        [Test]
        public void Refresh_EmptyCity_LeavesOnlyBackground()
        {
            Refresh();
            Color[] pixels = MinimapTexture.Current.GetPixels();
            int wrong = 0;
            for (int i = 0; i < pixels.Length; i++) if (!Same(pixels[i], MinimapTexture.BackgroundColor)) wrong++;
            Assert.AreEqual(0, wrong, "в пустом городе чужих пикселей нет");
        }
        [Test]
        public void Refresh_BeforeHalfSecond_KeepsPreviousPixels()
        {
            Refresh();
            zones.SetZone(new GridPosition(5, 6), ZoneType.Residential);
            Assert.IsFalse(MinimapTexture.RefreshIfDue(grid, zones, roads, homes, placed, 0.1f), "до 0.5 с перекраски нет");
            Assert.IsTrue(Same(PixelOfCell(5, 6), MinimapTexture.BackgroundColor), "фон");
            Assert.IsTrue(MinimapTexture.RefreshIfDue(grid, zones, roads, homes, placed, 100f), "после 0.5 с есть");
            Assert.IsTrue(Same(PixelOfCell(5, 6), MinimapTexture.ResidentialColor), "жилая");
        }

        // Конвенция Y (ловит переворот): верх окна — последний ряд сетки (+Z = 99), низ — первый (+Z = 1).
        [Test]
        public void ClickAtTopAndBottomEdgesOfWindow_JumpToOppositeGridRows()
        {
            ClickAt(window.center.x, window.y + 1f, 0);
            Assert.AreEqual(99f, camera.transform.position.z, 0.05f, "клик у верха окна = клетка Y 49");
            Assert.AreEqual(51f, camera.transform.position.x, 0.05f, "середина окна по X = клетка 25");
            ClickAt(window.center.x, window.yMax - 1f, 0);
            Assert.AreEqual(1f, camera.transform.position.z, 0.05f, "клик у низа окна = клетка Y 0");
        }
        [Test]
        public void ClickOutsideWindowOrRightButton_LeavesCameraAlone()
        {
            ClickAt(window.x - 5f, window.center.y, 0);
            ClickAt(window.center.x, window.center.y, 1);
            Assert.AreEqual(1f, camera.transform.position.x, 0.01f);
            Assert.AreEqual(1f, camera.transform.position.z, 0.01f);
        }

        // IsOverMenu ждёт экранные координаты: GUI-y окна = Screen.height минус экранный y.
        [Test]
        public void WindowRect_IsGuardedWhileShownAndFreeWhileHidden()
        {
            Vector2 screen = new Vector2(window.center.x, Screen.height - window.center.y);
            ClickAt(window.center.x, window.center.y, -1);
            Assert.IsTrue(UiClickGuard.IsOverMenu(screen), "под окном мир не кликается");
            MinimapView.Hide();
            Assert.IsFalse(UiClickGuard.IsOverMenu(screen), "со скрытым окном мир кликается");
        }
        [Test]
        public void Draw_WithoutSceneSystems_HidesWindowAndThrowsNothing()
        {
            MinimapView.Hide();
            Assert.IsFalse(UiClickGuard.IsOverMenu(new Vector2(window.center.x, Screen.height - window.center.y)));
            ClickAt(window.center.x, window.center.y, -1);
            MinimapView.Draw(true, false, true, false);
            Assert.IsFalse(UiClickGuard.IsOverMenu(new Vector2(window.center.x, Screen.height - window.center.y)));
        }
        [Test]
        public void RectToPixel_InvertsGuiY_AndKeepsX()
        {
            Rect rect = new Rect(100f, 20f, MinimapTexture.Size, MinimapTexture.Size);
            float px, py;
            MinimapMapping.RectToPixel(rect, 170f, 21f, MinimapTexture.Size, out px, out py);
            Assert.AreEqual(139f, py, 0.5f, "1 px от верха rect'а = строка 139, почти верх текстуры");
            MinimapMapping.RectToPixel(rect, 170f, 159f, MinimapTexture.Size, out px, out py);
            Assert.AreEqual(1f, py, 0.5f, "1 px от низа rect'а = строка 1, почти низ текстуры");
            Assert.AreEqual(70f, px, 0.5f, "X не инвертируется");
        }
    }
}
