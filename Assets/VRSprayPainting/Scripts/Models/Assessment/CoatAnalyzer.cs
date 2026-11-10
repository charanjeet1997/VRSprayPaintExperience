using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace VRSprayPainting
{
    /// <summary>
    /// Measures paint thickness over a surface: shrinks its paint texture on the GPU and reads it back
    /// asynchronously (no frame stall), converts it to dry film thickness (FilmSpec) and counts how much of the
    /// surface is bare, below the target, on target and above it (heavy, runs), plus how even the coat is.
    /// </summary>
    public static class CoatAnalyzer
    {
        /// <summary>Below this the texel counts as unpainted.</summary>
        public const float PaintedVolume = 0.05f;

        private const int SampleSize = 128;

        public static void Measure(PaintableSurface surface, FilmSpec film, Action<CoatStats> onDone)
        {
            RenderTexture source = surface != null ? surface.PaintTexture : null;
            if (source == null || !SystemInfo.supportsAsyncGPUReadback)
            {
                onDone(new CoatStats());
                return;
            }

            // Two halving steps before the final size so each sample averages its area instead of point-sampling it.
            RenderTexture small = Downsample(source);
            AsyncGPUReadback.Request(small, 0, TextureFormat.RGBAFloat, request =>
            {
                CoatStats stats = request.hasError ? new CoatStats() : Count(request.GetData<Color>(), film);
                RenderTexture.ReleaseTemporary(small);
                onDone(stats);
            });
        }

        private static RenderTexture Downsample(RenderTexture source)
        {
            RenderTexture current = source;
            int size = source.width;
            while (size / 2 >= SampleSize)
            {
                size /= 2;
                RenderTexture next = RenderTexture.GetTemporary(size, size, 0, GraphicsFormat.R16G16B16A16_SFloat);
                next.filterMode = FilterMode.Bilinear;
                Graphics.Blit(current, next);
                if (current != source)
                    RenderTexture.ReleaseTemporary(current);
                current = next;
            }

            if (current == source)
            {
                current = RenderTexture.GetTemporary(SampleSize, SampleSize, 0, GraphicsFormat.R16G16B16A16_SFloat);
                Graphics.Blit(source, current);
            }
            return current;
        }

        private static CoatStats Count(NativeArray<Color> texels, FilmSpec film)
        {
            var stats = new CoatStats { film = film };
            int total = texels.Length;
            if (total == 0)
                return stats;

            float minVolume = film.TargetMinVolume;
            float maxVolume = film.TargetMaxVolume;
            // Evenness and average are judged over the body of the coat: every spray fades out thin at its edges.
            float bodyVolume = minVolume * 0.5f;

            int painted = 0, below = 0, inTarget = 0, above = 0, body = 0;
            double sum = 0, sumSq = 0;
            for (int i = 0; i < total; i++)
            {
                float volume = texels[i].a;
                if (volume < PaintedVolume)
                    continue;

                painted++;
                if (volume < minVolume) below++;
                else if (volume <= maxVolume) inTarget++;
                else above++;

                if (volume >= bodyVolume)
                {
                    body++;
                    sum += volume;
                    sumSq += volume * volume;
                }
            }

            stats.paintedFraction = (float)painted / total;
            stats.missingFraction = 1f - stats.paintedFraction;
            stats.thinFraction = (float)below / total;
            stats.inTargetFraction = (float)inTarget / total;
            stats.thickFraction = (float)above / total;
            stats.coveredFraction = (float)(inTarget + above) / total;
            if (body > 0)
            {
                double mean = sum / body;
                double variance = Math.Max(sumSq / body - mean * mean, 0.0);
                stats.averageVolume = (float)mean;
                stats.averageFilm = film.ToMicrons((float)mean);
                stats.variation = mean > 1e-4 ? (float)(Math.Sqrt(variance) / mean) : 0f;
            }
            return stats;
        }
    }
}
