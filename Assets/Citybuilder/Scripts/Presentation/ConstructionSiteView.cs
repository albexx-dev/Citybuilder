using System;
using UnityEngine;

namespace Citybuilder
{
    // Универсальная стройплощадка (EPIC-61): леса-каркас (ConstructionScaffold) поверх фундамента
    // + «прорастание» здания по высоте. Прогресс = прошедшие тики / ConstructionTicks. Два режима:
    // sprout — растёт готовый вид (куб без префаба); sprout = null — растёт блок-заглушка, по завершении
    // колбэк заменяет площадку готовым видом (путь префаба). Домен и сейвы не трогает.
    public class ConstructionSiteView : MonoBehaviour
    {
        public const float TargetHeight = 2f;
        public const float TargetWidthFactor = 0.9f;
        private const float MinHeight = 0.001f;
        private static readonly Color FillColor = new Color(0.5f, 0.5f, 0.5f);

        private readonly ConstructionScaffold scaffold = new ConstructionScaffold();
        private SimulationRunner runner;
        private Transform sprout;
        private GameObject fillBlock;
        private Vector3 targetScale;
        private Vector3 groundCenter;
        private int totalTicks;
        private int ticksElapsed;
        private bool isConstructing;
        private Action onCompleted;

        public bool IsConstructing { get { return isConstructing; } }
        public int TicksElapsed { get { return ticksElapsed; } }

        // Габарит цели, когда высота префаба неизвестна: фундамент ×0.9 и номинальная высота.
        public static Vector3 TargetScale(BuildingDefinition def, float cellSize)
        {
            float w = def.Footprint.x * cellSize * TargetWidthFactor;
            float d = def.Footprint.y * cellSize * TargetWidthFactor;
            return new Vector3(w, TargetHeight, d);
        }

        // Площадка-оверлей вместо готового вида: для ручных построек (BuildingViewFactory).
        public static ConstructionSiteView Spawn(Transform parent, Vector3 groundCenter, BuildingDefinition def,
            float cellSize, Vector3 targetScale, Transform sprout, Action onCompleted)
        {
            GameObject host = new GameObject("ConstructionSite");
            host.transform.SetParent(parent, false);
            host.transform.localPosition = new Vector3(groundCenter.x, 0f, groundCenter.z);
            ConstructionSiteView site = host.AddComponent<ConstructionSiteView>();
            site.Begin(sprout, targetScale, Vector3.zero, def.Footprint, cellSize, def.ConstructionTicks, onCompleted);
            return site;
        }

        // targetScale — полная шкала цели; ground — её основание в координатах родителя лесов.
        public void Begin(Transform target, Vector3 fullScale, Vector3 ground, Vector2Int footprint, float cellSize, int ticks, Action completed)
        {
            sprout = target;
            targetScale = fullScale;
            groundCenter = ground;
            totalTicks = ConstructionProgress.ClampTicks(ticks);
            ticksElapsed = 0;
            onCompleted = completed;
            if (totalTicks <= 0) { Finish(); return; }
            isConstructing = true;
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

        // Колбэк зовётся последним: он может пересоздать вид (снос площадки — Destroy/DestroyImmediate).
        private void Finish()
        {
            isConstructing = false;
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

        // Цель прорастает по высоте: локальный Scale.y 0→1 по прогрессу, основание стоит на земле.
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

        // Блок-заглушка: силуэт «растущего» здания, пока префаб ещё не стоит (путь префаба).
        private void BuildFill()
        {
            fillBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            BuildingViewFactory.DestroySafe(fillBlock.GetComponent<Collider>());
            fillBlock.transform.SetParent(transform, false);
            fillBlock.GetComponent<MeshRenderer>().sharedMaterial =
                new Material(Shader.Find("Unlit/Color")) { color = FillColor };
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

        private void OnDestroy()
        {
            UnsubscribeClock();
            scaffold.Clear();
        }
    }
}
