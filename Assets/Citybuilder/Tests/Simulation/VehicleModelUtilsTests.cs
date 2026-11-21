using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Авто-масштаб Kenney-моделей (жалоба владельца: «машины огроменные»): Fit приводит
    // длину модели по Z к целевой в метрах мира и выравнивает центр уже под новым масштабом.
    public class VehicleModelUtilsTests
    {
        private GameObject MakeModel(float lengthZ, float heightY, float offsetX, float offsetY)
        {
            GameObject root = new GameObject("Model");
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(offsetX - 0.5f, offsetY, -lengthZ * 0.5f),
                new Vector3(offsetX + 0.5f, offsetY + heightY, lengthZ * 0.5f)
            };
            mesh.triangles = new[] { 0, 1, 0 };
            MeshFilter filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            return root;
        }

        // TryBounds меряет КОРНЕВУЮ длину (до своего localScale) — мировая = корневая × scale.z.
        private static float WorldLength(GameObject model)
        {
            Vector3 min, max;
            Assert.IsTrue(VehicleModelUtils.TryBounds(model, out min, out max));
            return (max.z - min.z) * model.transform.localScale.z;
        }

        [Test]
        public void Fit_ScalesLengthToTarget_AndAlignsCenterOnPivot()
        {
            GameObject model = MakeModel(13.46f, 1f, 0f, 0f);
            float multiplier = VehicleModelUtils.Fit(model, 1.6f);
            Assert.AreEqual(1.6f, WorldLength(model), 0.001f, "мировая длина = целевой");
            Assert.AreEqual(1.6f / 13.46f, multiplier, 0.0001f);
            Vector3 min, max;
            Assert.IsTrue(VehicleModelUtils.TryBounds(model, out min, out max));
            Assert.Less(Mathf.Abs(min.y * model.transform.localScale.y), 0.001f, "низ на земле (в мире)");
            Object.DestroyImmediate(model);
        }

        [Test]
        public void Fit_MeasuresWorldLength_KeepsExistingScaleMeaning()
        {
            GameObject model = MakeModel(10f, 1f, 0f, 0f);
            model.transform.localScale = Vector3.one * 2f; // мир: 20
            VehicleModelUtils.Fit(model, 2f);
            Assert.AreEqual(2f, WorldLength(model), 0.001f, "мировая длина приведена к цели");
            Object.DestroyImmediate(model);
        }

        [Test]
        public void Fit_ModelWithoutMeshes_ReturnsOne_AndDoesNotThrow()
        {
            GameObject empty = new GameObject("Empty");
            Assert.AreEqual(1f, VehicleModelUtils.Fit(empty, 1.6f));
            Object.DestroyImmediate(empty);
        }
    }
}
