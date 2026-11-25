using System;

namespace Citybuilder
{
    // Политика отсечения машин (issue #120): дистанция + усечённая пирамида камеры.
    // Чистый C#: без UnityEngine и аллокаций; базис камеры передаётся числами.
    public sealed class VehicleVisibilityPolicy
    {
        private float distance;
        private float margin;
        private float tanX;
        private float tanY;
        private float px, py, pz;
        private float fx, fy, fz;
        private float rx, ry, rz;
        private float ux, uy, uz;

        public int VisibleCount { get; private set; }
        public int HiddenCount { get; private set; }

        // Параметры проекции на кадр; сбрасывает счётчики замера (ADR-001).
        public void Configure(float fovDegrees, float aspect, float maxDistance, float edgeMargin)
        {
            VisibleCount = 0;
            HiddenCount = 0;
            if (fovDegrees < 1f) fovDegrees = 1f;
            if (fovDegrees > 179f) fovDegrees = 179f;
            if (aspect < 0.1f) aspect = 0.1f;
            distance = Math.Max(1f, maxDistance);
            margin = Math.Max(0f, edgeMargin);
            tanY = (float)Math.Tan(fovDegrees * Math.PI / 360.0);
            tanX = tanY * aspect;
            fx = 0f; fy = 0f; fz = 1f;
            rx = 1f; ry = 0f; rz = 0f;
            ux = 0f; uy = 1f; uz = 0f;
        }

        // Мировые координаты и ортонормированный базис: transform.right/up/forward камеры.
        public void SetCamera(float posX, float posY, float posZ,
            float forwardX, float forwardY, float forwardZ,
            float rightX, float rightY, float rightZ,
            float upX, float upY, float upZ)
        {
            px = posX; py = posY; pz = posZ;
            fx = forwardX; fy = forwardY; fz = forwardZ;
            rx = rightX; ry = rightY; rz = rightZ;
            ux = upX; uy = upY; uz = upZ;
        }

        public bool IsVisible(float x, float y, float z)
        {
            float dx = x - px;
            float dy = y - py;
            float dz = z - pz;
            float depth = dx * fx + dy * fy + dz * fz;
            bool visible = depth >= -margin && depth <= distance
                && Math.Abs(dx * rx + dy * ry + dz * rz) <= depth * tanX + margin
                && Math.Abs(dx * ux + dy * uy + dz * uz) <= depth * tanY + margin;
            if (visible) VisibleCount++;
            else HiddenCount++;
            return visible;
        }
    }
}
