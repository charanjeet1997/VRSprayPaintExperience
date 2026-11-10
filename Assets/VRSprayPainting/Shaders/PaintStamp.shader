// Adds one spray deposit into the paint texture. Drawn by PaintableSurface over the stamp's rectangle only
// (pass 0), then that rectangle is copied back into the main texture (pass 1).
//
// Paint texture (half float, linear):
//   rgb = colour of the paint already on that spot (reflectance)
//   a   = paint VOLUME on that spot, in "coats" (1 coat ~ 63% hiding, see PaintedSurface)
//
// Each stamp deposits a volume of paint shaped like the spray: lots in the centre, a thin mist at the edge.
// Like real spray paint, the droplets merge into a wet film; a thin film is translucent, a thick one hides.
// The visible colour is the Kubelka-Munk reflectance of the new film over the existing colour, so blends come
// from physics, not tuned mixing rules. The spot is an ellipse given by two UV semi-axes.
Shader "Hidden/VRSprayPainting/PaintStamp"
{
    Properties
    {
        _MainTex ("Paint", 2D) = "black" {}
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _AxisU ("Semi-axis A (UV)", Vector) = (0.05, 0, 0, 0)
        _AxisV ("Semi-axis B (UV)", Vector) = (0, 0.05, 0, 0)
        _Softness ("Edge Softness", Range(0, 1)) = 0.6
        _Color ("Color", Color) = (1, 0, 0, 1)
        _Amount ("Amount", Range(0, 1)) = 0.2
        _Seed ("Seed", Float) = 0
        _Grain ("Grain", Range(0, 1)) = 0.6
        _Overspray ("Overspray Density", Range(0, 1)) = 0.12
        _MaxVolume ("Max Volume (coats)", Float) = 4
        _HidingPower ("Hiding Power (film scattering per coat)", Float) = 3
        _PigmentFloor ("Pigment Min Reflectance (linear)", Range(0.002, 0.1)) = 0.03
        _MergeStart ("Droplets Start Merging (spray density)", Range(0, 1)) = 0.35
        _MergeFull ("Droplets Fully Merged (spray density)", Range(0, 1)) = 0.8
    }

    SubShader
    {
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Center;
            float4 _AxisU;
            float4 _AxisV;
            float _Softness;
            float4 _Color;
            float _Amount;
            float _Seed;
            float _Grain;
            float _Overspray;
            float _MaxVolume;
            float _HidingPower;
            float _PigmentFloor;
            float _MergeStart;
            float _MergeFull;

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i0 = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash(i0);
                float b = Hash(i0 + float2(1, 0));
                float c = Hash(i0 + float2(0, 1));
                float d = Hash(i0 + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Kubelka-Munk layer formula: reflectance of a paint film of thickness sx (scattering x thickness) whose
            // fully-hiding colour is rInf, lying over a background of reflectance rg. Per channel, in linear light.
            //   thin film  -> light passes through it, reflects off the old colour and comes back filtered (a glaze)
            //   thick film -> only the new paint is seen
            // Successive films compose exactly, so many light stamps equal one heavy coat.
            float3 FilmOver(float3 rg, float3 rInf, float sx)
            {
                // Real paint pigments reflect at least a few percent in every channel (binder and extenders scatter
                // light); a perfectly saturated swatch colour would make thin films absorb almost everything.
                rInf = clamp(rInf, _PigmentFloor, 0.998);
                float3 ks = (1.0 - rInf) * (1.0 - rInf) / (2.0 * rInf);
                float3 a = 1.0 + ks;
                float3 b = sqrt(a * a - 1.0);
                float3 bsx = max(b * sx, 1e-4);
                float3 e2 = exp(-2.0 * bsx);
                float3 coth = (1.0 + e2) / (1.0 - e2);
                return (1.0 - rg * (a - b * coth)) / (a - rg + b * coth);
            }

            float4 frag(v2f_img i) : SV_Target
            {
                // Work from the pixel actually being written, never the interpolated texcoord: render targets may be
                // flipped (XR, D3D/Vulkan), and the texcoord would then point at a mirrored texel and copy paint from
                // elsewhere. Pixel position / size always samples the texel being written, in mesh-UV convention.
                float2 uv = i.pos.xy * _MainTex_TexelSize.xy;
                float4 dst = tex2D(_MainTex, uv);

                // Ellipse coordinates: o = s * A + t * B, d = |(s, t)|.
                float2 a = _AxisU.xy;
                float2 b = _AxisV.xy;
                float2 o = uv - _Center.xy;
                float det = a.x * b.y - a.y * b.x;
                if (abs(det) < 1e-10)
                    return dst;
                float s = (o.x * b.y - o.y * b.x) / det;
                float t = (a.x * o.y - a.y * o.x) / det;
                float d = sqrt(s * s + t * t);
                if (d > 1.8)
                    return dst;

                // Droplets are far smaller than a texel: a smooth coat with a soft mottle, plus sparse loose drops.
                float2 texelPos = floor(i.pos.xy);
                float mottle = ValueNoise(texelPos / 3.0 + _Seed) * 0.65 + ValueNoise(texelPos / 1.5 + _Seed * 1.9) * 0.35;
                float drop = Hash(floor(texelPos) * 1.7 + _Seed * 3.1 + 17.0);

                // Spray profile: wet (low softness) = flat-topped with a hard rim, soft = wide gaussian falloff.
                float soft = exp(-d * d * 2.4);
                float wet = 1.0 - smoothstep(0.75, 1.0, d);
                float core = lerp(wet, soft, saturate(_Softness));
                float hide = _Amount * core * lerp(1.0, 0.3 + mottle * 1.4, _Grain * 0.8);
                float dropChance = _Overspray * 0.5 * saturate(1.0 - (d - 0.6) / 1.2);
                hide += step(1.0 - dropChance, drop) * _Amount * 1.5 * (1.0 - core);
                hide = saturate(hide);

                // Hiding added by this stamp -> paint volume (coverage = 1 - exp(-volume), so coats stack like before).
                float added = -log(1.0 - min(hide, 0.999));
                if (added <= 1e-5)
                    return dst;

                float existing = dst.a;
                float total = min(existing + added, _MaxVolume);
                if (existing <= 1e-5)
                    return float4(_Color.rgb, total);

                // Where the spray is dense (stroke centre) droplets merge into a continuous wet film: a translucent
                // layer over the old colour (Kubelka-Munk), so thin films glaze (blue over yellow -> green) and thick
                // ones hide. Where it is sparse (stroke edges, overspray) droplets stay separate: each opaque dot
                // covers part of the spot and the old colour shows between them, so edges stay bright
                // (green over orange -> olive, not black).
                float sx = added * _HidingPower;
                float3 film = FilmOver(dst.rgb, _Color.rgb, sx);
                float3 droplets = lerp(dst.rgb, _Color.rgb, 1.0 - exp(-sx));
                float merged = smoothstep(_MergeStart, _MergeFull, core);
                float3 mixed = saturate(lerp(droplets, film, merged));
                return float4(mixed, total);
            }
            ENDCG
        }

        // Pass 1: plain copy, used to sync the stamped rectangle back into the main paint texture.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 frag(v2f_img i) : SV_Target
            {
                return tex2D(_MainTex, i.pos.xy * _MainTex_TexelSize.xy);
            }
            ENDCG
        }

        // Pass 2: downsample by pixel position (same orientation as the stamps on every platform).
        // Draw into a smaller target; _TargetSize = its size in pixels.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TargetSize;

            float4 frag(v2f_img i) : SV_Target
            {
                return tex2D(_MainTex, i.pos.xy / _TargetSize.xy);
            }
            ENDCG
        }
    }
}
