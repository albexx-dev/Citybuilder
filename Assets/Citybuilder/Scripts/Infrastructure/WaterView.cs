using UnityEngine;

namespace Citybuilder
{
    // Показывает воду города. V1: штатный OnGUI-лейбл под строкой энергии (y=278).
    // Нет контроллера — молча не рисует.
    public class WaterView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер воды. Опционален: иначе ищется на этом же объекте.")]
        public WaterController Controller;

        public string CurrentText
        {
            get
            {
                WaterController controller = Controller != null ? Controller : GetComponent<WaterController>();
                if (controller == null) return string.Empty;
                return "Вода: " + controller.Demand + "/" + controller.Supply + " (" + ToPercent(controller.Rate01) + "%)";
            }
        }

        private void OnGUI()
        {
            string text = CurrentText;
            if (string.IsNullOrEmpty(text)) return;
            HudText.Draw(10, 278, text);
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
