using UnityEngine;

namespace Citybuilder
{
    // Показывает мусор города. V1: штатный OnGUI-лейбл под строкой воды (y=310).
    // Нет контроллера — молча не рисует.
    public class WasteView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер мусора. Опционален: иначе ищется на этом же объекте.")]
        public WasteController Controller;

        private void OnGUI()
        {
            WasteController controller = Controller;
            if (controller == null) controller = GetComponent<WasteController>();
            if (controller == null) return;
            string text = "Мусор: " + controller.Demand + "/" + controller.Supply;
            text += " (" + ToPercent(controller.Rate01) + "%)";
            // y=310: ниже воды (278+30+2), иначе налезает на лейблы.
            HudText.Draw(10, 310, text);
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
