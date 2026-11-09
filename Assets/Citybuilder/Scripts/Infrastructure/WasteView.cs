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

        public string CurrentText
        {
            get
            {
                WasteController controller = Controller != null ? Controller : GetComponent<WasteController>();
                if (controller == null) return string.Empty;
                return "Мусор: " + controller.Demand + "/" + controller.Supply + " (" + ToPercent(controller.Rate01) + "%)";
            }
        }

        private void OnGUI()
        {
            string text = CurrentText;
            if (string.IsNullOrEmpty(text)) return;
            HudText.Draw(10, 310, text);
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
