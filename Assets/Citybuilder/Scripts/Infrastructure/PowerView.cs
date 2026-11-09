using UnityEngine;

namespace Citybuilder
{
    // Показывает энергию города. V1: штатный OnGUI-лейбл под строкой связности (y=246).
    // Нет контроллера — молча не рисует.
    public class PowerView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер энергии. Опционален: иначе ищется на этом же объекте.")]
        public PowerController Controller;

        public string CurrentText
        {
            get
            {
                PowerController controller = Controller != null ? Controller : GetComponent<PowerController>();
                if (controller == null) return string.Empty;
                return "Энергия: " + controller.Demand + "/" + controller.Supply + " (" + ToPercent(controller.Rate01) + "%)";
            }
        }

        private void OnGUI()
        {
            string text = CurrentText;
            if (string.IsNullOrEmpty(text)) return;
            HudText.Draw(10, 246, text);
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
