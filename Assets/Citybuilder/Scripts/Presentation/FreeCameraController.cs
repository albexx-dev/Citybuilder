using UnityEngine;

namespace Citybuilder
{
    // Свободная камера: WASD/стрелки — панорама, колесо — зум, Q/E и ПКМ-drag — поворот.
    // Настройка сцены: компонент на MainCamera, Target можно оставить пустым.
    public class FreeCameraController : MonoBehaviour
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
        [Tooltip("Ширина карты по X в метрах.")]
        public float MapWidth = 50f;
        [Tooltip("Длина карты по Z в метрах.")]
        public float MapHeight = 50f;
        [Tooltip("Запас выхода камеры за край карты в метрах.")]
        public float Margin = 10f;

        private void Update()
        {
            Camera camera = Target != null ? Target : Camera.main;
            if (camera == null)
            {
                return;
            }
            float boost = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 3f : 1f;
            float moveStep = Mathf.Max(MoveSpeed, 1f) * Time.deltaTime * boost;
            float turnStep = Mathf.Max(RotateSpeed, 1f) * Time.deltaTime;
            float right = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            float forward = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f)
                - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
            if (right != 0f || forward != 0f)
            {
                Pan(right * moveStep, forward * moveStep);
            }
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (wheel != 0f)
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
            float yaw = camera.transform.eulerAngles.y * Mathf.Deg2Rad;
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
        // Наклон в градусах, pitch в [20, 90], крен всегда 0.
        private void AddPitch(float degrees)
        {
            Camera camera = Target != null ? Target : Camera.main;
            if (camera == null)
            {
                return;
            }
            Vector3 angles = camera.transform.eulerAngles;
            float pitch = Mathf.Clamp(angles.x + degrees, MinPitch, MaxPitch);
            camera.transform.rotation = Quaternion.Euler(pitch, ReadYaw(angles), 0f);
        }
        // Поворот мышью при зажатой ПКМ. Только старый Input.
        private void DragRotate(float turnStep)
        {
            if (!Input.GetMouseButton(1))
            {
                return;
            }
            AddYaw(Input.GetAxis("Mouse X") * turnStep);
            AddPitch(-Input.GetAxis("Mouse Y") * turnStep);
        }
        // Границы карты по XZ, высота в [MinHeight, MaxHeight].
        private void ClampPosition(Camera camera)
        {
            float margin = Mathf.Max(Margin, 0f);
            float low = Mathf.Min(MinHeight, MaxHeight);
            float high = Mathf.Max(MinHeight, MaxHeight);
            Vector3 point = camera.transform.position;
            point.x = Mathf.Clamp(point.x, -margin, MapWidth + margin);
            point.y = Mathf.Clamp(point.y, low, high);
            point.z = Mathf.Clamp(point.z, -margin, MapHeight + margin);
            camera.transform.position = point;
        }
        // На питче 90° yaw и roll вырождаются: истинный yaw = y − z.
        private static float ReadYaw(Vector3 angles)
        {
            if (Mathf.Abs(angles.x - MaxPitch) < 0.05f)
            {
                return angles.y - angles.z;
            }
            return angles.y;
        }
    }
}
