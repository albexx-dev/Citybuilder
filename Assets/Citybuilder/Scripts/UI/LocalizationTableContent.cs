namespace Citybuilder
{
    // Хвост реестра локализации (issue #126): новые ключи вынесены в отдельную часть,
    // чтобы LocalizationTable.cs не превышал мягкий лимит 150 строк.
    public static partial class LocalizationTable
    {
        static LocalizationTable()
        {
            values["building_recycling_depot"] = new[] { "Перерабатывающий склад", "Recycling Depot" };
            values["decor_hedge"] = new[] { "Изгородь", "Hedge" };
            values["decor_flowerbed"] = new[] { "Клумба", "Flower bed" };
            values["decor_topiary"] = new[] { "Фигурное дерево", "Topiary" };
        }
    }
}
