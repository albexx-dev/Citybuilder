using System;

namespace Citybuilder
{
    // Хвост класса GameSettings (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class GameSettings
    {
        private void Set(ref bool field, bool value)
        {
            if (field == value) return;
            field = value;
            if (Changed != null) Changed();
        }
    }
}
