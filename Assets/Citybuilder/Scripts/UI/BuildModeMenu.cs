using UnityEngine;

namespace Citybuilder
{
    // Меню режимов стройки: активен ровно один (или ни одного).
    // Кнопки переехали в BottomBarView (EPIC-13 шаг 2): здесь остаётся только переключение режимов и кисти.
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
        [Tooltip("Ручной декор: деревья без занятия клеток. Пусто = ищется на этом же объекте.")]
        public DecorPlacementController Decor;
        [Tooltip("Режим стройки декора доступен. Гасится настройкой DecorEnabled (EPIC-69 шаг 3).")]
        public bool DecorBuildEnabled = true;

        public enum Mode
        {
            Off,
            Houses,
            Roads,
            Zones,
            Growth,
            Decor
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
        public void SetMode(Mode mode)
        {
            if (mode == Mode.Decor && !DecorBuildEnabled) mode = Mode.Off; // настройка DecorEnabled
            current = mode;
            EnsureRefs();
            ApplyModeFlags(mode);
        }

        // Применение флагов режимов (issue #127): состав действий не меняется.
        private void ApplyModeFlags(Mode mode)
        {
            if (Houses != null)
            {
                Houses.enabled = (mode == Mode.Houses);
                Houses.PlacementActive = (mode == Mode.Houses);
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
            if (Decor != null)
            {
                Decor.enabled = (mode == Mode.Decor);
                Decor.placementActive = (mode == Mode.Decor);
            }
        }

        // Настройка DecorEnabled (EPIC-69 шаг 3): вход в декор запрещён, активный режим гасится.
        public void SetDecorBuildEnabled(bool enabled)
        {
            DecorBuildEnabled = enabled;
            if (!enabled && current == Mode.Decor) SetMode(Mode.Off);
        }

        // Кисть зон. None и null ZonePainter — игнор. Публичный для E2E-моста.
        public void SetBrush(ZoneType zone)
        {
            EnsureRefs();
            if (zone == ZoneType.None || Zones == null) return;
            Zones.Brush = zone;
        }

        // Ленивый добор недостающих ссылок на том же объекте.
        private void EnsureRefs()
        {
            if (Houses == null) Houses = GetComponent<BuildingPlacementController>();
            if (Roads == null) Roads = GetComponent<RoadPlacementController>();
            if (Zones == null) Zones = GetComponent<ZonePaintingController>();
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Decor == null) Decor = GetComponent<DecorPlacementController>();
        }
    }
}
