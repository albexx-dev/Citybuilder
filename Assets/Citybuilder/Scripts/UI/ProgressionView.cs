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

        private void OnGUI()
        {
            if (Growth == null || Definitions == null || Definitions.Count == 0) return;
            BuildingDefinition next = UnlockPolicy.NextLocked(Definitions, Growth.GrownCount);
            // y=406: ниже пожарных (342) и кнопки налога (374 + 26).
            HudText.Draw(10, 406, BuildText(next));
        }

        // Текст строки: ближайший тип или признание, что открыто всё.
        private static string BuildText(BuildingDefinition next)
        {
            if (next == null) return "Все типы открыты";
            return "Дальше: " + next.DisplayName + " — " + next.UnlockBuildings + " зд.";
        }
    }
}
