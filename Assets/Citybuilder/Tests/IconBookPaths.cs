using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Citybuilder
{
    // Набор иконок (EPIC-66): каждый ключ реестра обязан иметь PNG на диске,
    // иначе кнопка молча останется текстовой. Пути считаются от Application.dataPath.
    public class IconBookPaths
    {
        [Test]
        public void AllRegisteredKeys_HavePngOnDisk()
        {
            Assert.Greater(IconBook.Keys.Length, 0);
            foreach (string key in IconBook.Keys)
            {
                Assert.IsTrue(File.Exists(IconBook.AbsolutePath(key)), "Нет PNG для ключа: " + key);
            }
        }

        [Test]
        public void Keys_AreUnique()
        {
            HashSet<string> seen = new HashSet<string>();
            foreach (string key in IconBook.Keys)
            {
                Assert.IsTrue(seen.Add(key), "Дубликат ключа иконки: " + key);
            }
        }

        [Test]
        public void Paths_AreInsideIconsFolder()
        {
            foreach (string key in IconBook.Keys)
            {
                StringAssert.Contains("Art/Icons", IconBook.AbsolutePath(key));
                StringAssert.EndsWith(key + ".png", IconBook.AbsolutePath(key));
            }
        }
    }
}
