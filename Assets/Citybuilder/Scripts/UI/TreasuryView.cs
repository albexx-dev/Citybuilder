using UnityEngine;

namespace Citybuilder
{
    // Показывает баланс. V1: штатный OnGUI-лейбл без UI-пакетов.
    // Позже заменить на uGUI/UI Toolkit отдельным проходом.
    public class TreasuryView : MonoBehaviour
    {
        [Tooltip("Стартовый баланс города.")]
        public int StartingBalance = 1000;

        private Treasury treasury;

        // Ленивый доступ: после перезагрузки домена казна пересоздаётся.
        // Баланс при этом сбрасывается (чинятся сохранениями, не V1).
        public Treasury Treasury
        {
            get
            {
                if (treasury == null)
                {
                    treasury = new Treasury(StartingBalance);
                }
                return treasury;
            }
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
