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

        private void OnGUI()
        {
            PowerController controller = Controller;
            if (controller == null) controller = GetComponent<PowerController>();
            if (controller == null) return;
            string text = "Энергия: " + controller.Demand + "/" + controller.Supply;
            text += " (" + ToPercent(controller.Rate01) + "%)";
            // y=246: ниже связности (214+26+6 зазор), иначе налезает на лейблы.
            HudText.Draw(10, 246, text);
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
