using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Ручные здания игрока: состояние и вид хранятся парами, индексы общие.
    public class ManualBuildingsStore
    {
        private readonly List<BuildingState> states = new List<BuildingState>();
        private readonly List<GameObject> views = new List<GameObject>();

        // Копия построенного: нужна покрытию, инфраструктуре и сохранениям.
        public List<BuildingState> Placed
        {
            get
            {
                List<BuildingState> copy = new List<BuildingState>();
                for (int i = 0; i < states.Count; i++)
                {
                    if (states[i] != null) copy.Add(states[i]);
                }
                return copy;
            }
        }

        // Новая постройка: состояние и его вид добавляются вместе.
        public void Add(BuildingState state, GameObject view)
        {
            if (state == null) return;
            states.Add(state);
            views.Add(view);
        }

        // Вид без состояния: пара нужна, чтобы индексы списков не разъехались.
        public void AddView(GameObject view)
        {
            if (view == null) return;
            states.Add(null);
            views.Add(view);
        }

        // Снос одной постройки: вид уничтожается, пара убирается.
        public bool Demolish(BuildingState state)
        {
            int index = IndexOf(state);
            if (index < 0) return false;
            if (views[index] != null) Object.Destroy(views[index]);
            states.RemoveAt(index);
            views.RemoveAt(index);
            return true;
        }

        // Снос всех ручных: клетки освобождаются, виды уничтожаются, списки пустеют.
        public void Clear(BuildingSystem system, GridVisualizer grid)
        {
            if (system != null)
            {
                for (int i = 0; i < states.Count; i++)
                {
                    if (states[i] != null) system.Demolish(states[i]);
                }
            }
            for (int i = 0; i < views.Count; i++)
            {
                if (views[i] != null) Object.Destroy(views[i]);
            }
            states.Clear();
            views.Clear();
            if (grid != null) grid.RefreshOccupied();
        }

        // Состояние под клеткой: нужно сносу по правой кнопке.
        public BuildingState FindAt(GridPosition cell)
        {
            for (int i = 0; i < states.Count; i++)
            {
                BuildingState state = states[i];
                if (state == null || state.Cells == null) continue;
                for (int c = 0; c < state.Cells.Count; c++)
                {
                    if (state.Cells[c] == cell) return state;
                }
            }
            return null;
        }

        private int IndexOf(BuildingState state)
        {
            if (state == null) return -1;
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] == state) return i;
            }
            return -1;
        }
    }
}
