using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Окно ленты по кнопке «Лента»/клавише L (EPIC-09 шаг 2, ADR-007, EPIC-19): в закрытом виде ничего не рисует; кеш строк пересобирается при открытии и раз в полсекунды.
    public class EventLogView : MonoBehaviour
    {
        private const float RefreshIntervalSec = 0.5f;
        private const float LineHeight = 24f;

        private readonly CityEventLog log = new CityEventLog();
        private readonly SoundCooldownPolicy popCooldown = new SoundCooldownPolicy();
        private readonly List<string> cachedLines = new List<string>();
        private bool isOpen;
        private Vector2 scroll;
        private float nextRefreshTime;

        public CityEventLog Log
        {
            get { return log; }
        }

        public bool IsOpen
        {
            get { return isOpen; }
        }

        // Сборка строки ленты только во view: домен хранит ключ и аргументы (ADR-007/013).
        public static string FormatLine(CityEvent e)
        {
            int day = e.Tick / SimulationTime.TicksPerDay;
            int hour = e.Tick % SimulationTime.TicksPerDay;
            return Localization.T("event_log_prefix", day, hour) + Localization.T(e.MessageKey, e.Args);
        }

        // Смена языка пересобирает кеш уже существующих записей, без вызовов Add (ADR-013).
        private void OnLanguageChanged()
        {
            if (isOpen) RebuildCache();
        }

        private void OnEnable()
        {
            Localization.LanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        public void Toggle()
        {
            SetOpen(!isOpen);
        }

        public void SetOpen(bool open)
        {
            if (isOpen != open) AudioController.PlaySwooshGlobal();
            isOpen = open;
            if (isOpen)
            {
                RebuildCache();
            }
            else
            {
                UiClickGuard.Set("event_log", new Rect());
            }
        }

        public void Record(EventKind kind, string messageKey, object[] args, int tick)
        {
            if (log.Add(kind, messageKey, args, tick))
            {
                if (popCooldown.ShouldPlayDeficit(Time.unscaledTime)) AudioController.PlayPopGlobal();
                if (isOpen) RebuildCache();
            }
        }

        public void ClearLog()
        {
            log.Clear();
            popCooldown.Reset();
            cachedLines.Clear();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L) && BuildModeHotkeys.CanProcessPanelHotkeys())
            {
                if (!isOpen) CloseSiblingPanels();
                Toggle();
            }
            if (isOpen && Time.unscaledTime >= nextRefreshTime) RebuildCache();
        }

        // Хоткей открывает ленту поверх других: инфоменю и задачи закрываются (EPIC-09/14/48).
        private void CloseSiblingPanels()
        {
            InfoMenuView menu = FindFirstObjectByType<InfoMenuView>();
            QuestLogView quests = FindFirstObjectByType<QuestLogView>();
            if (menu != null) menu.SetOpen(false);
            SettingsView.CloseIfOpen();
            if (quests != null) quests.SetOpen(false);
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            Rect panel = HudLayout.EventLogRect(Screen.width, Screen.height);
            UiClickGuard.Set("event_log", panel);
            BottomBarStyle.DrawBarBackground(panel);
            Rect inner = new Rect(panel.x + 10f, panel.y + 8f, panel.width - 20f, panel.height - 16f);
            float contentHeight = Mathf.Max(inner.height, (cachedLines.Count + 2) * LineHeight + 8f);
            Rect view = new Rect(0f, 0f, inner.width - 20f, contentHeight);
            scroll = GUI.BeginScrollView(inner, scroll, view);
            DrawLines();
            GUI.EndScrollView();
        }

        private void RebuildCache()
        {
            nextRefreshTime = Time.unscaledTime + RefreshIntervalSec;
            cachedLines.Clear();
            IReadOnlyList<CityEvent> snap = log.Snapshot();
            for (int i = snap.Count - 1; i >= 0; i--)
            {
                cachedLines.Add(FormatLine(snap[i]));
            }
        }

        private void DrawLines()
        {
            float y = 4f;
            HudText.Draw(4f, y, Localization.T("event_log_title"));
            y += LineHeight;
            if (cachedLines.Count == 0)
            {
                HudText.Draw(12f, y, Localization.T("event_log_empty"));
                return;
            }
            for (int i = 0; i < cachedLines.Count; i++)
            {
                HudText.Draw(12f, y, cachedLines[i]);
                y += LineHeight;
            }
        }
    }
}
