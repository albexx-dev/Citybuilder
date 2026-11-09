using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Данные палитры типов зданий для режима домов.
    // Отрисовка кнопок переехала над нижнюю панель в BottomBarView (EPIC-13 шаг 2).
    public class BuildPaletteView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер стройки. Клик по кнопке вызывает SetDefinition.")]
        public BuildingPlacementController Placement;
        [Tooltip("Меню режимов: палитра видна только в режиме «Дома».")]
        public BuildModeMenu Menu;
        [Tooltip("Типы зданий для палитры. Пустой список = палитра не рисуется.")]
        public List<BuildingDefinition> Definitions = new List<BuildingDefinition>();
        [Tooltip("Рост для разблокировки типов. Пусто = все доступны.")]
        public SettlementGrowthController Growth;
    }
}
