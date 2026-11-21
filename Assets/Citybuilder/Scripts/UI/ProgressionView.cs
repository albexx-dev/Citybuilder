using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // HUD-строка «что откроется дальше»: ближайший закрытый тип по выросшим зданиям.
    // Без роста или без списка типов молча не рисует.
    public class ProgressionView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Рост города: источник GrownCount. Опционален: без него строка не рисуется.")]
        public SettlementGrowthController Growth;
        [Tooltip("Типы зданий с порогами UnlockBuildings. Пустой список = строка не рисуется.")]
        public List<BuildingDefinition> Definitions = new List<BuildingDefinition>();

        public string CurrentText
        {
            get
            {
                if (Growth == null || Definitions == null || Definitions.Count == 0) return string.Empty;
                return BuildText(UnlockPolicy.NextLocked(Definitions, Growth.GrownCount));
            }
        }

        // Текст строки: ближайший тип или признание, что открыто всё.
        private static string BuildText(BuildingDefinition next)
        {
            if (next == null) return Localization.T("progression_all");
            return Localization.T("progression_next", BuildingDisplayName.Of(next), next.UnlockBuildings);
        }
    }
}
