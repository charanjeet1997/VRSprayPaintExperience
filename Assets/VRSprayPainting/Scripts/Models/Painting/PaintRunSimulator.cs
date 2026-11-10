using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace VRSprayPainting
{
    /// <summary>
    /// Runs, like real wet paint: just after spraying, the surface's thickness is read back a few times a second
    /// (small, asynchronous). Only at the heaviest fresh spots, a few spaced-apart runs form: a wider sag that
    /// tapers into a thin run of the same colour, slowing as it empties and ending in a small teardrop bead.
    /// No new runs start once the paint is no longer fresh. Needs the surface's down direction in UV.
    /// </summary>
    [RequireComponent(typeof(PaintableSurface))]
    public class PaintRunSimulator : MonoBehaviour
    {
        private class Drip
        {
            public Vector2 uv;
            public Color color;
            public float budget;      // metres of run left
            public float startBudget;
        }

        [SerializeField] private Shader stampShader;
        [Tooltip("World down in this surface's UV space.")]
        [SerializeField] private Vector2 uvDown = new Vector2(0f, -1f);
        [Tooltip("UV units per metre on this surface (panel 0.8 m wide = 1.25).")]
        [SerializeField] private float uvPerMetre = 1.25f;

        [Header("When paint runs")]
        [Tooltip("Coats the paint can hold before it runs. Clearly above CoatAnalyzer.ThickVolume: only really heavy paint runs.")]
        [SerializeField] private float runThreshold = 3.4f;
        [Tooltip("New runs only start this soon after spraying (fresh, wet paint). Runs already moving finish.")]
        [SerializeField] private float startWindow = 2.5f;
        [SerializeField] private float scanInterval = 0.4f;
        [SerializeField] private int scanSize = 64;
        [Tooltip("Runs moving at once.")]
        [SerializeField] private int maxDrips = 6;
        [Tooltip("New runs per scan, heaviest spots first.")]
        [SerializeField] private int maxNewPerScan = 2;
        [Tooltip("Cells beside a run (each side) that won't start another one, so runs stay apart.")]
        [SerializeField] private int sideSpacing = 3;
        [Tooltip("A spot that ran stays drained at least this long.")]
        [SerializeField] private float spotCooldown = 20f;

        [Header("Run shape")]
        [Tooltip("Run length (metres) per coat of excess paint, before random variation.")]
        [SerializeField] private float lengthPerExcess = 0.06f;
        [SerializeField] private Vector2 lengthRange = new Vector2(0.02f, 0.12f);
        [Tooltip("Slide speed (m/s) when nearly spent and when full.")]
        [SerializeField] private Vector2 speedRange = new Vector2(0.003f, 0.02f);
        [Tooltip("Width at the end of the run; it starts wider (the sag) and tapers.")]
        [SerializeField] private float trailWidth = 0.003f;
        [Tooltip("Extra coats along the run (same colour: reads as a thicker ridge, not a line).")]
        [SerializeField] private float trailCoats = 0.9f;

        private PaintableSurface surface;
        private Material downsampleMaterial;
        private readonly List<Drip> drips = new List<Drip>();
        private readonly List<(int index, float excess)> candidates = new List<(int index, float excess)>();
        private float[] cooldownUntil;
        private float scanTimer;
        private bool scanPending;

        private const int DownsamplePass = 2;
        private static readonly int TargetSizeId = Shader.PropertyToID("_TargetSize");

        private void OnEnable() => GetComponent<PaintableSurface>().Cleared += Clear;
        private void OnDisable() => GetComponent<PaintableSurface>().Cleared -= Clear;

        private void Awake()
        {
            surface = GetComponent<PaintableSurface>();
            downsampleMaterial = new Material(stampShader);
            cooldownUntil = new float[scanSize * scanSize];
        }

        private void OnDestroy()
        {
            if (downsampleMaterial != null)
                Destroy(downsampleMaterial);
        }

        /// <summary>Runs that formed since the paint was last cleared.</summary>
        public int RunCount { get; private set; }

        /// <summary>Clears moving runs (paint was reset).</summary>
        public void Clear()
        {
            RunCount = 0;
            drips.Clear();
            System.Array.Clear(cooldownUntil, 0, cooldownUntil.Length);
        }

        private void Update()
        {
            bool fresh = Time.time - surface.LastPaintTime < startWindow;
            if (fresh && !scanPending && SystemInfo.supportsAsyncGPUReadback)
            {
                scanTimer += Time.deltaTime;
                if (scanTimer >= scanInterval)
                {
                    scanTimer = 0f;
                    Scan();
                }
            }

            for (int i = drips.Count - 1; i >= 0; i--)
                if (!Slide(drips[i], Time.deltaTime))
                    drips.RemoveAt(i);
        }

        private void Scan()
        {
            RenderTexture source = surface.PaintTexture;
            if (source == null)
                return;

            RenderTexture small = RenderTexture.GetTemporary(scanSize, scanSize, 0, GraphicsFormat.R16G16B16A16_SFloat);
            downsampleMaterial.SetVector(TargetSizeId, new Vector4(scanSize, scanSize, 0f, 0f));
            Graphics.Blit(source, small, downsampleMaterial, DownsamplePass);
            scanPending = true;
            AsyncGPUReadback.Request(small, 0, TextureFormat.RGBAFloat, request =>
            {
                scanPending = false;
                if (this != null && !request.hasError)
                    FindRuns(request.GetData<Color>());
                RenderTexture.ReleaseTemporary(small);
            });
        }

        // Starts a few runs at the heaviest fresh spots, spaced apart.
        private void FindRuns(Unity.Collections.NativeArray<Color> cells)
        {
            float now = Time.time;
            if (now - surface.LastPaintTime > startWindow)
                return;

            candidates.Clear();
            for (int i = 0; i < cells.Length; i++)
            {
                float excess = cells[i].a - runThreshold;
                if (excess > 0f && now >= cooldownUntil[i])
                    candidates.Add((i, excess));
            }
            candidates.Sort((p, q) => q.excess.CompareTo(p.excess));

            int started = 0;
            foreach ((int index, float excess) in candidates)
            {
                if (started >= maxNewPerScan || drips.Count >= maxDrips)
                    break;
                if (now < cooldownUntil[index]) // blocked by a run started in this scan
                    continue;

                int x = index % scanSize, y = index / scanSize;
                float length = Mathf.Clamp(excess * lengthPerExcess * Random.Range(0.6f, 1.4f), lengthRange.x, lengthRange.y);
                var uv = new Vector2((x + Random.Range(0.25f, 0.75f)) / scanSize, (y + Random.Range(0.25f, 0.75f)) / scanSize);
                // Paint is stored in linear colour; stamp colours are set like any colour (gamma), so convert back,
                // otherwise the run is converted twice and comes out far too dark.
                Color color = new Color(cells[index].r, cells[index].g, cells[index].b, 1f).gamma;
                drips.Add(new Drip { uv = uv, color = color, budget = length, startBudget = length });
                RunCount++;
                started++;
                Block(x, y, now);
            }
        }

        // Drains the spot: no new run beside it or just under it for a while.
        private void Block(int x, int y, float now)
        {
            Vector2 down = uvDown.normalized;
            Vector2 across = new Vector2(-down.y, down.x);
            for (int side = -sideSpacing; side <= sideSpacing; side++)
            {
                for (int k = -1; k < 6; k++)
                {
                    int cx = x + Mathf.RoundToInt(across.x * side + down.x * k);
                    int cy = y + Mathf.RoundToInt(across.y * side + down.y * k);
                    if (cx >= 0 && cy >= 0 && cx < scanSize && cy < scanSize)
                        cooldownUntil[cy * scanSize + cx] = now + spotCooldown;
                }
            }
        }

        // Moves one run; false when it has stopped. Wide sag at the top tapering to a thin run and a teardrop bead.
        private bool Slide(Drip drip, float dt)
        {
            float fullness = Mathf.Clamp01(drip.budget / Mathf.Max(drip.startBudget, 1e-4f));
            float speed = Mathf.Lerp(speedRange.x, speedRange.y, fullness * fullness);
            float distance = Mathf.Min(speed * dt, drip.budget);
            Vector2 down = uvDown.normalized;
            Vector2 stepUv = down * (distance * uvPerMetre);
            Vector2 across = new Vector2(-down.y, down.x);
            float width = trailWidth * Mathf.Lerp(0.8f, 2.2f, fullness * fullness);

            surface.ApplyRunPaint(new PaintStamp
            {
                center = drip.uv + stepUv * 0.5f,
                axisU = across * (width * uvPerMetre),
                axisV = down * (Mathf.Max(distance * 0.5f, width * 0.5f) * uvPerMetre),
                color = drip.color,
                amount = 1f - Mathf.Exp(-trailCoats * Mathf.Clamp01(distance / width)),
                softness = 0.7f,
                grain = 0f,
                overspray = 0f,
            });

            drip.uv += stepUv;
            drip.budget -= distance;

            // The run's own paint mustn't start new runs.
            int cx = Mathf.FloorToInt(drip.uv.x * scanSize);
            int cy = Mathf.FloorToInt(drip.uv.y * scanSize);
            if (cx >= 0 && cy >= 0 && cx < scanSize && cy < scanSize)
                cooldownUntil[cy * scanSize + cx] = Time.time + spotCooldown;

            bool inside = drip.uv.x > 0f && drip.uv.x < 1f && drip.uv.y > 0f && drip.uv.y < 1f;
            if (drip.budget > 1e-4f && inside)
                return true;

            // The last of the paint gathers in a small teardrop bead.
            if (inside)
            {
                surface.ApplyRunPaint(new PaintStamp
                {
                    center = drip.uv - stepUv,
                    axisU = across * (trailWidth * 1.3f * uvPerMetre),
                    axisV = down * (trailWidth * 1.9f * uvPerMetre),
                    color = drip.color,
                    amount = 0.6f,
                    softness = 0.55f,
                    grain = 0f,
                    overspray = 0f,
                });
            }
            return false;
        }
    }
}
