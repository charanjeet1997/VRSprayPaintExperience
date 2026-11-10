using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// Technique over time on the report: nozzle distance (white) against the green target band, and sprayer
    /// tilt (orange) against its limit line. Drawn into a small texture, so the trainee sees exactly when they
    /// drifted. Coat changes are marked with a vertical line.
    /// </summary>
    public class TechniqueGraphView : MonoBehaviour
    {
        [SerializeField] private RawImage image;
        [SerializeField] private int width = 512;
        [SerializeField] private int height = 160;
        [Tooltip("Top of the distance scale, metres.")]
        [SerializeField] private float maxDistance = 0.5f;
        [Tooltip("Top of the tilt scale, degrees.")]
        [SerializeField] private float maxTilt = 45f;

        private static readonly Color Background = new Color(0.07f, 0.08f, 0.09f, 1f);
        private static readonly Color Band = new Color(0.2f, 0.6f, 0.3f, 0.35f);
        private static readonly Color DistanceLine = new Color(0.95f, 0.96f, 0.98f);
        private static readonly Color TiltLine = new Color(0.98f, 0.6f, 0.15f);
        private static readonly Color TiltLimit = new Color(0.98f, 0.6f, 0.15f, 0.45f);
        private static readonly Color CoatMark = new Color(1f, 1f, 1f, 0.25f);

        private Texture2D texture;
        private Color[] pixels;

        public void Show(AssessmentResult result)
        {
            EnsureTexture();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Background;

            // Target distance band and tilt limit.
            int bandLow = ToY(result.sweetMin / maxDistance), bandHigh = ToY(result.sweetMax / maxDistance);
            for (int y = bandLow; y <= bandHigh; y++)
                for (int x = 0; x < width; x++)
                    Blend(x, y, Band);
            int limit = ToY(result.maxGoodTilt / maxTilt);
            for (int x = 0; x < width; x += 6)
                for (int k = 0; k < 3; k++)
                    Blend(x + k, limit, TiltLimit);

            List<TechniqueSample> samples = result.samples;
            if (samples.Count > 1)
            {
                float duration = Mathf.Max(samples[samples.Count - 1].time, 1e-3f);
                int lastCoat = samples[0].coat;
                Vector2 prevD = default, prevT = default;
                for (int i = 0; i < samples.Count; i++)
                {
                    TechniqueSample s = samples[i];
                    float x = s.time / duration * (width - 1);
                    var d = new Vector2(x, ToY(s.distance / maxDistance));
                    var t = new Vector2(x, ToY(s.tilt / maxTilt));
                    if (s.coat != lastCoat)
                    {
                        for (int y = 0; y < height; y++)
                            Blend((int)x, y, CoatMark);
                        lastCoat = s.coat;
                    }
                    if (i > 0)
                    {
                        Line(prevT, t, TiltLine);
                        Line(prevD, d, DistanceLine);
                    }
                    prevD = d;
                    prevT = t;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false);
            image.texture = texture;
        }

        private void EnsureTexture()
        {
            if (texture != null)
                return;
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            pixels = new Color[width * height];
        }

        private int ToY(float normalized) => Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(normalized) * (height - 1)), 0, height - 1);

        private void Blend(int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;
            int i = y * width + x;
            pixels[i] = Color.Lerp(pixels[i], new Color(c.r, c.g, c.b, 1f), c.a);
        }

        // Two-pixel-thick line.
        private void Line(Vector2 a, Vector2 b, Color c)
        {
            int steps = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y)));
            for (int i = 0; i <= steps; i++)
            {
                Vector2 p = Vector2.Lerp(a, b, steps == 0 ? 0f : (float)i / steps);
                Blend((int)p.x, (int)p.y, c);
                Blend((int)p.x, (int)p.y + 1, c);
            }
        }

        private void OnDestroy()
        {
            if (texture != null)
                Destroy(texture);
        }
    }
}
