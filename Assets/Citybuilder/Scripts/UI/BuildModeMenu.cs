using UnityEngine;

namespace Citybuilder
{
    // Меню режимов стройки: активен ровно один (или ни одного). Только OnGUI.
    // Вешать на тот же объект, что контроллеры (Grid). Ссылки опциональны.
    public class BuildModeMenu : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Ручная стройка домов. Пусто = ищется на этом же объекте.")]
        public BuildingPlacementController Houses;
        [Tooltip("Стройка дорог drag-отрезками. Пусто = ищется на этом же объекте.")]
        public RoadPlacementController Roads;
        [Tooltip("Кисть зон. Пусто = ищется на этом же объекте.")]
        public ZonePaintingController Zones;
        [Tooltip("Автовырастание домов. Пусто = ищется на этом же объекте.")]
        public SettlementGrowthController Growth;

        public enum Mode
        {
            Off,
            Houses,
            Roads,
            Zones,
            Growth
        }

        private Mode current = Mode.Off;

        // Текущий режим. Для E2E-моста и проверок.
        public Mode CurrentMode
        {
            get { return current; }
        }

        private void Start()
        {
            SetMode(Mode.Off);
        }

        // Включает один режим, остальные гасит. Публичный для E2E-моста.
        // placementActive у Houses приватный — им управляем только через enabled.
        public void SetMode(Mode mode)
        {
            current = mode;
            EnsureRefs();
            if (Houses != null)
            {
                Houses.enabled = (mode == Mode.Houses);
            }
            if (Roads != null)
            {
                Roads.enabled = (mode == Mode.Roads);
                Roads.placementActive = (mode == Mode.Roads);
            }
            if (Zones != null)
            {
                Zones.enabled = (mode == Mode.Zones);
                Zones.placementActive = (mode == Mode.Zones);
            }
            if (Growth != null)
            {
                Growth.enabled = (mode == Mode.Growth);
                Growth.placementActive = (mode == Mode.Growth);
            }
        }

        // Кисть зон. None и null ZonePainter — игнор. Публичный для E2E-моста.
        public void SetBrush(ZoneType zone)
        {
            if (zone == ZoneType.None || Zones == null) return;
            Zones.Brush = zone;
        }

        // Ленивый добор недостающих ссылок на том же объекте.
        private void EnsureRefs()
        {
            if (Houses == null)
            {
                Houses = GetComponent<BuildingPlacementController>();
            }
            if (Roads == null)
            {
                Roads = GetComponent<RoadPlacementController>();
            }
            if (Zones == null)
            {
                Zones = GetComponent<ZonePaintingController>();
            }
            if (Growth == null)
            {
                Growth = GetComponent<SettlementGrowthController>();
            }
        }

        private void OnGUI()
        {
            EnsureRefs();
            Mode[] modes = { Mode.Off, Mode.Houses, Mode.Roads, Mode.Zones, Mode.Growth };
            string[] labels = { "Выкл [Esc]", "Дома [1]", "Дороги [2]", "Зоны [3]", "Рост [4]" };
            float width = 90f;
            float height = 30f;
            float margin = 10f;
            float x = Screen.width - width - margin;
            UiClickGuard.Set("modes", new Rect(x, margin, width, 5 * (height + 4f)));
            for (int i = 0; i < modes.Length; i++)
            {
                Rect rect = new Rect(x, margin + i * (height + 4f), width, height);
                if (DrawButton(rect, labels[i], current == modes[i]))
                {
                    SetMode(modes[i]);
                }
            }
            if (current == Mode.Zones)
            {
                UiClickGuard.Set("brush", new Rect(x - 2 * (width + 4f), margin + 5 * (height + 4f) + 8f, 3 * width + 8f, height));
                DrawBrushRow(x, margin + 5 * (height + 4f) + 8f, width, height);
            }
        }

        // Ряд кисти зон под кнопками режимов. Те же размеры, активная подсвечена.
        private void DrawBrushRow(float x, float y, float width, float height)
        {
            ZoneType[] brushes = { ZoneType.Residential, ZoneType.Commercial, ZoneType.Industrial };
            string[] labels = { "Жилая", "Коммерция", "Промзона" };
            for (int i = 0; i < brushes.Length; i++)
            {
                Rect rect = new Rect(x - (brushes.Length - 1 - i) * (width + 4f), y, width, height);
                bool highlighted = Zones != null && Zones.Brush == brushes[i];
                if (DrawButton(rect, labels[i], highlighted))
                {
                    SetBrush(brushes[i]);
                }
            }
        }

        // Одна кнопка: подсвечена жёлтым, цвет восстанавливается. Возвращает клик.
        private bool DrawButton(Rect rect, string label, bool highlighted)
        {
            Color saved = GUI.backgroundColor;
            if (highlighted)
            {
                GUI.backgroundColor = Color.yellow;
            }
            bool clicked = GUI.Button(rect, label);
            GUI.backgroundColor = saved;
            return clicked;
        }
    }
}
