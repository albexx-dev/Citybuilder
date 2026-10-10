using UnityEngine;

namespace Citybuilder
{
    // Показывает население. V1: штатный OnGUI-лейбл без UI-пакетов.
    // Нет контроллера — молча не рисует. Есть, но нет системы — рисует нули.
    public class PopulationView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер населения. Опционален: иначе ищется на этом же объекте.")]
        public PopulationController Controller;

        private void OnGUI()
        {
            PopulationController controller = Controller;
            if (controller == null) controller = GetComponent<PopulationController>();
            if (controller == null) return;
            int residents = 0;
            int households = 0;
            if (controller.Population != null)
            {
                residents = controller.Population.TotalResidents;
                households = controller.Population.HouseholdCount;
            }
            string text = "Pop: " + residents + " / " + controller.GrownCapacity;
            text += " — " + households + " households";
            GUI.Label(new Rect(10, 30, 300, 30), text);
        }
    }
}
