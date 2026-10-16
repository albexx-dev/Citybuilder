using UnityEngine;

namespace Citybuilder
{
    // Показывает связность домов с дорогами. V1: штатный OnGUI-лейбл под строкой цели.
    // Нет контроллера — молча не рисует.
    public class ConnectivityView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер связности. Опционален: иначе ищется на этом же объекте.")]
        public ConnectivityController Controller;

        private void OnGUI()
        {
            ConnectivityController controller = Controller;
            if (controller == null) controller = GetComponent<ConnectivityController>();
            if (controller == null) return;
            string text = "Связано: " + controller.Connected + "/" + controller.Total;
            text += " (" + ToPercent(controller.Rate01) + "%)";
            // y=214: ниже меню (182+26+6 зазор), иначе налезает на кнопки.
            GUI.Label(new Rect(10, 214, 400, 30), text);
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
