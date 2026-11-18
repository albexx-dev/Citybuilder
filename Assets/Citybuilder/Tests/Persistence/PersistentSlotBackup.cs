using System.Collections.Generic;
using System.IO;

namespace Citybuilder
{
    // Страж чужих сейвов для тестов, которые пишут прямо в Application.persistentDataPath (EPIC-64 шаг 3).
    // Take() запоминает содержимое файлов, которые тест может перезаписать, Restore() возвращает его
    // байт-в-байт, а файлы, которых до теста не было, удаляются. Иначе прогон EditMode-тестов в Editor
    // затирал бы реальные slot*.json и slots.json разработчика. Список имён берётся из SaveSlotIndex.
    public class PersistentSlotBackup
    {
        private readonly string dir;
        private readonly List<string> kept = new List<string>();
        private readonly List<byte[]> contents = new List<byte[]>();

        public PersistentSlotBackup(string baseDir) { dir = baseDir; }

        public void Take()
        {
            kept.Clear(); contents.Clear();
            string[] guarded = GuardedNames();
            for (int i = 0; i < guarded.Length; i++)
            {
                string path = Path.Combine(dir, guarded[i]);
                if (!File.Exists(path)) continue;
                kept.Add(guarded[i]); contents.Add(File.ReadAllBytes(path));
            }
        }

        // Вызывается из TearDown, а NUnit выполняет его и при падении теста.
        public void Restore()
        {
            string[] guarded = GuardedNames();
            for (int i = 0; i < guarded.Length; i++)
            {
                string path = Path.Combine(dir, guarded[i]);
                int index = kept.IndexOf(guarded[i]);
                if (index < 0) { if (File.Exists(path)) File.Delete(path); continue; }
                File.WriteAllBytes(path, contents[index]);
            }
        }

        // Ручные слоты 0..N, файл автосейва и индекс слотов — всё, что записывают SaveController и LoadController.
        private static string[] GuardedNames()
        {
            List<string> names = new List<string>();
            for (int slot = 0; slot < SaveSlotIndex.ManualSlotCount; slot++) names.Add(SaveSlotIndex.FileNameForSlot(slot));
            names.Add(SaveSlotIndex.FileNameForSlot(SaveSlotIndex.AutosaveSlot));
            names.Add(SaveSlotIndex.IndexFileName);
            return names.ToArray();
        }
    }
}
