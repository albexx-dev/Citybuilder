using NUnit.Framework;

namespace Citybuilder
{
    // Маппинг мини-карты (EPIC-67 шаг 1): клетка ↔ пиксель текстуры ↔ мир.
    // Сетка 50×50, текстуры 50×50 (масштаб 1) и 140×140 (масштаб 2.8) — оба крайних случая плана эпика.
    public class MinimapMappingTests
    {
        private const int GridCells = 50;

        [TestCase(0, 0, 50, 0, 0)]
        [TestCase(49, 49, 50, 49, 49)]
        [TestCase(0, 0, 140, 1, 1)]
        [TestCase(49, 49, 140, 138, 138)]
        [TestCase(0, 49, 140, 1, 138)]
        public void CellToPixel_Corners_MapExpectedPixels(int cellX, int cellY, int textureSize, int expectedX, int expectedY)
        {
            int pixelX, pixelY;
            MinimapMapping.CellToPixel(cellX, cellY, GridCells, textureSize, out pixelX, out pixelY);
            Assert.AreEqual(expectedX, pixelX, "пиксель X клетки " + cellX + "," + cellY);
            Assert.AreEqual(expectedY, pixelY, "пиксель Y клетки " + cellX + "," + cellY);
        }

        // Обратный переход получает точку внутри пикселя, поэтому round-trip проверяется по центру пикселя.
        [TestCase(50)]
        [TestCase(140)]
        public void RoundTrip_EveryCell_ReturnsSameCell(int textureSize)
        {
            for (int cellY = 0; cellY < GridCells; cellY++)
            {
                for (int cellX = 0; cellX < GridCells; cellX++)
                {
                    int pixelX, pixelY;
                    MinimapMapping.CellToPixel(cellX, cellY, GridCells, textureSize, out pixelX, out pixelY);
                    int backX, backY;
                    MinimapMapping.PixelToCell(pixelX + 0.5f, pixelY + 0.5f, textureSize, GridCells, out backX, out backY);
                    Assert.AreEqual(cellX, backX, "клетка X " + cellX + "," + cellY + " при текстуре " + textureSize);
                    Assert.AreEqual(cellY, backY, "клетка Y " + cellX + "," + cellY + " при текстуре " + textureSize);
                }
            }
        }

        [TestCase(-1f, -1f)]
        [TestCase(-0.5f, -0.5f)]
        [TestCase(-999f, -999f)]
        public void PixelToCell_NegativePixel_ClampsToFirstCell(float pixelX, float pixelY)
        {
            int cellX, cellY;
            MinimapMapping.PixelToCell(pixelX, pixelY, 140, GridCells, out cellX, out cellY);
            Assert.AreEqual(0, cellX);
            Assert.AreEqual(0, cellY);
        }

        [TestCase(140f)]
        [TestCase(140.5f)]
        [TestCase(9000f)]
        [TestCase(1e30f)]
        public void PixelToCell_PixelBeyondTexture_ClampsToLastCell(float pixel)
        {
            int cellX, cellY;
            MinimapMapping.PixelToCell(pixel, pixel, 140, GridCells, out cellX, out cellY);
            Assert.AreEqual(GridCells - 1, cellX);
            Assert.AreEqual(GridCells - 1, cellY);
        }

        [Test]
        public void PixelToCell_NaN_ReturnsFirstCell()
        {
            int cellX, cellY;
            MinimapMapping.PixelToCell(float.NaN, float.NaN, 140, GridCells, out cellX, out cellY);
            Assert.AreEqual(0, cellX);
            Assert.AreEqual(0, cellY);
        }

        [TestCase(0, 50, 50)]
        [TestCase(0, 0, 140)]
        [TestCase(140, 50, 0)]
        public void Mapping_DegenerateSizes_DoNotThrow(int textureSize, int gridSize, int cell)
        {
            int pixelX, pixelY, backX, backY;
            MinimapMapping.CellToPixel(cell, cell, gridSize, textureSize, out pixelX, out pixelY);
            Assert.GreaterOrEqual(pixelX, 0);
            Assert.Less(pixelX, textureSize > 0 ? textureSize : 1);
            Assert.GreaterOrEqual(pixelY, 0);
            Assert.Less(pixelY, textureSize > 0 ? textureSize : 1);
            MinimapMapping.PixelToCell(-3f, 7f, textureSize, gridSize, out backX, out backY);
            Assert.GreaterOrEqual(backX, 0);
            Assert.Less(backX, gridSize > 0 ? gridSize : 1);
            Assert.GreaterOrEqual(backY, 0);
            Assert.Less(backY, gridSize > 0 ? gridSize : 1);
        }

        [TestCase(-1, 0)]
        [TestCase(-50, 0)]
        [TestCase(GridCells, 139)]
        [TestCase(GridCells + 400, 139)]
        public void CellToPixel_CellOutsideGrid_ClampsToTexture(int cell, int expectedPixel)
        {
            int pixelX, pixelY;
            MinimapMapping.CellToPixel(cell, cell, GridCells, 140, out pixelX, out pixelY);
            Assert.AreEqual(expectedPixel, pixelX);
            Assert.AreEqual(expectedPixel, pixelY);
        }

        // CellToWorld обязан совпадать с GridSystem.GridToWorld — иначе маркер и клик разойдутся с реальной сеткой.
        [TestCase(0, 0)]
        [TestCase(7, 3)]
        [TestCase(25, 41)]
        [TestCase(49, 49)]
        public void CellToWorld_MatchesGridSystem(int cellX, int cellY)
        {
            GridSystem grid = new GridSystem(new GridSize(GridCells, GridCells), 2f);
            float fromGridX, fromGridZ, fromMappingX, fromMappingZ;
            grid.GridToWorld(new GridPosition(cellX, cellY), out fromGridX, out fromGridZ);
            MinimapMapping.CellToWorld(cellX, cellY, 2f, out fromMappingX, out fromMappingZ);
            Assert.AreEqual(fromGridX, fromMappingX, 0.0001f, "X клетки " + cellX + "," + cellY);
            Assert.AreEqual(fromGridZ, fromMappingZ, 0.0001f, "Z клетки " + cellX + "," + cellY);
        }

        // Клик по пикселу → клетка → мир: мировая точка должна попадать внутрь той же клетки реальной сетки.
        [TestCase(1f, 50)]
        [TestCase(2f, 140)]
        [TestCase(4f, 140)]
        public void PixelToCell_ThenCellToWorld_LandsInsideSameGridCell(float cellSize, int textureSize)
        {
            GridSystem grid = new GridSystem(new GridSize(GridCells, GridCells), cellSize);
            for (int pixel = 0; pixel < textureSize; pixel++)
            {
                int cellX, cellY;
                MinimapMapping.PixelToCell(pixel + 0.5f, pixel + 0.5f, textureSize, GridCells, out cellX, out cellY);
                float worldX, worldZ;
                MinimapMapping.CellToWorld(cellX, cellY, cellSize, out worldX, out worldZ);
                Assert.IsTrue(grid.IsInside(new GridPosition(cellX, cellY)), "клетка вне сетки для пикселя " + pixel);
                Assert.AreEqual(cellX, grid.WorldToGrid(worldX, worldZ).X, "X для пикселя " + pixel);
                Assert.AreEqual(cellY, grid.WorldToGrid(worldX, worldZ).Y, "Z для пикселя " + pixel);
            }
        }
    }
}
