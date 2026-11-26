using System;
using System.Collections.Generic;
using UnityEngine;
namespace Citybuilder
{
    // Панель решений (issue #167, ADR-020): кнопки «тушить/игнорировать» по образцу InfoPanelUpgrade.
    // GUI только в OnGUI, подписи — Localization.T, геометрия — HudLayout и UiClickGuard.
    public class DecisionPanel : MonoBehaviour
    {
        private const float PanelHeight = 76f;
        private const float ButtonHeight = 26f;
        private const float TitleHeight = 22f;
        [Header("References")]
        [Tooltip("Лента событий. Пусто = ищется в сцене.")] public EventLogView EventLog;
        [Tooltip("Казна для тушения. Пусто = ищется в сцене.")] public TreasuryView Treasury;

        private DecisionController controller;
        private DecisionController Controller { get { EnsureController(); return controller; } }
        // Хост подключает эффекты без Unity-событий и новой шины.
        public void Bind(CityEventLog log, Treasury treasury, Action<int> extinguish)
        {
            controller = new DecisionController(log, treasury, extinguish);
        }
        private void OnEnable() { EnsureController(); }
        private void EnsureController()
        {
            if (controller != null) return;
            if (EventLog == null) EventLog = FindFirstObjectByType<EventLogView>();
            if (Treasury == null) Treasury = FindFirstObjectByType<TreasuryView>();
            controller = new DecisionController(EventLog != null ? EventLog.Log : null,
                Treasury != null ? Treasury.Treasury : null, null);
        }
        // Возгорания приходят в ленту напрямую из FireTicker — решение предлагается по снимку.
        private void Update()
        {
            if (EventLog == null) EnsureController();
            if (EventLog == null) return;
            IReadOnlyList<CityEvent> snap = EventLog.Log.Snapshot();
            for (int i = 0; i < snap.Count; i++)
            {
                CityEvent e = snap[i];
                if (e.Subtype != EventSubtype.FireStart || e.Args.Length == 0) continue;
                Controller.Expected((int)e.Args[0], e.Tick);
            }
        }
        private void OnGUI()
        {
            int home = Controller.FirstPendingHome();
            if (home < 0) return;
            Rect host = HudLayout.EventLogRect(Screen.width, Screen.height);
            Rect box = new Rect(host.x + 8f, host.yMax - PanelHeight - 8f, host.width - 16f, PanelHeight);
            UiClickGuard.Set("decision_panel", box);
            BottomBarStyle.DrawBarBackground(box);
            HudText.Draw(box.x + 8f, box.y + 4f, Localization.T(EventMessageCatalog.DecisionTitle, home));
            DrawButtons(home, box);
        }
        private void DrawButtons(int home, Rect box)
        {
            float width = (box.width - 24f) * 0.5f;
            float y = box.y + TitleHeight;
            Rect fight = new Rect(box.x + 8f, y, width, ButtonHeight);
            Rect ignore = new Rect(box.x + 16f + width, y, width, ButtonHeight);
            if (GUI.Button(fight, Localization.T(EventMessageCatalog.DecisionSuppressButton, DecisionController.SuppressionCost)))
                controller.Resolve(home, true, 0);
            if (GUI.Button(ignore, Localization.T(EventMessageCatalog.DecisionIgnoreButton)))
                controller.Resolve(home, false, 0);
        }
    }
}
