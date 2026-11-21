namespace Citybuilder
{
    // Имя здания на текущем языке: ключ ассета через реестр, фолбэк — DisplayName, затем Id.
    public static class BuildingDisplayName
    {
        public static string Of(BuildingDefinition def)
        {
            if (def == null) return string.Empty;
            return Resolve(def.DisplayNameKey, def.DisplayName, def.Id);
        }

        public static string Of(BuildingInfo info)
        {
            return Resolve(info.DisplayNameKey, info.DisplayName, info.Id);
        }

        public static string Of(DecorDefinition def)
        {
            if (def == null) return string.Empty;
            return Resolve(def.DisplayNameKey, def.DisplayName, def.Id);
        }

        private static string Resolve(string key, string fallback, string id)
        {
            if (!string.IsNullOrEmpty(key) && Localization.Has(key)) return Localization.T(key);
            if (!string.IsNullOrEmpty(fallback)) return fallback;
            return id ?? string.Empty;
        }
    }
}
