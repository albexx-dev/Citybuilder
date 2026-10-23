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

        private void OnGUI()
        {
            WaterController controller = Controller;
            if (controller == null) controller = GetComponent<WaterController>();
            if (controller == null) return;
            string text = "Вода: " + controller.Demand + "/" + controller.Supply;
            text += " (" + ToPercent(controller.Rate01) + "%)";
            // y=278: ниже энергии (246+26+6 зазор), иначе налезает на лейблы.
            GUI.Label(new Rect(10, 278, 400, 30), text);
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
