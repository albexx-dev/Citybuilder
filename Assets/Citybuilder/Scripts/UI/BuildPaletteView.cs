using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Палитра типов зданий в режиме домов. Только OnGUI, клик меняет тип у контроллера.
    public class BuildPaletteView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер стройки. Клик по кнопке вызывает SetDefinition.")]
        public BuildingPlacementController Placement;
        [Tooltip("Меню режимов: палитра видна только в режиме «Дома».")]
        public BuildModeMenu Menu;
        [Tooltip("Типы зданий для палитры. Пустой список = палитра не рисуется.")]
        public List<BuildingDefinition> Definitions = new List<BuildingDefinition>();
        [Tooltip("Население для разблокировки типов. Пусто = все доступны.")]
        public PopulationController Population;

        private const float Width = 130f;
        private const float Height = 30f;
        private const float Step = 34f;
        private const float Top = 188f;

        private void OnGUI()
        {
            if (!CanDraw())
            {
                UiClickGuard.Set("palette", new Rect());
                return;
            }
            float x = Screen.width - Width - 10f;
            UiClickGuard.Set("palette", new Rect(x, Top, Width, Definitions.Count * Step));
            for (int i = 0; i < Definitions.Count; i++)
            {
                DrawDefinition(x, Top + i * Step, Definitions[i]);
            }
        }

        private bool CanDraw()
        {
            if (Placement == null || Menu == null) return false;
            if (Menu.CurrentMode != BuildModeMenu.Mode.Houses) return false;
            return Definitions != null && Definitions.Count > 0;
        }

        private void DrawDefinition(float x, float y, BuildingDefinition def)
        {
            if (def == null) return;
            Rect rect = new Rect(x, y, Width, Height);
            int residents = Residents;
            if (!UnlockPolicy.IsUnlocked(def, residents))
            {
                DrawLocked(rect, def.DisplayName + " — " + UnlockPolicy.Remaining(def, residents) + " жит.");
                return;
            }
            string label = def.DisplayName + " " + def.ConstructionCost;
            if (DrawButton(rect, label, Placement.Definition == def))
            {
                Placement.SetDefinition(def);
            }
        }

        // Нет населения в сцене — разблокировки нет (старое поведение).
        private int Residents
        {
            get { return Population == null ? int.MaxValue : Population.Population.TotalResidents; }
        }

        // Заблокированная кнопка серая: клик не проходит, вместо цены — нехватка жителей.
        private void DrawLocked(Rect rect, string label)
        {
            bool saved = GUI.enabled;
            GUI.enabled = false;
            GUI.Button(rect, label);
            GUI.enabled = saved;
        }

        // Одна кнопка: активный тип подсвечен жёлтым, цвет восстанавливается.
        private bool DrawButton(Rect rect, string label, bool highlighted)
        {
            Color saved = GUI.backgroundColor;
            if (highlighted)
            {
                GUI.backgroundColor = Color.yellow;
            }
            bool clicked = GUI.Button(rect, label);
            GUI.backgroundColor = saved;
            return clicked;
        }
    }
}
