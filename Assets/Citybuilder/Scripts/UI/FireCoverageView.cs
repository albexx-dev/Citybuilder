using UnityEngine;

namespace Citybuilder
{
    // Показывает покрытие пожарных. V1: штатный OnGUI-лейбл под строкой мусора (y=310).
    // Нет контроллера — молча не рисует.
    public class FireCoverageView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер покрытия. Опционален: иначе ищется на этом же объекте.")]
        public FireCoverageController Controller;

        private void OnGUI()
        {
            FireCoverageController controller = Controller;
            if (controller == null) controller = GetComponent<FireCoverageController>();
            if (controller == null) return;
            string text = "Пожарные: " + controller.Covered + "/" + controller.Homes;
            text += " (" + ToPercent(controller.Rate01) + "%)";
            // y=342: ниже мусора (310+30+2), иначе налезает на лейблы.
            GUI.Label(new Rect(10, 342, 400, 30), text);
        }

        // Округлённый процент из ставки ядра 0..1.
        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
