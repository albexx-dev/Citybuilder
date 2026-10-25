using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Мост пробок в рост (067): поездки копят нагрузку дорог, загруженные клетки
    // мягко замедляют рост домов. Счёт — в CongestionSystem, здесь только связка.
    // Настройка сцены: тот же объект, что GridVisualizer.
    public class CongestionController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязателен: даёт Grid для маршрутов и подсчёта пробок.")]
        public GridVisualizer GridVisualizer;

        [Header("Congestion")]
        [Tooltip("С какой нагрузки клетки начинается пробка. Минимум 1.")]
        public int Threshold = 4;
        [Tooltip("Прибавка множителя за поездку сверх порога. Минимум 0.")]
        public float Factor = 0.25f;
        [Tooltip("Потолок множителя. Минимум 1.")]
        public float MaxMultiplier = 3f;

        private RouteSystem routes;
        private CongestionSystem congestion;

        // Наихудший множитель по сетке. Без систем — 1, без исключений.
        public float Multiplier
        {
            get
            {
                BuildSystems();
                return congestion == null ? 1f : congestion.WorstMultiplier();
            }
        }

        // Во сколько раз дольше растёт дом. Без систем — 1: рост как раньше.
        public int GrowthTickMultiplier
        {
            get
            {
                BuildSystems();
                return congestion == null ? 1 : congestion.GrowthTickMultiplier();
            }
        }

        private void Start()
        {
            ClampLimits();
            EnsureReady();
        }

        private void OnValidate()
        {
            ClampLimits();
            ApplySettings();
        }

        // Публичный вход для будущих поездок: клетки пути копят нагрузку.
        public void TrackTrip(List<GridPosition> path)
        {
            if (path == null || path.Count == 0)
            {
                return;
            }
            if (!EnsureReady() || routes == null)
            {
                return;
            }
            routes.TrackLoad(path);
        }

        // Сброс накопленных нагрузок (новый день, перестройка дорог).
        public void ClearLoads()
        {
            if (!EnsureReady() || routes == null)
            {
                return;
            }
            routes.ClearLoads();
        }

        // Проверяет ссылку и лениво создаёт системы. Без GridVisualizer гаснет.
        private bool EnsureReady()
        {
            if (GridVisualizer == null)
            {
                Debug.LogError("CongestionController: задайте GridVisualizer в Inspector.");
                enabled = false;
                return false;
            }
            BuildSystems();
            return true;
        }

        // Ленивое создание: после перезагрузки домена поля Unity пустеют.
        private void BuildSystems()
        {
            if (GridVisualizer == null)
            {
                return;
            }
            if (routes == null)
            {
                routes = new RouteSystem(GridVisualizer.Grid);
            }
            if (congestion == null)
            {
                congestion = new CongestionSystem(routes, GridVisualizer.Grid);
                ApplySettings();
            }
        }

        private void ApplySettings()
        {
            if (congestion == null)
            {
                return;
            }
            congestion.Threshold = Threshold;
            congestion.Factor = Factor;
            congestion.MaxMultiplier = MaxMultiplier;
        }

        private void ClampLimits()
        {
            if (Threshold < 1) Threshold = 1;
            if (Factor < 0f) Factor = 0f;
            if (MaxMultiplier < 1f) MaxMultiplier = 1f;
        }
    }
}
