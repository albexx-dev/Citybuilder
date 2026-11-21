using System;
using UnityEngine;

namespace Citybuilder
{
    // Стройплощадка (EPIC-61): рост по тикам; снимок v6 — в ConstructionSnapshot.
    public class ConstructionSiteView : MonoBehaviour
    {
        public const float TargetHeight = 2f;
        public const float TargetWidthFactor = 0.9f;
        private const float MinHeight = 0.001f;
        private static readonly Color FillColor = new Color(0.5f, 0.5f, 0.5f);
        // Один неизменяемый материал на все стройки — общий, удалять нельзя.
        private static readonly Material FillMaterial = new Material(Shader.Find("Unlit/Color")) { color = FillColor };
        private readonly ConstructionScaffold scaffold = new ConstructionScaffold();
        private SimulationRunner runner;
        private Transform sprout;
        private GameObject fillBlock;
        private Vector3 targetScale;
        private Vector3 groundCenter;
        private int totalTicks;
        private int ticksElapsed;
        private bool isConstructing;
        private bool hasOrigin;
        private GridPosition origin;
        private string defId;
        private Action onCompleted;
        public bool IsConstructing { get { return isConstructing; } }
        public int TicksElapsed { get { return ticksElapsed; } }
        public GridPosition Origin { get { return origin; } }
        public string DefId { get { return defId; } }
        public static Vector3 TargetScale(BuildingDefinition def, float cellSize)
        {
            return new Vector3(def.Footprint.x * cellSize * TargetWidthFactor, TargetHeight,
                def.Footprint.y * cellSize * TargetWidthFactor);
        }
        public static ConstructionSiteView Spawn(Transform parent, Vector3 groundCenter, BuildingDefinition def,
            float cellSize, Vector3 targetScale, Transform sprout, Action onCompleted)
        {
            GameObject host = new GameObject("ConstructionSite");
            host.transform.SetParent(parent, false);
            host.transform.localPosition = new Vector3(groundCenter.x, 0f, groundCenter.z);
            ConstructionSiteView site = host.AddComponent<ConstructionSiteView>();
            float cell = cellSize > 0f ? cellSize : 1f;
            site.origin = new GridPosition(Mathf.RoundToInt(groundCenter.x / cell - def.Footprint.x * 0.5f),
                Mathf.RoundToInt(groundCenter.z / cell - def.Footprint.y * 0.5f));
            site.hasOrigin = true;
            site.Begin(sprout, targetScale, Vector3.zero, def.Footprint, cellSize, def.ConstructionTicks,
                onCompleted, 0, def.Id);
            return site;
        }
        public void Begin(Transform target, Vector3 fullScale, Vector3 ground, Vector2Int footprint, float cellSize,
            int ticks, Action completed, int elapsed = 0, string definitionId = null)
        {
            sprout = target;
            targetScale = fullScale;
            groundCenter = ground;
            totalTicks = ConstructionProgress.ClampTicks(ticks);
            ticksElapsed = ConstructionProgress.ClampTicks(elapsed);
            if (ticksElapsed > totalTicks) ticksElapsed = totalTicks;
            onCompleted = completed;
            if (definitionId != null) defId = definitionId;
            if (!hasOrigin && cellSize > 0f)
            {
                origin = new GridPosition(Mathf.RoundToInt(ground.x / cellSize - footprint.x * 0.5f),
                    Mathf.RoundToInt(ground.z / cellSize - footprint.y * 0.5f));
                hasOrigin = true;
            }
            if (ConstructionProgress.IsDone(ticksElapsed, totalTicks)) { Finish(); return; }
            isConstructing = true;
            ConstructionSnapshot.Register(this);
            Transform frameRoot = target != null && target.parent != null ? target.parent : transform;
            scaffold.Build(frameRoot, new Vector3(ground.x, 0f, ground.z), footprint, cellSize, fullScale.y);
            if (sprout == null) BuildFill();
            ApplyProgress();
            SubscribeClock();
        }
        public void OnClockTick(SimulationTime time)
        {
            if (!isConstructing) return;
            ticksElapsed++;
            if (ConstructionProgress.IsDone(ticksElapsed, totalTicks)) Finish();
            else ApplyProgress();
        }
        private void Finish()
        {
            isConstructing = false;
            ConstructionSnapshot.Unregister(this);
            UnsubscribeClock();
            scaffold.Clear();
            BuildingViewFactory.DestroySafe(fillBlock);
            fillBlock = null;
            ApplyReadyScale();
            if (onCompleted != null) onCompleted();
        }
        private void ApplyReadyScale()
        {
            if (sprout == null) return;
            sprout.localScale = targetScale;
            sprout.localPosition = new Vector3(groundCenter.x, targetScale.y * 0.5f, groundCenter.z);
        }
        private void ApplyProgress()
        {
            float height = targetScale.y * ConstructionProgress.Progress(ticksElapsed, totalTicks);
            float grown = Mathf.Max(height, MinHeight);
            if (sprout != null)
            {
                sprout.localScale = new Vector3(targetScale.x, grown, targetScale.z);
                sprout.localPosition = new Vector3(groundCenter.x, height * 0.5f, groundCenter.z);
            }
            if (fillBlock != null)
            {
                fillBlock.transform.localScale = new Vector3(targetScale.x * TargetWidthFactor, grown, targetScale.z * TargetWidthFactor);
                fillBlock.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            }
        }
        private void BuildFill()
        {
            fillBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            BuildingViewFactory.DestroySafe(fillBlock.GetComponent<Collider>());
            fillBlock.transform.SetParent(transform, false);
            fillBlock.GetComponent<MeshRenderer>().sharedMaterial = FillMaterial;
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

        // Скачок к накопленным тикам v6 без перепроигрывания событий.
        public void SetElapsed(int ticks)
        {
            if (!isConstructing) return;
            ticksElapsed = ConstructionProgress.ClampTicks(ticks);
            if (ticksElapsed > totalTicks) ticksElapsed = totalTicks;
            if (ConstructionProgress.IsDone(ticksElapsed, totalTicks)) Finish();
            else ApplyProgress();
        }
        private void OnDestroy()
        {
            ConstructionSnapshot.Unregister(this);
            UnsubscribeClock();
            scaffold.Clear();
        }
    }
}
