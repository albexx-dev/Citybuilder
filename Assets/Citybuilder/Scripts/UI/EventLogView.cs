using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Окно событийной ленты города по кнопке «Лента» или клавише L (EPIC-09 шаг 2, ADR-007, EPIC-19).
    // В закрытом состоянии ничего не рисует; снимок строк пересобирается при открытии и раз в полсекунды.
    public class EventLogView : MonoBehaviour
    {
        private const float RefreshIntervalSec = 0.5f;
        private const float LineHeight = 24f;

        private readonly CityEventLog log = new CityEventLog();
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

        public void Record(EventKind kind, string message, int tick)
        {
            if (log.Add(kind, message, tick))
            {
                AudioController.PlayPopGlobal();
                if (isOpen) RebuildCache();
            }
        }

        public void ClearLog()
        {
            log.Clear();
            cachedLines.Clear();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L)) Toggle();
            if (isOpen && Time.unscaledTime >= nextRefreshTime) RebuildCache();
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
                cachedLines.Add(snap[i].FormatLine());
            }
        }

        private void DrawLines()
        {
            float y = 4f;
            HudText.Draw(4f, y, "=== События города ===");
            y += LineHeight;
            if (cachedLines.Count == 0)
            {
                HudText.Draw(12f, y, "Событий пока нет.");
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
