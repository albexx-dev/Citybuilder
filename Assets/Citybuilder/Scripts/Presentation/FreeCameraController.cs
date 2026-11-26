using UnityEngine;

namespace Citybuilder
{
    // Свободная камера: WASD/стрелки — панорама, колесо — зум, Q/E и ПКМ-drag — поворот.
    // Настройка сцены: компонент на MainCamera, Target можно оставить пустым.
    public partial class FreeCameraController : MonoBehaviour
    {
        private const float MinPitch = 20f;
        private const float MaxPitch = 90f;

        [Header("Target")]
        [Tooltip("Камера. Пусто = Camera.main каждый кадр.")]
        public Camera Target;

        [Header("Speeds")]
        [Tooltip("Скорость панорамы в м/с.")]
        public float MoveSpeed = 30f;
        [Tooltip("Зум: метров за единицу колеса мыши.")]
        public float ZoomSpeed = 40f;
        [Tooltip("Скорость поворота клавишами Q/E в град/с.")]
        public float RotateSpeed = 90f;
        [Tooltip("Скорость обзора мышью в град/с.")]
        public float DragSpeed = 240f;

        [Header("Limits")]
        [Tooltip("Минимальная высота камеры в метрах.")]
        public float MinHeight = 5f;
        [Tooltip("Максимальная высота камеры в метрах.")]
        public float MaxHeight = 80f;

        [Header("Map")]
        [Tooltip("Визуализатор: единственный источник размеров карты (Grid.Size × CellSize).")]
        public GridVisualizer Visualizer;

        // Запасные границы нужны только headless-тестам без визуализатора.
        private float mapWidth;
        private float mapHeight;
        public float MapWidth
        {
            get { return Visualizer == null ? mapWidth : Visualizer.Grid.Size.Width * Visualizer.Grid.CellSize; }
            set { mapWidth = value; }
        }
        public float MapHeight
        {
            get { return Visualizer == null ? mapHeight : Visualizer.Grid.Size.Height * Visualizer.Grid.CellSize; }
            set { mapHeight = value; }
        }
        [Tooltip("Запас выхода камеры за край карты в метрах.")]
        public float Margin = 10f;

        private bool lookBlocked;

        private void Update()
        {
            Camera camera = Target != null ? Target : Camera.main;
            if (camera == null)
            {
                return;
            }
            ReadMovement(out float right, out float forward, out float moveStep,
                out float turnStep, out float boost);
            if (right != 0f || forward != 0f)
            {
                Pan(right * moveStep, forward * moveStep);
            }
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            // Колесо над меню (в т.ч. скролл каталога, задача 173) не должно зумить камеру.
            if (wheel != 0f && !UiClickGuard.IsOverMenu(Input.mousePosition))
            {
                Zoom(wheel * boost);
            }
            float turn = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
            if (turn != 0f)
            {
                AddYaw(turn * turnStep);
            }
            DragRotate(Mathf.Max(DragSpeed, 1f) * Time.deltaTime);
        }
        // Сдвиг в метрах: right — вправо, forward — вперёд (плоскость XZ по yaw камеры).
        public void Pan(float right, float forward)
        {
            Camera camera = Target != null ? Target : Camera.main;
            if (camera == null)
            {
                return;
            }
            float yaw = ReadYaw(camera.transform.eulerAngles) * Mathf.Deg2Rad;
            Vector3 ahead = new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw));
            Vector3 side = new Vector3(ahead.z, 0f, -ahead.x);
            camera.transform.position += side * right + ahead * forward;
            ClampPosition(camera);
        }
        // Зум в шагах колеса: движение вдоль взгляда, высота в [MinHeight, MaxHeight].
        public void Zoom(float steps)
        {
            Camera camera = Target != null ? Target : Camera.main;
            if (camera == null)
            {
                return;
            }
            camera.transform.position += camera.transform.forward * steps * Mathf.Max(ZoomSpeed, 1f);
            ClampPosition(camera);
        }
        // Прыжок камеры над мировой точкой (клик по мини-карте, EPIC-67 шаг 1): высота и yaw остаются, границы карты — ClampPosition.
        public void FocusOn(float worldX, float worldZ)
        {
            Camera camera = Target != null ? Target : Camera.main;
            if (camera == null)
            {
                return;
            }
            Vector3 point = camera.transform.position;
            point.x = worldX;
            point.z = worldZ;
            camera.transform.position = point;
            ClampPosition(camera);
        }
        // Поворот в градусах вокруг мировой оси Y, крен всегда 0.
        public void AddYaw(float degrees)
        {
            Camera camera = Target != null ? Target : Camera.main;
            if (camera == null)
            {
                return;
            }
            Vector3 angles = camera.transform.eulerAngles;
            camera.transform.rotation = Quaternion.Euler(angles.x, ReadYaw(angles) + degrees, 0f);
        }
        // Чтение ввода камеры (issue #127): значения не меняются, только вынесены в хелпер.
        private void ReadMovement(out float right, out float forward, out float moveStep,
            out float turnStep, out float boost)
        {
            boost = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 3f : 1f;
            moveStep = Mathf.Max(MoveSpeed, 1f) * Time.deltaTime * boost;
            turnStep = Mathf.Max(RotateSpeed, 1f) * Time.deltaTime;
            right = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            forward = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f)
                - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
        }

    }
}
