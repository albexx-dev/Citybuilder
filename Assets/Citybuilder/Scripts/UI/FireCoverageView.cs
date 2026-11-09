using UnityEngine;

namespace Citybuilder
{
    // Показывает покрытие служб. В EPIC-01 шаг 2 читает оба вида из одного ServiceCoverageController (или второго контроллера).
    public class FireCoverageView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер покрытия служб. Опционален: иначе ищется на этом же объекте.")]
        public ServiceCoverageController Controller;

        [Tooltip("Второй контроллер — полиция (опционально: иначе читается из того же Controller).")]
        public ServiceCoverageController Police;

        public string CurrentText
        {
            get
            {
                ServiceCoverageController primary = Controller != null ? Controller : GetComponent<ServiceCoverageController>();
                if (primary == null) return string.Empty;
                string text = KindLine("Пожарные", primary, ServiceKind.Fire);
                ServiceCoverageController police = Police != null ? Police : primary;
                text += " • " + KindLine("Полиция", police, ServiceKind.Police);
                return text;
            }
        }

        private static string KindLine(string label, ServiceCoverageController controller, ServiceKind kind)
        {
            string text = label + ": " + controller.CoveredOf(kind) + "/" + controller.Homes;
            return text + " (" + ToPercent(controller.RateOf(kind)) + "%)";
        }

        private static int ToPercent(float rate)
        {
            return Mathf.RoundToInt(rate * 100f);
        }
    }
}
