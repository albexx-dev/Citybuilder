using UnityEngine;

namespace Citybuilder
{
    // Хоткеи меню режимов: Esc гасит, 1-4 включают режимы. Только клавиатура.
    // Вешать на тот же объект, что меню (Grid). Ссылка опциональна.
    public class BuildModeHotkeys : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Меню режимов. Пусто = ищется на этом же объекте.")]
        public BuildModeMenu Menu;

        private void Update()
        {
            BuildModeMenu menu = ResolveMenu();
            if (menu == null)
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                menu.SetMode(BuildModeMenu.Mode.Off);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                menu.SetMode(BuildModeMenu.Mode.Houses);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                menu.SetMode(BuildModeMenu.Mode.Roads);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                menu.SetMode(BuildModeMenu.Mode.Zones);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                menu.SetMode(BuildModeMenu.Mode.Growth);
            }
        }

        // Ленивый добор меню на том же объекте. Нет меню — ошибка и стоп.
        private BuildModeMenu ResolveMenu()
        {
            if (Menu != null)
            {
                return Menu;
            }
            BuildModeMenu found = GetComponent<BuildModeMenu>();
            if (found == null)
            {
                Debug.LogError("BuildModeHotkeys: задайте Menu в Inspector.");
                enabled = false;
            }
            Menu = found;
            return found;
        }
    }
}
