using UnityEngine;

namespace Citybuilder
{
    // Показывает покрытие всех четырёх служб города (EPIC-01/EPIC-02): пожарные, полиция, больницы и досуг.
    public class FireCoverageView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер покрытия служб. Опционален: иначе ищется на этом же объекте.")]
        public ServiceCoverageController Controller;

        [Tooltip("Второй контроллер — полиция (опционально: иначе читается из того же Controller).")]
        public ServiceCoverageController Police;

        // Строка пожарных и полиции.
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

        // Строка больниц и досуга (EPIC-02 шаг 2) из того же снимка ServiceBuildingSystem.
        public string HealthLeisureText
        {
            get
            {
                ServiceCoverageController primary = Controller != null ? Controller : GetComponent<ServiceCoverageController>();
                if (primary == null) return string.Empty;
                return KindLine("Больницы", primary, ServiceKind.Health)
                    + " • " + KindLine("Досуг", primary, ServiceKind.Leisure);
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
