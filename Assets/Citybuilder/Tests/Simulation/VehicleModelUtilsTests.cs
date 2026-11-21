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

        [Test]
        public void Fit_ScalesLengthToTarget_AndAlignsCenterOnPivot()
        {
            GameObject model = MakeModel(13.46f, 1f, 0f, 0f);
            float scale = VehicleModelUtils.Fit(model, 1.6f);
            Vector3 min, max;
            Assert.IsTrue(VehicleModelUtils.TryBounds(model, out min, out max));
            Assert.AreEqual(1.6f, max.z - min.z, 0.001f, "длина = целевой");
            Assert.AreEqual(0.8f, (min.z + max.z) * 0.5f, 0.01f, "центр на оси корня");
            Assert.Less(Mathf.Abs(min.y), 0.001f, "низ на земле");
            Assert.AreEqual(1.6f / 13.46f, scale, 0.0001f);
            Object.DestroyImmediate(model);
        }

        [Test]
        public void Fit_MultipliesExistingScale_NotReset()
        {
            GameObject model = MakeModel(10f, 1f, 0f, 0f);
            model.transform.localScale = Vector3.one * 2f;
            VehicleModelUtils.Fit(model, 2f);
            Vector3 min, max;
            Assert.IsTrue(VehicleModelUtils.TryBounds(model, out min, out max));
            Assert.AreEqual(2f, max.z - min.z, 0.001f);
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
