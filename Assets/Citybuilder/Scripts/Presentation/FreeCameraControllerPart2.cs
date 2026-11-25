using UnityEngine;

namespace Citybuilder
{
    // Хвост класса FreeCameraController (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class FreeCameraController : MonoBehaviour
    {
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
            if (Input.GetMouseButtonDown(1))
            {
                lookBlocked = UiClickGuard.IsOverMenu(Input.mousePosition);
            }
            if (!Input.GetMouseButton(1))
            {
                lookBlocked = false;
                return;
            }
            if (lookBlocked)
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
        public static float ReadYaw(Vector3 angles)
        {
            if (Mathf.Abs(angles.x - MaxPitch) < 0.05f)
            {
                return angles.y - angles.z;
            }
            return angles.y;
        }
    }
}
