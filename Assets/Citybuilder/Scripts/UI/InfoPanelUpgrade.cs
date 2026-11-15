using UnityEngine;

namespace Citybuilder
{
    // Кнопка «Улучшить» в инфопанели (EPIC-51): цена от уровня, клик — списание из казны
    // и пересчёт покрытия через UpgradeController. Жилые дома кнопки не получают:
    // Upgradeable в снимке BuildingInfoQuery считается только для станций и служб.
    public static class InfoPanelUpgrade
    {
        public const float ButtonHeight = 26f;
        public const float ButtonMargin = 8f;

        // Прямоугольник кнопки в координатах панели: внизу, во всю ширину с отступами.
        public static Rect ButtonRect(Rect panel)
        {
            return new Rect(
                panel.x + ButtonMargin,
                panel.yMax - ButtonHeight - ButtonMargin,
                panel.width - 2f * ButtonMargin,
                ButtonHeight);
        }

        // Нарисовать кнопку и вернуть true по клику. Кнопки нет — false, ничего не рисует.
        public static bool Draw(Rect panel, BuildingInfo info)
        {
            if (!info.Upgradeable) return false;
            return GUI.Button(ButtonRect(panel), "Улучшить — $" + info.UpgradePrice);
        }

        // Попытка апгрейда выбранного здания: state по origin, казна из роста,
        // пересчёт снимка покрытия колбэком UpgradeController.
        public static UpgradeController.Result TryUpgradeCurrent(
            BuildingInfo current,
            SettlementGrowthController growth,
            BuildingPlacementController placement,
            ServiceCoverageController services)
        {
            if (growth == null || placement == null) return UpgradeController.Result.Fail(UpgradeController.Failure.NotUpgradeable);
            BuildingState state = BuildingInfoQuery.FindStateAt(current.Origin, growth.Homes, placement.Placed);
            Treasury treasury = growth.Treasury != null ? growth.Treasury.Treasury : null;
            UpgradeController controller = new UpgradeController(
                delegate { if (services != null) services.RefreshNow(); });
            return controller.TryUpgrade(state, treasury);
        }
    }
}
