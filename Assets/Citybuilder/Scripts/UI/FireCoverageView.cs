using UnityEngine;

namespace Citybuilder
{
    // Показывает покрытие служб. V1: штатный OnGUI-лейбл под строкой мусора (y=310).
    // Нет контроллера — молча не рисует.
    public class FireCoverageView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер покрытия. Опционален: иначе ищется на этом же объекте.")]
        public FireCoverageController Controller;

        [Tooltip("Второй контроллер — полиция. Пусто = одна строка")]
        public FireCoverageController Police;

        public string CurrentText
        {
            get
            {
                FireCoverageController fire = Controller != null ? Controller : GetComponent<FireCoverageController>();
                if (fire == null) return string.Empty;
                string text = Line("Пожарные", fire);
                if (Police != null) text += " • " + Line("Полиция", Police);
                return text;
            }
        }

        // Один сегмент строки: «Пожарные: N/M (Z%)».
        private static string Line(string label, FireCoverageController controller)
        {
            string text = label + ": " + controller.Covered + "/" + controller.Homes;
            return text + " (" + ToPercent(controller.Rate01) + "%)";
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
