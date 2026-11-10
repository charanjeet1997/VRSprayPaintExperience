using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace VRSprayPainting
{
    /// <summary>
    /// Owns the paint texture for one surface, applies paint stamps into it and binds it to the surface material.
    /// Paint texture: rgb = paint colour, a = paint volume in coats (see PaintStamp.shader), stored as half float
    /// so thin mists and gradual colour shifts aren't rounded away.
    /// Each stamp only touches its own rectangle: drawn into a scratch texture, then copied back. A full-texture
    /// pass per stamp would cost the whole texture (1M texels at 1024²) up to 16 times a frame.
    /// Needs a MeshCollider so raycasts return texture coordinates.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class PaintableSurface : MonoBehaviour, IPaintable
    {
        private const int StampPass = 0;
        private const int CopyPass = 1;
        // Stamp shader draws out to 1.8 radii (overspray); one extra texel for bilinear edges.
        private const float StampReach = 1.8f;

        [SerializeField] private Shader stampShader;
        [SerializeField] private int textureSize = 512;

        private Renderer targetRenderer;
        private MaterialPropertyBlock propertyBlock;
        private Material stampMaterial;

        // paintTexture is what the surface shows; scratchTexture receives each stamp's rectangle.
        private RenderTexture paintTexture;
        private RenderTexture scratchTexture;

        /// <summary>rgb = paint colour, a = paint volume in coats. Read-only use (assessment).</summary>
        public RenderTexture PaintTexture => paintTexture;
        /// <summary>Time.time of the last paint stamp (wet paint can still run).</summary>
        public float LastPaintTime => lastPaintTime;
        public int TextureSize => textureSize;

        private float lastPaintTime = float.NegativeInfinity;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            stampMaterial = new Material(stampShader);

            paintTexture = CreatePaintTexture("Paint");
            scratchTexture = CreatePaintTexture("Scratch");

            ResetPaint();
        }

        private void OnDestroy()
        {
            ReleaseTexture(ref paintTexture);
            ReleaseTexture(ref scratchTexture);

            if (stampMaterial != null)
                Destroy(stampMaterial);
        }

        public void ApplyPaint(in PaintStamp stamp)
        {
            Stamp(stamp);
            lastPaintTime = Time.time;
        }

        /// <summary>Adds paint that is not fresh spray (runs and drips): doesn't extend how long the paint stays wet.</summary>
        public void ApplyRunPaint(in PaintStamp stamp) => Stamp(stamp);

        private void Stamp(in PaintStamp stamp)
        {
            // UV-space bounding box of the ellipse including overspray.
            float extentU = StampReach * (Mathf.Abs(stamp.axisU.x) + Mathf.Abs(stamp.axisV.x));
            float extentV = StampReach * (Mathf.Abs(stamp.axisU.y) + Mathf.Abs(stamp.axisV.y));
            float texel = 1f / textureSize;
            float u0 = Mathf.Clamp01(stamp.center.x - extentU - texel);
            float u1 = Mathf.Clamp01(stamp.center.x + extentU + texel);
            float v0 = Mathf.Clamp01(stamp.center.y - extentV - texel);
            float v1 = Mathf.Clamp01(stamp.center.y + extentV + texel);
            if (u1 <= u0 || v1 <= v0)
                return;

            stampMaterial.SetVector(ShaderIDs.Center, stamp.center);
            stampMaterial.SetVector(ShaderIDs.AxisU, stamp.axisU);
            stampMaterial.SetVector(ShaderIDs.AxisV, stamp.axisV);
            stampMaterial.SetFloat(ShaderIDs.Softness, stamp.softness);
            stampMaterial.SetColor(ShaderIDs.Color, stamp.color);
            stampMaterial.SetFloat(ShaderIDs.Amount, stamp.amount);
            stampMaterial.SetFloat(ShaderIDs.Grain, stamp.grain);
            stampMaterial.SetFloat(ShaderIDs.Overspray, stamp.overspray);
            // New mottle/droplet pattern per stamp so it never tiles.
            stampMaterial.SetFloat(ShaderIDs.Seed, Random.value * 1000f);

            // Stamp: paint -> scratch (rectangle only), then copy the rectangle back: scratch -> paint.
            DrawRect(paintTexture, scratchTexture, StampPass, u0, v0, u1, v1);
            DrawRect(scratchTexture, paintTexture, CopyPass, u0, v0, u1, v1);
        }

        /// <summary>Raised after the paint is wiped.</summary>
        public event System.Action Cleared;

        /// <summary>1 right after spraying, falling to 0 when dry (TrainingSettings.DryTime).</summary>
        public float Wetness { get; private set; }
        private float appliedWetness = -1f;

        // Fresh paint looks wet and glossy and dries to its own sheen.
        private void Update()
        {
            float since = Time.time - lastPaintTime;
            Wetness = float.IsInfinity(since) ? 0f : Mathf.Clamp01(1f - since / TrainingSettings.DryTime);
            if (Mathf.Abs(Wetness - appliedWetness) < 0.02f && !(Wetness == 0f && appliedWetness != 0f))
                return;

            appliedWetness = Wetness;
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(ShaderIDs.Wetness, Wetness);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        [ContextMenu("Reset Paint")]
        public void ResetPaint()
        {
            lastPaintTime = float.NegativeInfinity;
            ClearTexture(paintTexture);
            ClearTexture(scratchTexture);
            BindPaintTexture();
            Cleared?.Invoke();
        }

        private void DrawRect(RenderTexture source, RenderTexture target, int pass, float u0, float v0, float u1, float v1)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            stampMaterial.SetTexture(ShaderIDs.MainTex, source);
            stampMaterial.SetPass(pass);

            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            Quad(u0, v0, u1, v1);
            // Some targets render vertically flipped (XR, D3D/Vulkan), which would put the quad on the mirrored rows.
            // The shader derives everything from the pixel it writes, so drawing the mirrored rectangle as well
            // guarantees the real one is covered; pixels outside the stamp are written back unchanged. Overlap is
            // harmless: each pass reads the other texture, so writing a pixel twice gives the same value.
            Quad(u0, 1f - v1, u1, 1f - v0);
            GL.End();
            GL.PopMatrix();

            RenderTexture.active = previous;
        }

        private static void Quad(float u0, float v0, float u1, float v1)
        {
            GL.Vertex3(u0, v0, 0f);
            GL.Vertex3(u0, v1, 0f);
            GL.Vertex3(u1, v1, 0f);
            GL.Vertex3(u1, v0, 0f);
        }

        private void BindPaintTexture()
        {
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(ShaderIDs.PaintTex, paintTexture);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        private RenderTexture CreatePaintTexture(string label)
        {
            // Half float: colour precision for gradual mixing, and alpha holds volume above 1.
            var texture = new RenderTexture(textureSize, textureSize, GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormat.None)
            {
                name = $"{name}_{label}",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                useMipMap = false,
            };
            texture.Create();
            return texture;
        }

        private static void ClearTexture(RenderTexture texture)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = texture;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = previous;
        }

        private static void ReleaseTexture(ref RenderTexture texture)
        {
            if (texture == null)
                return;

            texture.Release();
            Destroy(texture);
            texture = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (TryGetComponent(out Collider col) && !(col is MeshCollider))
                Debug.LogWarning($"[PaintableSurface] {name} needs a MeshCollider; raycasts on {col.GetType().Name} return no UVs.", this);
        }
#endif
    }
}
