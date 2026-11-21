using UnityEngine;

namespace Citybuilder
{
    // Кнопка RU/EN в верхнем ряду HUD (EPIC-72 шаг 1): четвёртая строка под часами.
    // Не в нижней панели: у неё свои контракты в параллельных ветках.
    public class LanguageToggleView : MonoBehaviour
    {
        private const string GuardKey = "language_toggle";
        private const float ButtonX = 10f;
        private const float ButtonY = 70f; // казна 10, население 30, часы 50 — кнопка ниже последней строки.
        private const float ButtonWidth = 72f;
        private const float ButtonHeight = 22f;

        [Header("Appearance")]
        [Tooltip("Подпись кнопки переключателя языка.")]
        public string Label = "RU/EN";

        private void Awake()
        {
            // Выбор языка из прошлого запуска подтягивается при старте сцены.
            Localization.LoadSaved();
        }

        private void OnGUI()
        {
            Rect rect = new Rect(ButtonX, ButtonY, ButtonWidth, ButtonHeight);
            UiClickGuard.Set(GuardKey, rect);
            GUIContent content = new GUIContent(Label, "Переключить язык / Toggle language");
            if (GUI.Button(rect, content))
            {
                Toggle();
            }
        }

        // Публично для E2E-моста: один вызов = смена языка с сохранением в PlayerPrefs.
        public void Toggle()
        {
            Localization.Language = Localization.Language == Language.En ? Language.Ru : Language.En;
        }
    }
}
