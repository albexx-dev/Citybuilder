using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Графики инфоменю (EPIC-68 шаг 2): пиксели линии, подписи, троттлинг, пустая история.
    public class MetricsGraphViewTests
    {
        [SetUp]
        public void SetUp()
        {
            MetricsGraphTexture.Reset();
        }

        private static MetricsHistory History(int count, int populationStep, int balanceStart)
        {
            MetricsHistory history = new MetricsHistory();
            for (int i = 0; i < count; i++)
            {
                history.Record(new MetricsSnapshot(i, 10 + i * populationStep, balanceStart + i * 5,
                    0f, 0f, 50f + i, 0f, 0f));
            }
            return history;
        }

        // RGBA32 квантует каналы: сравнение с допуском (1/255 дрейф) вместо точного Color.
        private static bool Close(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.004f && Mathf.Abs(a.g - b.g) < 0.004f && Mathf.Abs(a.b - b.b) < 0.004f;
        }

        // Высота строки линии в столбце x: -1, если столбик пуст.
        private static int TopRow(Texture2D graph, int x)
        {
            for (int y = MetricsGraphTexture.Height - 1; y >= 0; y--)
            {
                if (!Close(graph.GetPixel(x, y), new Color(0.13f, 0.15f, 0.17f))) return y;
            }
            return -1;
        }

        [Test]
        public void Refresh_RisingPopulation_LineGoesUpFromLeftToRight()
        {
            MetricsHistory history = History(48, 2, 100);
            Assert.IsTrue(MetricsGraphTexture.RefreshIfDue(history));
            Texture2D graph = MetricsGraphTexture.At(0);
            Assert.IsNotNull(graph);
            Assert.Less(TopRow(graph, 0), TopRow(graph, MetricsGraphTexture.Width - 1));
        }

        [Test]
        public void Refresh_NegativeBalance_StillDrawsLineInsideTexture()
        {
            MetricsHistory history = History(24, 1, -300);
            MetricsGraphTexture.RefreshIfDue(history);
            Texture2D graph = MetricsGraphTexture.At(1);
            int top = TopRow(graph, MetricsGraphTexture.Width - 1);
            Assert.GreaterOrEqual(top, 0);
            Assert.Less(top, MetricsGraphTexture.Height);
        }

        [Test]
        public void Refresh_AllSeries_GetTheirOwnTextureAndColor()
        {
            MetricsHistory history = History(12, 1, 50);
            MetricsGraphTexture.RefreshIfDue(history);
            for (int i = 0; i < MetricsGraphTexture.SeriesCount; i++)
            {
                Assert.IsNotNull(MetricsGraphTexture.At(i));
                Assert.AreNotSame(MetricsGraphTexture.At(i), MetricsGraphTexture.At((i + 1) % MetricsGraphTexture.SeriesCount));
            }
            Assert.IsNull(MetricsGraphTexture.At(-1));
            Assert.IsNull(MetricsGraphTexture.At(MetricsGraphTexture.SeriesCount));
        }

        [Test]
        public void Refresh_FlatSeries_DrawsLineInMiddleWithoutCrash()
        {
            MetricsHistory history = new MetricsHistory();
            for (int i = 0; i < 10; i++) history.Record(new MetricsSnapshot(i, 42, 0, 0f, 0f, 0f, 0f, 0f));
            MetricsGraphTexture.RefreshIfDue(history);
            int top = TopRow(MetricsGraphTexture.At(0), 0);
            Assert.AreEqual(MetricsGraphTexture.Height / 2, top, 1);
        }

        [Test]
        public void Refresh_WithinHalfSecond_KeepsPreviousPixels()
        {
            MetricsHistory history = History(8, 1, 0);
            Assert.IsTrue(MetricsGraphTexture.RefreshIfDue(history, 10f));
            int middle = MetricsGraphTexture.Width / 2;
            int before = TopRow(MetricsGraphTexture.At(0), middle);
            history.Record(new MetricsSnapshot(99, 900, 0, 0f, 0f, 0f, 0f, 0f));
            Assert.IsFalse(MetricsGraphTexture.RefreshIfDue(history, 10.4f));
            Assert.AreEqual(before, TopRow(MetricsGraphTexture.At(0), middle));
            Assert.IsTrue(MetricsGraphTexture.RefreshIfDue(history, 10.6f));
            // Всплеск 900 растянул ось: середина серии опустилась к нулю.
            Assert.Less(TopRow(MetricsGraphTexture.At(0), middle), before);
        }

        [Test]
        public void Refresh_EmptyOrNullHistory_DoesNothing()
        {
            Assert.IsFalse(MetricsGraphTexture.RefreshIfDue(null));
            Assert.IsFalse(MetricsGraphTexture.RefreshIfDue(new MetricsHistory()));
        }

        [Test]
        public void Labels_ShowNameAndLatestValue()
        {
            MetricsHistory history = History(5, 3, 200);
            StringAssert.StartsWith("Население:", MetricsGraphTexture.LabelAt(0, history));
            StringAssert.EndsWith("22", MetricsGraphTexture.LabelAt(0, history));
            StringAssert.EndsWith("220", MetricsGraphTexture.LabelAt(1, history));
            Assert.AreEqual(string.Empty, MetricsGraphTexture.LabelAt(9, history));
        }

        [Test]
        public void Draw_EmptyHistory_ReservesHintHeightOnly()
        {
            float y = 0f;
            MetricsGraphView.Draw(ref y, 300f, new MetricsHistory());
            Assert.AreEqual(MetricsGraphView.EmptyHeight, y, 0.001f);
            Assert.AreEqual(MetricsGraphView.EmptyHeight, MetricsGraphView.SectionHeight(null), 0.001f);
        }

        [Test]
        public void Draw_WithHistory_AdvancesByFourRows()
        {
            MetricsHistory history = History(20, 1, 0);
            float y = 0f;
            MetricsGraphView.Draw(ref y, 300f, history);
            float expected = MetricsGraphView.HeaderHeight + MetricsGraphTexture.SeriesCount * MetricsGraphView.RowHeight;
            Assert.AreEqual(expected, y, 0.001f);
            Assert.AreEqual(expected, MetricsGraphView.SectionHeight(history), 0.001f);
        }

        [Test]
        public void Draw_NarrowMenu_KeepsGraphInsideWidth()
        {
            MetricsHistory history = History(6, 1, 0);
            float y = 0f;
            MetricsGraphView.Draw(ref y, 80f, history);
            Assert.Greater(y, 0f);
            Assert.IsNotNull(MetricsGraphTexture.At(0));
        }
    }
}