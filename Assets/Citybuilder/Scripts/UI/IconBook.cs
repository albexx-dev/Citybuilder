using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Citybuilder
{
    // Реестр UI-иконок (EPIC-66 шаг 1): ключ -> Texture2D, PNG лежит в Assets/Citybuilder/Art/Icons.
    // Загрузка с диска через File.ReadAllBytes + LoadImage: работает и в EditMode, и в Play, и в билде —
    // AssetDatabase в билде недоступен, а IconBook статический, поэтому Inspector-ссылки не подходят
    // (модели же грузятся через Inspector, см. VehicleAgentView).
    public static class IconBook
    {
        private const string IconsFolder = "Art/Icons";

        // Все ключи набора: кнопки нижней панели, кисти зон и задел служб/настроек.
        public static readonly string[] Keys =
        {
            "house", "shop", "factory", "road", "zones", "growth", "decor", "off",
            "pause", "play", "fast", "stats", "feed", "quests", "settings", "garbage"
        };

        private static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        // Иконка по ключу; null, если PNG нет (кнопка рисуется как текстовая) или ключ пуст.
        public static Texture2D Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (cache.ContainsKey(key)) return cache[key];
            Texture2D texture = LoadFromDisk(key);
            cache[key] = texture;
            return texture;
        }

        // Абсолютный путь PNG от Application.dataPath: тесты File.Exists и загрузка с диска.
        public static string AbsolutePath(string key)
        {
            return Path.Combine(Application.dataPath, "Citybuilder/" + IconsFolder + "/" + key + ".png");
        }

        // PNG с диска: LoadImage сам подбирает размер и формат; неудача или отсутствие файла = null.
        private static Texture2D LoadFromDisk(string key)
        {
            string path = AbsolutePath(key);
            if (!File.Exists(path)) return null;
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(File.ReadAllBytes(path)))
            {
                Object.DestroyImmediate(texture);
                return null;
            }
            texture.name = key;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            return texture;
        }
    }
}
