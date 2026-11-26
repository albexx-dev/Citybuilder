using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Масштаб сетки (issue #159 / ADR-017): единственный источник Width/Height/CellSize — GridVisualizer.
    // 50 — прежняя карта, 100 — целевая конфигурация задач масштаба.
    public class GridScaleTests
    {
        private static GridVisualizer MakeGrid(int cells, float cellSize)
        {
            GameObject go = new GameObject("grid");
            GridVisualizer visualizer = go.AddComponent<GridVisualizer>();
            visualizer.Width = cells;
            visualizer.Height = cells;
            visualizer.CellSize = cellSize;
            return visualizer;
        }

        [Test]
        public void GridVisualizer_DefaultIsHundred()
        {
            GridVisualizer visualizer = new GameObject("grid").AddComponent<GridVisualizer>();
            Assert.AreEqual(100, visualizer.Width);
            Assert.AreEqual(100, visualizer.Height);
            Assert.AreEqual(1f, visualizer.CellSize, 1e-6f);
        }

        [TestCase(50)]
        [TestCase(100)]
        public void GridVisualizer_GridIsBuiltFromItsFields(int cells)
        {
            GridVisualizer visualizer = MakeGrid(cells, 2f);
            Assert.AreEqual(cells, visualizer.Grid.Size.Width);
            Assert.AreEqual(cells, visualizer.Grid.Size.Height);
            Assert.AreEqual(2f, visualizer.Grid.CellSize, 1e-6f);
        }

        [TestCase(50)]
        [TestCase(100)]
        public void CameraBounds_ComeFromGridVisualizer(int cells)
        {
            FreeCameraController camera = new GameObject("camera").AddComponent<FreeCameraController>();
            camera.Visualizer = MakeGrid(cells, 2f);
            Assert.AreEqual(cells * 2f, camera.MapWidth, 1e-6f);
            Assert.AreEqual(cells * 2f, camera.MapHeight, 1e-6f);
        }

        [TestCase(50)]
        [TestCase(100)]
        public void CameraFocus_ClampsToGridSize(int cells)
        {
            GameObject go = new GameObject("camera");
            Camera cam = go.AddComponent<Camera>();
            FreeCameraController camera = go.AddComponent<FreeCameraController>();
            camera.Target = cam;
            camera.Visualizer = MakeGrid(cells, 1f);
            camera.FocusOn(cells + 50f, -50f);
            Assert.AreEqual(cells + camera.Margin, camera.Target.transform.position.x, 1e-4f);
            Assert.AreEqual(-camera.Margin, camera.Target.transform.position.z, 1e-4f);
        }

        [TestCase(50)]
        [TestCase(100)]
        public void GroundBorder_IsSixMetersWiderThanGrid(int cells)
        {
            Vector3 min, max;
            GridGroundBuilder.BorderBounds(cells, cells, 1f, 0f, out min, out max);
            Assert.AreEqual(-GridGroundBuilder.BorderWidth, min.x, 1e-4f);
            Assert.AreEqual(cells + GridGroundBuilder.BorderWidth, max.x, 1e-4f);
            Assert.AreEqual(cells + GridGroundBuilder.BorderWidth, max.z, 1e-4f);
        }

        [TestCase(50, 50)]
        [TestCase(50, 140)]
        [TestCase(100, 100)]
        [TestCase(100, 140)]
        public void MinimapMapping_RoundTripForBothGridSizes(int cells, int textureSize)
        {
            for (int cellY = 0; cellY < cells; cellY++)
            {
                for (int cellX = 0; cellX < cells; cellX++)
                {
                    int pixelX, pixelY;
                    MinimapMapping.CellToPixel(cellX, cellY, cells, textureSize, out pixelX, out pixelY);
                    int backX, backY;
                    MinimapMapping.PixelToCell(pixelX + 0.5f, pixelY + 0.5f, textureSize, cells,
                        out backX, out backY);
                    Assert.AreEqual(cellX, backX, "X " + cellX + "," + cellY);
                    Assert.AreEqual(cellY, backY, "Y " + cellX + "," + cellY);
                }
            }
        }
    }
}
