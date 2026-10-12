using UnityEngine;

namespace Citybuilder
{
    // Драйвер игровых часов. Крутит SimulationClock каждый кадр.
    // В сцену добавляется отдельно (объект Simulation).
    public class SimulationRunner : MonoBehaviour
    {
        [Header("Simulation")]
        [Tooltip("Скорость симуляции. Пауза останавливает тики часов.")]
        public SimulationSpeed Speed = SimulationSpeed.Playing;

        private SimulationClock clock;

        // Ленивый доступ: часы создаются при первом обращении.
        // Скорость из инспектора применяется сразу после создания.
        public SimulationClock Clock
        {
            get
            {
                if (clock == null)
                {
                    clock = new SimulationClock();
                    clock.SetSpeed(Speed);
                }
                return clock;
            }
        }

        private void Start()
        {
            Clock.SetSpeed(Speed);
        }

        private void OnValidate()
        {
            if (clock != null)
            {
                clock.SetSpeed(Speed);
            }
        }

        private void Update()
        {
            if (Clock == null)
            {
                return;
            }
            Clock.Update(Time.deltaTime);
        }

        public void SetSpeed(SimulationSpeed speed)
        {
            Speed = speed;
            if (clock != null)
            {
                clock.SetSpeed(speed);
            }
        }
    }
}
