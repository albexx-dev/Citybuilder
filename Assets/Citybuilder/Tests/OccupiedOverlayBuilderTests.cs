using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Чистая геометрия квадов занятых клеток (EPIC-10 шаг 2). Без Unity-объектов: только массивы.
    public class OccupiedOverlayBuilderTests
    {
        [Test]
        public void BuildGeometry_SingleCell_MakesQuadAtCellOrigin()
        {
            Vector3[] vertices;
            int[] triangles;
            List<GridPosition> cells = new List<GridPosition> { new GridPosition(0, 0) };
            OccupiedOverlayBuilder.BuildGeometry(cells, 1f, out vertices, out triangles);
            Assert.AreEqual(4, vertices.Length);
            Assert.AreEqual(new Vector3(0f, 0.01f, 0f), vertices[0]);
            Assert.AreEqual(new Vector3(1f, 0.01f, 0f), vertices[1]);
            Assert.AreEqual(new Vector3(1f, 0.01f, 1f), vertices[2]);
            Assert.AreEqual(new Vector3(0f, 0.01f, 1f), vertices[3]);
        }

        [Test]
        public void BuildGeometry_SingleCell_MakesTwoTriangles()
        {
            int[] triangles;
            Vector3[] vertices;
            List<GridPosition> cells = new List<GridPosition> { new GridPosition(0, 0) };
            OccupiedOverlayBuilder.BuildGeometry(cells, 1f, out vertices, out triangles);
            Assert.AreEqual(new[] { 0, 2, 1, 0, 3, 2 }, triangles);
        }

        [Test]
        public void BuildGeometry_OffsetCell_StartsAtCellCoordinates()
        {
            Vector3[] vertices;
            int[] triangles;
            List<GridPosition> cells = new List<GridPosition> { new GridPosition(2, 3) };
            OccupiedOverlayBuilder.BuildGeometry(cells, 1f, out vertices, out triangles);
            Assert.AreEqual(new Vector3(2f, 0.01f, 3f), vertices[0]);
            Assert.AreEqual(new Vector3(3f, 0.01f, 4f), vertices[2]);
        }

        [Test]
        public void BuildGeometry_CellSize_ScalesQuad()
        {
            Vector3[] vertices;
            int[] triangles;
            List<GridPosition> cells = new List<GridPosition> { new GridPosition(1, 0) };
            OccupiedOverlayBuilder.BuildGeometry(cells, 2.5f, out vertices, out triangles);
            Assert.AreEqual(new Vector3(2.5f, 0.01f, 0f), vertices[0]);
            Assert.AreEqual(new Vector3(5f, 0.01f, 2.5f), vertices[2]);
        }

        [Test]
        public void BuildGeometry_ManyCells_SizesArraysPerCell()
        {
            Vector3[] vertices;
            int[] triangles;
            List<GridPosition> cells = new List<GridPosition>
            {
                new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(2, 1)
            };
            OccupiedOverlayBuilder.BuildGeometry(cells, 1f, out vertices, out triangles);
            Assert.AreEqual(12, vertices.Length);
            Assert.AreEqual(18, triangles.Length);
            Assert.AreEqual(new Vector3(2f, 0.01f, 1f), vertices[8]);
            Assert.AreEqual(8, triangles[12]);
        }
    }
}
