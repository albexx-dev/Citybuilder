using UnityEngine;

namespace Citybuilder
{
    // Показывает баланс. V1: штатный OnGUI-лейбл без UI-пакетов.
    // Позже заменить на uGUI/UI Toolkit отдельным проходом.
    public class TreasuryView : MonoBehaviour
    {
        [Tooltip("Стартовый баланс города.")]
        public int StartingBalance = 1000;

        public Treasury Treasury { get; private set; }

        private void Awake()
        {
            Treasury = new Treasury(StartingBalance);
        }

        private void OnGUI()
        {
            if (Treasury == null)
            {
                return;
            }
            GUI.Label(new Rect(10, 10, 300, 30), "$ " + Treasury.Balance);
        }
    }
}
