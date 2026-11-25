using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Оверлей огня (EPIC-43, ADR-009, issue #116): пул эффектов из кубов-примитивов,
    // пульсация пламени и подъём дыма по времени. Снимок горящих строится по Growth.Homes
    // через Fires.IsBurning — без List в Update. Звук возгорания уже играет AudioEventView
    // по событию FireStart в ленте: второго источника звука здесь нет.
    public class FireOverlayView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Экономика: даёт снимок пожаров Fires. Пусто = ищется в сцене.")] public EconomyTicker Economy;
        [Tooltip("Рост поселений: даёт дома для позиций пламени. Пусто = ищется в сцене.")] public SettlementGrowthController Growth;

        [Header("View")]
        [Tooltip("Пауза между обновлениями пламени в секундах. Минимум 0.25.")] public float RefreshIntervalSec = 0.5f;
        [Tooltip("Высота пламени над землёй в метрах.")] public float HeightAboveGround = 2.4f;

        private const int FlameParts = 3;
        private const int SmokeParts = 3;
        private const int PartCount = FlameParts + SmokeParts; // 6 кубов × 12 tri = 72 tri на эффект (≤300).

        private readonly FireFxSlots slots = new FireFxSlots();
        private GameObject[] effects;
        private Transform[] parts;
        private Material flameMat;
        private Material coreMat;
        private Material smokeMat;
        private float timer;

        private void Update()
        {
            if (!EnsureReady()) return;
            float dt = Time.deltaTime;
            slots.Advance(dt);
            Animate();
            timer += dt;
            if (timer < RefreshIntervalSec) return;
            timer = 0f;
            RefreshNow();
        }

        // Снимок горящих: сканируем дома, обновляем слоты и включаем только нужные эффекты.
        public void RefreshNow()
        {
            if (!EnsureReady()) return;
            slots.BeginSync();
            List<BuildingState> homes = Growth.Homes;
            float cellSize = Growth.GridVisualizer.Grid.CellSize;
            for (int i = 0; i < homes.Count; i++)
            {
                BuildingState home = homes[i];
                if (home == null || home.Definition == null) continue;
                if (!Economy.Fires.IsBurning(home.Id)) continue;
                float x = (home.Origin.X + home.Definition.Footprint.x * 0.5f) * cellSize;
                float z = (home.Origin.Y + home.Definition.Footprint.y * 0.5f) * cellSize;
                slots.Touch(home.Id, x, z);
            }
            slots.EndSync();
            for (int i = 0; i < effects.Length; i++) effects[i].SetActive(i < slots.Count);
        }

        private bool EnsureReady()
        {
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
            if (Growth == null) Growth = FindFirstObjectByType<SettlementGrowthController>();
            if (Economy == null || Growth == null || Growth.GridVisualizer == null) return false;
            if (effects == null) BuildPool();
            return true;
        }

        // Пул создаётся один раз: 12 корней по 6 кубов; материалы общие и не меняются в кадре.
        private void BuildPool()
        {
            Shader shader = Shader.Find("Unlit/Color");
            flameMat = new Material(shader) { color = new Color(1f, 0.45f, 0.08f) };
            coreMat = new Material(shader) { color = new Color(1f, 0.85f, 0.3f) };
            smokeMat = new Material(shader) { color = new Color(0.3f, 0.3f, 0.32f) };
            effects = new GameObject[FireFxSlots.Capacity];
            parts = new Transform[effects.Length * PartCount];
            for (int i = 0; i < effects.Length; i++) BuildEffect(i);
        }

        private void BuildEffect(int slot)
        {
            GameObject root = new GameObject("FireFx");
            root.transform.SetParent(transform, false);
            root.SetActive(false);
            effects[slot] = root;
            int start = slot * PartCount;
            for (int p = 0; p < PartCount; p++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                DestroySafe(cube.GetComponent<Collider>());
                cube.transform.SetParent(root.transform, false);
                Material mat = p < FlameParts ? (p == 0 ? coreMat : flameMat) : smokeMat;
                cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
                parts[start + p] = cube.transform;
            }
        }

        // Анимация только трансформами: фаза из FireFxSlots, аллокаций в кадре нет.
        private void Animate()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                Transform root = effects[i].transform;
                root.position = new Vector3(slots.XAt(i), HeightAboveGround, slots.ZAt(i));
                int start = i * PartCount;
                float phase = slots.AgeAt(i) / FireFxSlots.CycleSec;
                float pulse = 0.8f + 0.25f * Mathf.Sin(phase * Mathf.PI * 2f + i);
                for (int p = 0; p < FlameParts; p++)
                {
                    Transform flame = parts[start + p];
                    float wave = Mathf.Sin(phase * 6.2832f + p * 1.7f + i);
                    flame.localPosition = new Vector3(wave * 0.1f, 0.18f + p * 0.2f, wave * 0.06f);
                    flame.localScale = new Vector3(0.18f * pulse, (0.26f + p * 0.05f) * pulse, 0.18f * pulse);
                    flame.localRotation = Quaternion.Euler(0f, wave * 20f, wave * 12f);
                }
                for (int p = 0; p < SmokeParts; p++)
                {
                    Transform smoke = parts[start + FlameParts + p];
                    float rise = (phase + p / (float)SmokeParts) % 1f;
                    float size = 0.18f + rise * 0.4f;
                    smoke.localPosition = new Vector3(rise * 0.22f, 0.7f + rise * 0.95f, -rise * 0.18f);
                    smoke.localScale = new Vector3(size, size, size);
                }
            }
        }

        private void OnDestroy()
        {
            slots.Clear();
            if (effects != null) for (int i = 0; i < effects.Length; i++) DestroySafe(effects[i]);
            DestroySafe(flameMat);
            DestroySafe(coreMat);
            DestroySafe(smokeMat);
        }

        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }
    }
}
