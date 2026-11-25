using System.IO;

namespace Citybuilder
{
    // Перенос слота файлом (issue #118, ADR-016): тот же JSON v1–v6, валидация через SaveSlotStorage.
    public static class SlotTransfer
    {
        public const string ExportFileName = "slot-export.json";

        public static string DefaultPath(string baseDir)
        {
            return string.IsNullOrEmpty(baseDir) ? null : Path.Combine(baseDir, ExportFileName);
        }

        public static bool Export(string path, CitySnapshot snapshot)
        {
            if (string.IsNullOrEmpty(path) || !SaveSlotStorage.Validate(snapshot)) return false;
            return SaveSlotStorage.WriteSlot(path, snapshot);
        }

        public static CitySnapshot Import(string path)
        {
            CitySnapshot snapshot = SaveSlotStorage.ReadSlot(path);
            if (snapshot == null || !SaveSlotStorage.Validate(snapshot)) return null;
            SaveV6Snapshot.Normalize(snapshot);
            return snapshot;
        }
    }
}
