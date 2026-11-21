using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // issue #94: маркер — отдельный объект со своим материалом; roof/prefab не мутируется.
    public class HouseLevelMarkerTests
    {
        [Test]
        public void Update_LevelTwo_AddsOwnMarkerAndLevelOne_RemovesIt()
        {
            GameObject house = new GameObject("House");
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(house.transform, false);
            Material shared = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.2f, 0.3f, 0.4f) };
            roof.GetComponent<MeshRenderer>().sharedMaterial = shared;

            HouseLevelMarker.Update(house, 2);

            Transform marker = house.transform.Find(HouseLevelMarker.MarkerName);
            Assert.IsNotNull(marker);
            Material own = marker.GetComponent<MeshRenderer>().sharedMaterial;
            Assert.AreNotSame(shared, own);
            Assert.AreEqual(HouseLevelStyle.MarkerColor(2).r, own.color.r, 0.0001f);
            Assert.AreEqual(shared.color.r, roof.GetComponent<MeshRenderer>().sharedMaterial.color.r, 0.0001f);

            // Материал переиспользуется при смене уровня: новых утечек нет.
            HouseLevelMarker.Update(house, 2);
            Assert.AreSame(own, house.transform.Find(HouseLevelMarker.MarkerName).GetComponent<MeshRenderer>().sharedMaterial);

            HouseLevelMarker.Update(house, 1);
            Assert.IsNull(house.transform.Find(HouseLevelMarker.MarkerName));

            Object.DestroyImmediate(shared);
            Object.DestroyImmediate(own);
            Object.DestroyImmediate(house);
        }
    }
}
