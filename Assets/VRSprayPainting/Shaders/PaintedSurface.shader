// Visible surface shader: base surface with accumulated paint over it (URP, PBR lit).
// _PaintTex is set per renderer by PaintableSurface through a MaterialPropertyBlock.
Shader "VRSprayPainting/PaintedSurface"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.72, 0.73, 0.74, 1)
        _Smoothness ("Base Smoothness", Range(0, 1)) = 0.2
        _Metallic ("Base Metallic", Range(0, 1)) = 0
        _PaintSmoothness ("Paint Smoothness", Range(0, 1)) = 0.65
        _WetSmoothness ("Wet Paint Smoothness", Range(0, 1)) = 0.95
        [HideInInspector] _Wetness ("Wetness (set at runtime)", Range(0, 1)) = 0
        [NoScaleOffset] _PaintTex ("Paint (set at runtime)", 2D) = "black" {}
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Render Face", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ LIGHTMAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);
            TEXTURE2D(_PaintTex); SAMPLER(sampler_PaintTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Smoothness;
                half _Metallic;
                half _PaintSmoothness;
                half _WetSmoothness;
                half _Wetness;
                half _Cull;
            CBUFFER_END

            // Globals, set by AssessmentController: 1 = show paint thickness as a heat map, and the film target
            // (min / max) as paint volume.
            float _VRSP_ThicknessView;
            float _VRSP_FilmLow;
            float _VRSP_FilmHigh;

            // Thickness heat map against the film target: none = dark, below target (thin) = blue,
            // in target = green, above target (heavy, runs) = red. Same bands as CoatAnalyzer.
            half3 ThicknessHeat(half volume)
            {
                half low = _VRSP_FilmLow > 0.0 ? _VRSP_FilmLow : 1.6;
                half high = _VRSP_FilmHigh > low ? _VRSP_FilmHigh : 3.0;
                half3 none = half3(0.12, 0.12, 0.14);
                half3 thin = half3(0.15, 0.4, 1.0);
                half3 good = half3(0.2, 0.85, 0.3);
                half3 thick = half3(1.0, 0.18, 0.12);
                half3 c = lerp(none, thin, smoothstep(0.05, low * 0.35, volume));
                c = lerp(c, good, smoothstep(low * 0.9, low, volume));
                c = lerp(c, thick, smoothstep(high, high * 1.06, volume));
                return c;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                float2 lightmapUV : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.lightmapUV = input.lightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                return output;
            }

            half4 frag(Varyings input, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(input.uv, _BaseMap)) * _BaseColor;
                half4 paint = SAMPLE_TEXTURE2D(_PaintTex, sampler_PaintTex, input.uv);
                half volume = paint.a;
                // Paint alpha is volume in coats; how much it hides the surface follows 1 - e^-volume.
                paint.a = 1.0 - exp(-paint.a);

                // Back faces (Render Face: Both) light with the flipped normal.
                float3 normalWS = normalize(input.normalWS) * IS_FRONT_VFACE(isFrontFace, 1.0, -1.0);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                // Baked booth lighting: lightmap when the surface is lightmapped, light probes otherwise.
            #if defined(LIGHTMAP_ON)
                inputData.bakedGI = SampleLightmap(input.lightmapUV, normalWS);
            #else
                inputData.bakedGI = SampleSH(normalWS);
            #endif
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = lerp(baseSample.rgb, paint.rgb, paint.a);
                surface.metallic = lerp(_Metallic, 0.0, paint.a);
                // Fresh paint is wet and glossy, then dries to the paint's own sheen.
                half paintSmoothness = lerp(_PaintSmoothness, _WetSmoothness, saturate(_Wetness));
                surface.smoothness = lerp(_Smoothness, paintSmoothness, paint.a);
                surface.occlusion = 1.0;
                surface.alpha = 1.0;
                surface.normalTS = half3(0, 0, 1);

                half4 color = UniversalFragmentPBR(inputData, surface);
                // Heat map is unlit so its colours read the same everywhere in the booth.
                color.rgb = lerp(color.rgb, ThicknessHeat(volume), saturate(_VRSP_ThicknessView));
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
            }
            ENDHLSL
        }

        // Shadows and depth come from URP Lit (same _BaseMap / _BaseColor / _Cull names).
        UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
        UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
        UsePass "Universal Render Pipeline/Lit/DEPTHNORMALS"
        // Lets the lightmapper see the surface colour for bounce light.
        UsePass "Universal Render Pipeline/Lit/META"
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
