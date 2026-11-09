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

        public string CurrentText
        {
            get
            {
                ConnectivityController controller = Controller != null ? Controller : GetComponent<ConnectivityController>();
                if (controller == null) return string.Empty;
                return "Связано: " + controller.Connected + "/" + controller.Total + " (" + ToPercent(controller.Rate01) + "%)";
            }
        }

        private void OnGUI()
        {
            string text = CurrentText;
            if (string.IsNullOrEmpty(text)) return;
            HudText.Draw(10, 214, text);
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
