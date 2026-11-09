using UnityEngine;

namespace Citybuilder
{
    // Визуальная фаза стройки выросшего здания (EPIC-17): 3 тика в масштабе 0.6 серым каркасом без крыши,
    // затем плавный переход к полному размеру и цвету. Домен и сейвы не затрагивает.
    public class GrowthStageView : MonoBehaviour
    {
        public const int ConstructionTicks = 3;
        public const float FrameScale = 0.6f;

        private SimulationRunner runner;
        private MeshRenderer wallRenderer;
        private GameObject roofObject;
        private Vector3 fullScale;
        private Vector3 baseCenter;
        private Color finalWallColor;
        private int ticksRemaining = ConstructionTicks;
        private float growBlend = 1f;
        private bool isConstructing;

        public bool IsConstructing
        {
            get { return isConstructing; }
        }

        public int TicksRemaining
        {
            get { return ticksRemaining; }
        }

        public void BeginConstruction(
            MeshRenderer renderer, GameObject roof, Vector3 targetScale, Vector3 groundCenter, Color wallColor)
        {
            wallRenderer = renderer;
            roofObject = roof;
            fullScale = targetScale;
            baseCenter = groundCenter;
            finalWallColor = wallColor;
            ticksRemaining = ConstructionTicks;
            growBlend = 0f;
            isConstructing = true;
            if (roofObject != null) roofObject.SetActive(false);
            if (wallRenderer != null && wallRenderer.sharedMaterial != null)
            {
                wallRenderer.sharedMaterial.color = new Color(0.5f, 0.5f, 0.5f);
            }
            ApplyScale(FrameScale);
            SubscribeClock();
        }

        public void OnClockTick(SimulationTime time)
        {
            if (!isConstructing) return;
            ticksRemaining--;
            if (ticksRemaining > 0) return;
            isConstructing = false;
            growBlend = 0f;
            if (roofObject != null) roofObject.SetActive(true);
            if (wallRenderer != null && wallRenderer.sharedMaterial != null)
            {
                wallRenderer.sharedMaterial.color = finalWallColor;
            }
            UnsubscribeClock();
        }

        private void Update()
        {
            if (isConstructing || growBlend >= 1f) return;
            growBlend = Mathf.MoveTowards(growBlend, 1f, Time.deltaTime * 2.5f);
            ApplyScale(Mathf.Lerp(FrameScale, 1f, growBlend));
        }

        private void OnDestroy()
        {
            UnsubscribeClock();
        }

        private void ApplyScale(float factor)
        {
            Vector3 s = fullScale * factor;
            transform.localScale = s;
            transform.localPosition = new Vector3(baseCenter.x, s.y * 0.5f, baseCenter.z);
        }

        private void SubscribeClock()
        {
            if (runner == null) runner = FindFirstObjectByType<SimulationRunner>();
            if (runner != null && runner.Clock != null) runner.Clock.TickCompleted += OnClockTick;
        }

        private void UnsubscribeClock()
        {
            if (runner != null && runner.Clock != null) runner.Clock.TickCompleted -= OnClockTick;
        }
    }
}
