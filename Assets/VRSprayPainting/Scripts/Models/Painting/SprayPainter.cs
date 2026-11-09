using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Where the nozzle sits relative to the working range.</summary>
    public enum SprayZone
    {
        NoSurface,  // nothing paintable within reach
        Clipping,   // nozzle touching or inside the surface: no paint
        Wet,        // inside range but close: heavy coat, hard edge
        Sweet,      // ideal: even coat, soft edge
        Dry,        // inside range but far: thin, grainy coat that fades out at the max
        TooFar,     // beyond the maximum: no paint
    }

    /// <summary>
    /// Real-world spray model. Raycasts from the nozzle every frame and deposits paint only inside the working range.
    /// The spot is the spray cone's footprint (radius = distance × tan(cone/2)), stretched into an oval when the gun
    /// is tilted. The same paint over a bigger spot gives a thinner coat; close = wet and hard-edged,
    /// far = dry and speckled. Gaps between frames are filled so sweeps leave a continuous stroke.
    /// </summary>
    public class SprayPainter : MonoBehaviour
    {
        [SerializeField] private SprayGun sprayGun;
        [SerializeField] private SprayBrushData brush = SprayBrushData.Default;

        [Header("Working range (metres from the nozzle)")]
        [Tooltip("Nozzle closer than this (or inside the surface) counts as clipping: no paint. Anything further paints.")]
        [SerializeField] private float clipDistance = 0.005f;
        [Tooltip("Spot size never shrinks below its size at this distance, so very close spraying stays visible.")]
        [SerializeField] private float minSpotDistance = 0.03f;
        [Tooltip("Ideal coat between sweet min and sweet max.")]
        [SerializeField] private float sweetMin = 0.15f;
        [SerializeField] private float sweetMax = 0.30f;
        [Tooltip("The coat fades to nothing here (dry spray). Further paints nothing.")]
        [SerializeField] private float maxDistance = 0.45f;
        [Tooltip("How far the aim ray looks for a surface, for the aim marker.")]
        [SerializeField] private float aimReach = 1.2f;

        [Header("Tilt")]
        [Tooltip("Steepest gun-to-surface angle that still paints (0 = straight on).")]
        [SerializeField, Range(30f, 85f)] private float maxTiltAngle = 75f;

        [Header("Stroke")]
        [Tooltip("Rate the brush amount is tuned for. Coverage per second stays the same at any frame rate.")]
        [SerializeField] private float referenceRate = 30f;
        [Tooltip("Gap between filled-in stamps, as a fraction of the spot size.")]
        [SerializeField, Range(0.1f, 1f)] private float stampSpacing = 0.3f;
        [SerializeField] private int maxStampsPerFrame = 16;
        [Tooltip("UV jumps larger than this are a seam or another face; no fill-in across them.")]
        [SerializeField] private float maxFillUvDistance = 0.2f;
        [Tooltip("UV units per metre when the surface mesh can't be read (fallback only).")]
        [SerializeField] private float fallbackUvPerMetre = 1f;
        [SerializeField] private LayerMask paintMask = ~0;

        private readonly Dictionary<Mesh, MeshCache> meshCache = new Dictionary<Mesh, MeshCache>();
        private IPaintable lastSurface;
        private Vector2 lastUv;
        private float progressTimer;

        /// <summary>Raised about <see cref="referenceRate"/> times a second while paint is landing.</summary>
        public event Action Painted;

        public Color Color => brush.color;
        public float ClipDistance => clipDistance;
        public float SweetMin => sweetMin;
        public float SweetMax => sweetMax;
        public float MaxDistance => maxDistance;

        /// <summary>Aim state, updated every frame whether or not the trigger is held.</summary>
        public SprayZone Zone { get; private set; }
        public Vector3 AimPoint { get; private set; }
        public Vector3 AimNormal { get; private set; }
        public float AimDistance { get; private set; }
        /// <summary>True on the frames paint actually landed.</summary>
        public bool IsPainting { get; private set; }

        public void SetColor(Color color) => brush.color = color;

        /// <summary>Spot radius in metres at a distance, for the current cone.</summary>
        public float SpotRadiusAt(float distance) => distance * Mathf.Tan(sprayGun.ConeAngle * 0.5f * Mathf.Deg2Rad);

        public SprayZone ZoneAt(float distance)
        {
            if (distance < clipDistance) return SprayZone.Clipping;
            if (distance < sweetMin) return SprayZone.Wet;
            if (distance <= sweetMax) return SprayZone.Sweet;
            if (distance < maxDistance) return SprayZone.Dry;
            return SprayZone.TooFar;
        }

        private void Update()
        {
            IsPainting = false;
            bool hasHit = Aim(out RaycastHit hit, out IPaintable paintable);

            if (!sprayGun.IsSpraying || !hasHit || Zone == SprayZone.Clipping || Zone == SprayZone.TooFar)
            {
                lastSurface = null;
                return;
            }

            Spray(hit, paintable);
        }

        private bool Aim(out RaycastHit hit, out IPaintable paintable)
        {
            paintable = null;
            Transform nozzle = sprayGun.Nozzle;

            if (!Physics.Raycast(nozzle.position, nozzle.forward, out hit, aimReach, paintMask, QueryTriggerInteraction.Ignore)
                || !hit.collider.TryGetComponent(out paintable))
            {
                Zone = SprayZone.NoSurface;
                return false;
            }

            // Nozzle pushed into the surface: the ray may start inside and still find the far face, so check overlap.
            if (NozzleInsidePaintable(nozzle.position))
            {
                Zone = SprayZone.Clipping;
                return false;
            }

            AimPoint = hit.point;
            AimNormal = hit.normal;
            AimDistance = hit.distance;
            Zone = ZoneAt(hit.distance);
            return true;
        }

        private void Spray(RaycastHit hit, IPaintable paintable)
        {
            Vector3 forward = sprayGun.Nozzle.forward;
            float cosTilt = Vector3.Dot(-forward, hit.normal);
            if (cosTilt < Mathf.Cos(maxTiltAngle * Mathf.Deg2Rad))
            {
                lastSurface = null;
                return;
            }

            // Spot on the surface: the cone footprint, stretched along the tilt direction by 1/cos.
            float radius = SpotRadiusAt(Mathf.Max(hit.distance, minSpotDistance));
            float stretch = 1f / cosTilt;
            Vector3 along = Vector3.ProjectOnPlane(forward, hit.normal);
            if (along.sqrMagnitude < 1e-6f)
                along = Vector3.Cross(hit.normal, Mathf.Abs(hit.normal.y) < 0.9f ? Vector3.up : Vector3.right);
            along.Normalize();
            Vector3 across = Vector3.Cross(hit.normal, along);

            if (!TryWorldToUv(hit, along * (radius * stretch), across * radius, out Vector2 axisU, out Vector2 axisV))
            {
                axisU = new Vector2(radius * stretch * fallbackUvPerMetre, 0f);
                axisV = new Vector2(0f, radius * fallbackUvPerMetre);
            }

            // Same flow over a bigger area = thinner coat. Reference: middle of the sweet spot, default cone, straight on.
            float referenceRadius = (sweetMin + sweetMax) * 0.5f * Mathf.Tan(SprayGun.DefaultConeAngle * 0.5f * Mathf.Deg2Rad);
            float areaRatio = referenceRadius * referenceRadius / Mathf.Max(radius * radius * stretch, 1e-8f);
            float baseAmount = Mathf.Clamp(brush.amount * areaRatio, 0f, 0.6f) * DistanceFalloff(hit.distance);

            // Frame-rate independent: the frame deposits what referenceRate stamps/s would.
            float frameAmount = 1f - Mathf.Pow(1f - baseAmount, Time.deltaTime * referenceRate);

            Vector2 uv = hit.textureCoord;
            float uvStep = Mathf.Max(Mathf.Min(axisU.magnitude, axisV.magnitude) * stampSpacing, 1e-5f);
            bool fill = ReferenceEquals(paintable, lastSurface) && Vector2.Distance(uv, lastUv) <= maxFillUvDistance;
            int steps = fill ? Mathf.Clamp(Mathf.CeilToInt(Vector2.Distance(uv, lastUv) / uvStep), 1, maxStampsPerFrame) : 1;

            // Split the frame's paint across the stroke: a fast sweep spreads the same paint thinner.
            float stampAmount = 1f - Mathf.Pow(1f - frameAmount, 1f / steps);
            float wetness = 1f - Mathf.InverseLerp(clipDistance, sweetMin, hit.distance); // 1 at the surface, 0 from sweet min
            float dryness = Mathf.InverseLerp(sweetMax, maxDistance, hit.distance);      // 0 up to sweet max, 1 at max
            float glancing = 1f - cosTilt;                                               // tilted = more bounce-off

            var stamp = new PaintStamp
            {
                axisU = axisU,
                axisV = axisV,
                color = brush.color,
                amount = stampAmount,
                softness = Mathf.Lerp(0.85f, 0.15f, wetness),
                grain = Mathf.Clamp01(Mathf.Lerp(0.45f, 1f, dryness) - wetness * 0.35f),
                overspray = Mathf.Clamp01(0.08f + dryness * 0.3f + glancing * 0.2f),
            };

            for (int i = 1; i <= steps; i++)
            {
                stamp.center = fill ? Vector2.Lerp(lastUv, uv, (float)i / steps) : uv;
                paintable.ApplyPaint(stamp);
            }

            lastSurface = paintable;
            lastUv = uv;
            IsPainting = true;

            progressTimer += Time.deltaTime;
            float tick = 1f / referenceRate;
            while (progressTimer >= tick)
            {
                progressTimer -= tick;
                Painted?.Invoke();
            }
        }

        // Full strength through the sweet spot; droplets dry and miss as they travel beyond it.
        private float DistanceFalloff(float distance)
        {
            if (distance <= sweetMax)
                return 1f;
            float t = Mathf.InverseLerp(sweetMax, maxDistance, distance);
            return 1f - t * t * (3f - 2f * t);
        }

        // Maps two world vectors lying in the hit triangle's plane to UV vectors using that triangle's UV layout,
        // so the spot keeps its real-world size on any surface regardless of UV scale.
        private bool TryWorldToUv(RaycastHit hit, Vector3 worldA, Vector3 worldB, out Vector2 uvA, out Vector2 uvB)
        {
            uvA = uvB = default;
            if (!(hit.collider is MeshCollider meshCollider) || meshCollider.sharedMesh == null || hit.triangleIndex < 0)
                return false;

            Mesh mesh = meshCollider.sharedMesh;
            if (!meshCache.TryGetValue(mesh, out MeshCache cache))
            {
                if (!mesh.isReadable)
                    return false;
                cache = new MeshCache { vertices = mesh.vertices, uvs = mesh.uv, triangles = mesh.triangles };
                meshCache[mesh] = cache;
            }

            int t = hit.triangleIndex * 3;
            if (cache.uvs == null || cache.uvs.Length == 0 || t + 2 >= cache.triangles.Length)
                return false;

            Transform tr = hit.collider.transform;
            int i0 = cache.triangles[t], i1 = cache.triangles[t + 1], i2 = cache.triangles[t + 2];
            Vector3 p0 = tr.TransformPoint(cache.vertices[i0]);
            Vector3 e1 = tr.TransformPoint(cache.vertices[i1]) - p0;
            Vector3 e2 = tr.TransformPoint(cache.vertices[i2]) - p0;
            Vector2 d1 = cache.uvs[i1] - cache.uvs[i0];
            Vector2 d2 = cache.uvs[i2] - cache.uvs[i0];

            float g11 = Vector3.Dot(e1, e1), g12 = Vector3.Dot(e1, e2), g22 = Vector3.Dot(e2, e2);
            float det = g11 * g22 - g12 * g12;
            if (Mathf.Abs(det) < 1e-12f)
                return false;

            uvA = ToUv(worldA);
            uvB = ToUv(worldB);
            return true;

            // w = a*e1 + b*e2 (least squares in the triangle plane) -> uv = a*d1 + b*d2.
            Vector2 ToUv(Vector3 w)
            {
                float r1 = Vector3.Dot(w, e1), r2 = Vector3.Dot(w, e2);
                float a = (r1 * g22 - r2 * g12) / det;
                float b = (r2 * g11 - r1 * g12) / det;
                return d1 * a + d2 * b;
            }
        }

        private readonly Collider[] overlapBuffer = new Collider[8];

        private bool NozzleInsidePaintable(Vector3 point)
        {
            int count = Physics.OverlapSphereNonAlloc(point, clipDistance, overlapBuffer, paintMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
                if (overlapBuffer[i].TryGetComponent(out IPaintable _))
                    return true;
            return false;
        }

        private struct MeshCache
        {
            public Vector3[] vertices;
            public Vector2[] uvs;
            public int[] triangles;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            clipDistance = Mathf.Max(0.001f, clipDistance);
            minSpotDistance = Mathf.Max(clipDistance, minSpotDistance);
            sweetMin = Mathf.Max(clipDistance, sweetMin);
            sweetMax = Mathf.Max(sweetMin, sweetMax);
            maxDistance = Mathf.Max(sweetMax + 0.01f, maxDistance);
            aimReach = Mathf.Max(maxDistance, aimReach);
        }

        // Range along the nozzle: red = too close, orange = wet, green = sweet, yellow = dry, grey = no paint.
        // Discs show the spray spot size at each distance for the current cone.
        private void OnDrawGizmos()
        {
            if (sprayGun == null || sprayGun.Nozzle == null)
                return;

            Transform nozzle = sprayGun.Nozzle;
            Vector3 origin = nozzle.position;
            Vector3 dir = nozzle.forward;
            var wet = new Color(1f, 0.55f, 0.1f);
            var sweet = new Color(0.25f, 1f, 0.35f);
            var dry = new Color(1f, 0.9f, 0.2f);

            DrawSegment(origin, dir, 0f, clipDistance, new Color(1f, 0.2f, 0.2f));
            DrawSegment(origin, dir, clipDistance, sweetMin, wet);
            DrawSegment(origin, dir, sweetMin, sweetMax, sweet);
            DrawSegment(origin, dir, sweetMax, maxDistance, dry);
            DrawSegment(origin, dir, maxDistance, aimReach, new Color(0.6f, 0.6f, 0.6f, 0.35f));

            DrawDisc(nozzle, minSpotDistance, wet);
            DrawDisc(nozzle, sweetMin, sweet);
            DrawDisc(nozzle, (sweetMin + sweetMax) * 0.5f, new Color(sweet.r, sweet.g, sweet.b, 0.5f));
            DrawDisc(nozzle, sweetMax, sweet);
            DrawDisc(nozzle, maxDistance, dry);

            // Cone edges out to the max range.
            Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
            float r = SpotRadiusAt(maxDistance);
            Vector3 end = origin + dir * maxDistance;
            Gizmos.DrawLine(origin, end + nozzle.up * r);
            Gizmos.DrawLine(origin, end - nozzle.up * r);
            Gizmos.DrawLine(origin, end + nozzle.right * r);
            Gizmos.DrawLine(origin, end - nozzle.right * r);

            if (Application.isPlaying && Zone != SprayZone.NoSurface)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(AimPoint, 0.005f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (sprayGun == null || sprayGun.Nozzle == null)
                return;

            Transform nozzle = sprayGun.Nozzle;
            Vector3 up = nozzle.up * 0.025f;
            float sweetSpot = (sweetMin + sweetMax) * 0.5f;
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(nozzle.position + nozzle.forward * minSpotDistance + up, $"close/wet from {clipDistance * 1000f:0} mm");
            UnityEditor.Handles.Label(nozzle.position + nozzle.forward * sweetMin + up, $"sweet {sweetMin * 100f:0}-{sweetMax * 100f:0} cm");
            UnityEditor.Handles.Label(nozzle.position + nozzle.forward * maxDistance + up,
                $"max {maxDistance * 100f:0} cm\ncone {sprayGun.ConeAngle:0}°, spot Ø {SpotRadiusAt(sweetSpot) * 200f:0.#} cm at {sweetSpot * 100f:0} cm");
        }

        private static void DrawSegment(Vector3 origin, Vector3 dir, float from, float to, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(origin + dir * from, origin + dir * to);
        }

        private void DrawDisc(Transform nozzle, float distance, Color color)
        {
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawWireDisc(nozzle.position + nozzle.forward * distance, nozzle.forward, SpotRadiusAt(distance));
        }
#endif
    }
}
