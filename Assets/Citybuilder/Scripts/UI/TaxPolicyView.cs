using UnityEngine;

namespace Citybuilder
{
    // Кнопка налоговой политики в левом HUD, ниже строк пожарных. Только OnGUI.
    // Вешать на объект HUD (Grid). Ссылок не требует: политика живёт в памяти.
    public class TaxPolicyView : MonoBehaviour
    {
        // Не сериализуется: после загрузки снова обычный налог (долг на сохранения).
        private TaxPolicy policy;

        // Ленивое создание: сцена не требует ссылок, по умолчанию обычный налог.
        public TaxPolicy Policy
        {
            get
            {
                if (policy == null)
                {
                    policy = new TaxPolicy();
                }
                return policy;
            }
        }

        // Левая колонка HUD: y=374 — ниже пожарных (342 + 30 + зазор 2).
        private void OnGUI()
        {
            Rect rect = new Rect(10f, 374f, 240f, 26f);
            UiClickGuard.Set("tax", rect);
            if (GUI.Button(rect, Policy.Label))
            {
                Policy.Next();
            }
        }
    }
}
