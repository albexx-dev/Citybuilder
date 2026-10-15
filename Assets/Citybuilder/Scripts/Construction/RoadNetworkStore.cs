using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Состояние дорожной сети: система, список и вид. Ввод живёт в RoadPlacementController.
    public class RoadNetworkStore : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязателен: даёт Grid для RoadSystem.")]
        public GridVisualizer GridVisualizer;
        [Tooltip("Тип дороги. Цена клетки берётся отсюда.")]
        public RoadDefinition Definition;
        [Tooltip("Вид дорог. Пусто = создастся на этом же объекте.")]
        public RoadView View;

        private RoadSystem system;
        private readonly List<RoadState> states = new List<RoadState>();

        // Копия построенных дорог для сохранений. Отдаём копию, а не сам список.
        public List<RoadState> Roads
        {
            get
            {
                return new List<RoadState>(states);
            }
        }

        // Готовит систему по GridVisualizer.Grid, добирает View. Без GridVisualizer гаснет.
        private bool EnsureReady()
        {
            if (GridVisualizer == null)
            {
                Debug.LogError("RoadNetworkStore: задайте GridVisualizer в Inspector.");
                enabled = false;
                return false;
            }
            if (View == null)
            {
                View = GetComponent<RoadView>();
                if (View == null)
                {
                    View = gameObject.AddComponent<RoadView>();
                }
            }
            if (system == null)
            {
                system = new RoadSystem(GridVisualizer.Grid);
            }
            return true;
        }

        // Класть одну клетку: своя — пропуск, иначе system.Place и учёт. Возвращает state/null.
        public RoadState Place(GridPosition cell)
        {
            if (!EnsureReady() || system == null) return null;
            if (IsOwnRoadCell(cell)) return null;
            RoadState state = system.Place(cell);
            if (state == null) return null;
            states.Add(state);
            return state;
        }

        // Найти построенное на клетке. Нет — null.
        public RoadState FindStateAt(GridPosition cell)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].Cell == cell)
                {
                    return states[i];
                }
            }
            return null;
        }

        // Снести построенное: снятие с сетки, учёт и RefreshOccupied. Вид обновляет вызывающий.
        public bool DemolishState(RoadState state)
        {
            if (!EnsureReady() || system == null || state == null) return false;
            if (!system.Demolish(state)) return false;
            states.Remove(state);
            GridVisualizer.RefreshOccupied();
            return true;
        }

        // Восстановить дорогу из сохранения: без оплаты, список и вид обновляются.
        public RoadState RestoreRoad(GridPosition cell)
        {
            if (!EnsureReady() || system == null) return null;
            if (IsOwnRoadCell(cell)) return null;
            RoadState state = system.Place(cell);
            if (state == null) return null;
            states.Add(state);
            RefreshView();
            GridVisualizer.RefreshOccupied();
            return state;
        }

        // Снести все дороги (первый шаг загрузки слота).
        public void ClearRoads()
        {
            if (!EnsureReady() || system == null) return;
            for (int i = states.Count - 1; i >= 0; i--) system.Demolish(states[i]);
            states.Clear();
            RefreshView();
            GridVisualizer.RefreshOccupied();
        }

        // Свободна ли клетка для стройки (не занята сеткой).
        public bool CanPlace(GridPosition cell)
        {
            if (!EnsureReady() || system == null) return false;
            return system.CanPlace(cell);
        }

        // Цена одной клетки по Definition. Без системы — 0.
        public int CellCost()
        {
            if (!EnsureReady() || system == null) return 0;
            return system.CellCost(Definition);
        }

        // Своя клетка = уже есть в states (сравнение по Cell).
        public bool IsOwnRoadCell(GridPosition cell)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].Cell == cell)
                {
                    return true;
                }
            }
            return false;
        }

        // Перестроить вид по states. View добирается лениво, как было в контроллере.
        public void RefreshView()
        {
            if (!EnsureReady()) return;
            List<GridPosition> cells = new List<GridPosition>(states.Count);
            for (int i = 0; i < states.Count; i++)
            {
                cells.Add(states[i].Cell);
            }
            View.Rebuild(cells, GridVisualizer.Grid.CellSize);
        }
    }
}
