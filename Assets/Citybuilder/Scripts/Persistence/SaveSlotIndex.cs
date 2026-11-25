using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Citybuilder
{
    // Индекс слотов сохранений slots.json (EPIC-11 шаг 1, ADR-008): метаданные 3 слотов + autosave.json.
    [Serializable]
    public class SaveSlotIndex
    {
        public const int ManualSlotCount = 3;
        public const int AutosaveSlot = -1;
        public const string IndexFileName = "slots.json";
        public const string AutosaveFileName = "autosave.json";
        public const int MaxNameLength = 32;

        [Serializable]
        public class SlotMeta
        {
            public string file;
            public string name;
            public long savedAtTick;
            public string savedAtReal;
        }

        public List<SlotMeta> slots = new List<SlotMeta>();

        public static string FileNameForSlot(int slot)
        {
            if (slot == AutosaveSlot) return AutosaveFileName;
            if (slot < 0 || slot >= ManualSlotCount) return null;
            return "slot" + slot + ".json";
        }

        public static SaveSlotIndex Read(string baseDir)
        {
            if (string.IsNullOrEmpty(baseDir)) return new SaveSlotIndex();
            string path = Path.Combine(baseDir, IndexFileName);
            if (!File.Exists(path)) return new SaveSlotIndex();
            try
            {
                SaveSlotIndex index = JsonUtility.FromJson<SaveSlotIndex>(File.ReadAllText(path));
                if (index == null || index.slots == null) return new SaveSlotIndex();
                for (int i = index.slots.Count - 1; i >= 0; i--)
                {
                    if (index.slots[i] == null || string.IsNullOrEmpty(index.slots[i].file)) index.slots.RemoveAt(i);
                }
                return index;
            }
            catch (Exception)
            {
                return new SaveSlotIndex();
            }
        }

        public static void UpdateEntry(string baseDir, int slot, string slotName, long tick)
        {
            string file = FileNameForSlot(slot);
            if (string.IsNullOrEmpty(baseDir) || string.IsNullOrEmpty(file)) return;
            SaveSlotIndex index = Read(baseDir);
            SlotMeta entry = FindByFile(index, file);
            if (entry == null)
            {
                entry = new SlotMeta { file = file };
                index.slots.Add(entry);
            }
            entry.name = NormalizeName(slotName, slot);
            entry.savedAtTick = tick;
            entry.savedAtReal = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
            Write(baseDir, index);
        }

        // Переименование существующего слота: меняет только name, не трогает снимок и метки времени.
        public static bool Rename(string baseDir, int slot, string requested)
        {
            if (!IsValidName(requested)) return false;
            string file = FileNameForSlot(slot);
            if (string.IsNullOrEmpty(baseDir) || string.IsNullOrEmpty(file)) return false;
            SaveSlotIndex index = Read(baseDir);
            SlotMeta entry = FindByFile(index, file);
            if (entry == null)
            {
                CitySnapshot snap = SaveSlotStorage.ReadSlot(Path.Combine(baseDir, file));
                if (snap == null || !SaveSlotStorage.Validate(snap)) return false;
                entry = new SlotMeta { file = file, savedAtTick = snap.TickCount };
                index.slots.Add(entry);
            }
            entry.name = requested.Trim();
            return Write(baseDir, index);
        }

        public static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            string trimmed = name.Trim();
            return trimmed.Length > 0 && trimmed.Length <= MaxNameLength;
        }

        public static string NormalizeName(string name, int slot)
        {
            string trimmed = name == null ? string.Empty : name.Trim();
            if (trimmed.Length == 0) return DefaultName(slot);
            return trimmed.Length <= MaxNameLength ? trimmed : trimmed.Substring(0, MaxNameLength);
        }

        public static SlotMeta GetMeta(string baseDir, int slot)
        {
            string file = FileNameForSlot(slot);
            if (string.IsNullOrEmpty(baseDir) || string.IsNullOrEmpty(file)) return null;
            SaveSlotIndex index = Read(baseDir);
            SlotMeta meta = FindByFile(index, file);
            if (meta != null) return meta;
            string slotPath = Path.Combine(baseDir, file);
            CitySnapshot snap = SaveSlotStorage.ReadSlot(slotPath);
            if (snap == null || !SaveSlotStorage.Validate(snap)) return null;
            UpdateEntry(baseDir, slot, DefaultName(slot), snap.TickCount);
            return FindByFile(Read(baseDir), file);
        }

        public static string DefaultName(int slot)
        {
            return slot == AutosaveSlot ? "Автосейв" : "Слот " + (slot + 1);
        }

        private static bool Write(string baseDir, SaveSlotIndex index)
        {
            try
            {
                File.WriteAllText(Path.Combine(baseDir, IndexFileName), JsonUtility.ToJson(index));
                return true;
            }
            catch (Exception) { return false; }
        }

        private static SlotMeta FindByFile(SaveSlotIndex index, string file)
        {
            if (index == null || index.slots == null) return null;
            for (int i = 0; i < index.slots.Count; i++)
            {
                if (index.slots[i] != null && index.slots[i].file == file) return index.slots[i];
            }
            return null;
        }
    }
}
