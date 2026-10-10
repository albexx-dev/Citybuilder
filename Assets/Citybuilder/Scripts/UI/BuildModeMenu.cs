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
            string[] labels = { "Выкл", "Дома", "Дороги", "Зоны", "Рост" };
            float width = 90f;
            float height = 30f;
            float margin = 10f;
            float x = Screen.width - width - margin;
            for (int i = 0; i < modes.Length; i++)
            {
                Rect rect = new Rect(x, margin + i * (height + 4f), width, height);
                DrawModeButton(rect, modes[i], labels[i]);
            }
        }

        // Одна кнопка: активная подсвечена жёлтым, цвет восстанавливается.
        private void DrawModeButton(Rect rect, Mode mode, string label)
        {
            Color saved = GUI.backgroundColor;
            if (current == mode)
            {
                GUI.backgroundColor = Color.yellow;
            }
            if (GUI.Button(rect, label))
            {
                SetMode(mode);
            }
            GUI.backgroundColor = saved;
        }
    }
}
