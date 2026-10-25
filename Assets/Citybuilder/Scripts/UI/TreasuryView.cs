using UnityEngine;

namespace Citybuilder
{
    // Показывает баланс. V1: штатный OnGUI-лейбл без UI-пакетов.
    // Позже заменить на uGUI/UI Toolkit отдельным проходом.
    public class TreasuryView : MonoBehaviour
    {
        [Tooltip("Стартовый баланс города.")]
        public int StartingBalance = 1500;

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

        // Сброс баланса из сохранения: казна пересоздаётся как после reload.
        public void ResetBalance(int balance)
        {
            treasury = new Treasury(balance);
        }

        private void OnGUI()
        {
            if (Treasury == null)
            {
                return;
            }
            HudText.Draw(10, 10, "$ " + Treasury.Balance);
        }
    }
}
