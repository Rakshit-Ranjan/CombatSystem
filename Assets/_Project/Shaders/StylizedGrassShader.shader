Shader "Universal Render Pipeline/Nature/StylizedTerrainGrass"
{
    Properties
    {
        [Header(Textures)]
        _MainTex ("Grass Texture", 2D) = "white" {}
        
        [Header(Terrain Texture Sampling)]
        [Toggle(SAMPLE_TERRAIN_TEXTURE)] _SampleTerrainTexture ("Sample Terrain Texture", Float) = 0
        _Control0 ("Control Map 0 (RGBA)", 2D) = "black" {}
        _Control1 ("Control Map 1 (RGBA)", 2D) = "black" {}
        _Control2 ("Control Map 2 (R)", 2D) = "black" {}
        _TerrainSplat0 ("Terrain Layer 0", 2D) = "white" {}
        _TerrainSplat1 ("Terrain Layer 1", 2D) = "white" {}
        _TerrainSplat2 ("Terrain Layer 2", 2D) = "white" {}
        _TerrainSplat3 ("Terrain Layer 3", 2D) = "white" {}
        _TerrainSplat4 ("Terrain Layer 4", 2D) = "white" {}
        _TerrainSplat5 ("Terrain Layer 5", 2D) = "white" {}
        _TerrainSplat6 ("Terrain Layer 6", 2D) = "white" {}
        _TerrainSplat7 ("Terrain Layer 7", 2D) = "white" {}
        _TerrainSplat8 ("Terrain Layer 8", 2D) = "white" {}
        _TerrainSize ("Terrain Size (XZ)", Vector) = (500, 500, 0, 0)
        _TerrainPosition ("Terrain Position (XZ)", Vector) = (0, 0, 0, 0)
        
        [Header(Color Properties)]
        _NearColor ("Near Color", Color) = (0.4, 0.7, 0.3, 1)
        _FarColor ("Far Color", Color) = (0.3, 0.5, 0.25, 1)
        _TopColor ("Top Color", Color) = (0.6, 0.9, 0.5, 1)
        _BottomColor ("Bottom Color", Color) = (0.2, 0.4, 0.15, 1)
        
        [Header(Distance Settings)]
        _NearDistance ("Near Distance", Float) = 5.0
        _FarDistance ("Far Distance", Float) = 50.0
        
        [Header(Gradient Settings)]
        _GradientStrength ("Gradient Strength", Range(0, 1)) = 0.7
        _GradientOffset ("Gradient Offset", Range(-1, 1)) = 0.0
        
        [Header(Terrain Blending)]
        _TerrainBlend ("Terrain Color Blend", Range(0, 1)) = 0.3
        [Toggle(USE_TERRAIN_COLOR)] _UseTerrainColor ("Use Terrain Color", Float) = 1
        [Toggle(USE_SIMPLE_TERRAIN_BLEND)] _UseSimpleBlend ("Use Simple Terrain Blend", Float) = 1
        
        [Header(Wind Animation)]
        _WindSpeed ("Wind Speed", Float) = 1.0
        _WindStrength ("Wind Strength", Float) = 0.05
        _WindScale ("Wind Scale", Float) = 0.1
        _WindHeightStart ("Wind Height Start", Range(0, 1)) = 0.3
        _WindHeightEnd ("Wind Height End", Range(0, 1)) = 1.0
        
        [Header(Interaction)]
        _InteractionStrength ("Interaction Strength", Float) = 0.5
        _InteractionRadius ("Interaction Radius", Float) = 2.0
        _InteractionFalloff ("Interaction Falloff", Float) = 1.5
        _GrassRecovery ("Grass Recovery Speed", Range(0.1, 5)) = 1.5
        _GrassTrample ("Grass Trample Strength", Range(0, 2)) = 1.0
        
        [Header(Culling and LOD)]
        _CullDistance ("Cull Distance", Float) = 100.0
        _FadeRange ("Fade Range", Float) = 10.0
        [Toggle(ENABLE_DISTANCE_CULLING)] _EnableDistanceCulling ("Enable Distance Culling", Float) = 1
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.1
        
        [Header(Alpha Clipping)]
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

        [Header(Ambient Occlusion)]
        _AOStrength ("AO Strength", Range(0,1)) = 0.5
        _AOExponent ("AO Exponent", Range(0.5,4)) = 1.5
        _AONoiseStrength ("AO Noise Strength", Range(0,1)) = 0.2
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }
        LOD 200
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma shader_feature_local USE_TERRAIN_COLOR
            #pragma shader_feature_local USE_SIMPLE_TERRAIN_BLEND
            #pragma shader_feature_local SAMPLE_TERRAIN_TEXTURE
            #pragma shader_feature_local ENABLE_DISTANCE_CULLING

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // --------------------------------------------------
            // Textures
            // --------------------------------------------------
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            #ifdef SAMPLE_TERRAIN_TEXTURE
            TEXTURE2D(_Control0);
            TEXTURE2D(_Control1);
            TEXTURE2D(_Control2);
            TEXTURE2D(_TerrainSplat0);
            TEXTURE2D(_TerrainSplat1);
            TEXTURE2D(_TerrainSplat2);
            TEXTURE2D(_TerrainSplat3);
            TEXTURE2D(_TerrainSplat4);
            TEXTURE2D(_TerrainSplat5);
            TEXTURE2D(_TerrainSplat6);
            TEXTURE2D(_TerrainSplat7);
            TEXTURE2D(_TerrainSplat8);
            SAMPLER(sampler_Control0);
            SAMPLER(sampler_TerrainSplat0);
            #endif

            // --------------------------------------------------
            // Interaction globals (unchanged)
            // --------------------------------------------------
            float4 _InteractorPositions[10];
            int _InteractorCount;
            float4 _InteractorVelocity;

            // --------------------------------------------------
            // Material properties (unchanged names)
            // --------------------------------------------------
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;

                float4 _NearColor;
                float4 _FarColor;
                float4 _TopColor;
                float4 _BottomColor;

                float _NearDistance;
                float _FarDistance;

                float _GradientStrength;
                float _GradientOffset;

                float _TerrainBlend;

                float _WindSpeed;
                float _WindStrength;
                float _WindScale;

                float _InteractionStrength;
                float _InteractionRadius;
                float _InteractionFalloff;

                float _CullDistance;
                float _FadeRange;

                float _AmbientStrength;
                float _Cutoff;

                float _AOStrength;
                float _AOExponent;
                float _AONoiseStrength;

                float _GrassRecovery;
                float _GrassTrample;

                #ifdef SAMPLE_TERRAIN_TEXTURE
                float4 _TerrainSplat0_ST;
                float4 _TerrainSplat1_ST;
                float4 _TerrainSplat2_ST;
                float4 _TerrainSplat3_ST;
                float4 _TerrainSplat4_ST;
                float4 _TerrainSplat5_ST;
                float4 _TerrainSplat6_ST;
                float4 _TerrainSplat7_ST;
                float4 _TerrainSplat8_ST;
                float4 _TerrainSize;
                float4 _TerrainPosition;
                #endif
            CBUFFER_END

            // --------------------------------------------------
            // Structs
            // --------------------------------------------------
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float3 localPos   : TEXCOORD3;
                float fogCoord    : TEXCOORD4;
                float distFade    : TEXCOORD5;
                float bladeRootWS : TEXCOORD6;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // --------------------------------------------------
            // Utility
            // --------------------------------------------------
            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(Hash(i), Hash(i + float2(1,0)), f.x),
                    lerp(Hash(i + float2(0,1)), Hash(i + 1), f.x),
                    f.y
                );
            }

            float ComputeInteractionMask(float3 worldPos)
            {
                if (_InteractorCount <= 0)
                    return 1.0; // full wind

                float mask = 1.0;

                for (int i = 0; i < _InteractorCount; i++)
                {
                    float3 d = worldPos - _InteractorPositions[i].xyz;
                    float dist = length(d);

                    if (dist < _InteractionRadius)
                    {
                        float t = saturate(dist / _InteractionRadius);
                        mask = min(mask, t);
                    }
                }

                return mask;
            }

            // --------------------------------------------------
            // Interaction bending (optimized, stable)
            // --------------------------------------------------
            float3 ApplyInteractionBend(float3 worldPos, float tipFactor)
            {
                if (_InteractorCount <= 0)
                    return 0;

                float3 bend = 0;
                float speed = saturate(_InteractorVelocity.w * 0.25);

                // Movement direction (XZ only)
                float3 moveDir = _InteractorVelocity.xyz;
                moveDir.y = 0;
                moveDir = normalize(moveDir + 1e-5);

                for (int i = 0; i < _InteractorCount; i++)
                {
                    float3 toBlade = worldPos - _InteractorPositions[i].xyz;
                    float dist = length(toBlade);

                    if (dist < _InteractionRadius && dist > 1e-3)
                    {
                        float t = saturate(1.0 - dist / _InteractionRadius);
                        float falloff = pow(t, _InteractionFalloff);

                        float3 awayDir = normalize(toBlade);

                        // 1) Always-on proximity push (prevents clipping)
                        bend += awayDir * falloff * 0.35;

                        // 2) Directional bend when moving
                        float facing = saturate(dot(moveDir, -awayDir));
                        bend += moveDir * falloff * facing * speed;
                    }
                }

                // Stable "memory" (no exp, no time dependency)
                float memory = lerp(1.0 - _GrassRecovery, 1.0, speed);

                // Apply pinning EARLY and once
                bend *= tipFactor;

                return bend * _InteractionStrength * memory;
            }

            // --------------------------------------------------
            // Vertex
            // --------------------------------------------------
            Varyings vert (Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);

                o.localPos = input.positionOS.xyz;
                float tip = saturate(input.color.r);

                // Stable base world position
                float3 baseWorldPos = TransformObjectToWorld(o.localPos);
                float3 worldPos = baseWorldPos;

                // Wind phase
                float wave =
                    sin(baseWorldPos.x * 0.15 +
                        baseWorldPos.z * 0.10 +
                        _Time.y * _WindSpeed);

                float3 windDir = normalize(float3(1.0, 0.0, 1.0));

                float interactionMask = ComputeInteractionMask(worldPos);
                interactionMask *= interactionMask;

                float3 wind =
                    windDir * _WindStrength * tip * wave * interactionMask;

                input.positionOS.xyz += mul((float3x3)unity_WorldToObject, wind);

                // Interaction bend
                worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float3 bend = ApplyInteractionBend(worldPos, tip);
                input.positionOS.xyz += mul((float3x3)unity_WorldToObject, bend);

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);

                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.bladeRootWS = TransformObjectToWorld(float3(0,0,0));
                o.normalWS   = float3(0,1,0);
                o.uv         = TRANSFORM_TEX(input.uv, _MainTex);
                o.fogCoord   = ComputeFogFactor(o.positionCS.z);

                #ifdef ENABLE_DISTANCE_CULLING
                    float d = distance(_WorldSpaceCameraPos, o.positionWS);
                    o.distFade = saturate((_CullDistance - d) / _FadeRange);
                #else
                    o.distFade = 1.0;
                #endif

                return o;
            }
            half3 SampleTerrainColor(float3 worldPos)
            {
                #if defined(SAMPLE_TERRAIN_TEXTURE)

                    float2 terrainUV =
                        (worldPos.xz - _TerrainPosition.xy) / _TerrainSize.xy;

                    half4 c0 = SAMPLE_TEXTURE2D(_Control0, sampler_Control0, terrainUV);
                    half4 c1 = SAMPLE_TEXTURE2D(_Control1, sampler_Control0, terrainUV);
                    half4 c2 = SAMPLE_TEXTURE2D(_Control2, sampler_Control0, terrainUV);

                    float2 splatUV = worldPos.xz;

                    half3 t0 = SAMPLE_TEXTURE2D(_TerrainSplat0, sampler_TerrainSplat0, splatUV * _TerrainSplat0_ST.xy).rgb;
                    half3 t1 = SAMPLE_TEXTURE2D(_TerrainSplat1, sampler_TerrainSplat0, splatUV * _TerrainSplat1_ST.xy).rgb;
                    half3 t2 = SAMPLE_TEXTURE2D(_TerrainSplat2, sampler_TerrainSplat0, splatUV * _TerrainSplat2_ST.xy).rgb;
                    half3 t3 = SAMPLE_TEXTURE2D(_TerrainSplat3, sampler_TerrainSplat0, splatUV * _TerrainSplat3_ST.xy).rgb;
                    half3 t4 = SAMPLE_TEXTURE2D(_TerrainSplat4, sampler_TerrainSplat0, splatUV * _TerrainSplat4_ST.xy).rgb;
                    half3 t5 = SAMPLE_TEXTURE2D(_TerrainSplat5, sampler_TerrainSplat0, splatUV * _TerrainSplat5_ST.xy).rgb;
                    half3 t6 = SAMPLE_TEXTURE2D(_TerrainSplat6, sampler_TerrainSplat0, splatUV * _TerrainSplat6_ST.xy).rgb;
                    half3 t7 = SAMPLE_TEXTURE2D(_TerrainSplat7, sampler_TerrainSplat0, splatUV * _TerrainSplat7_ST.xy).rgb;
                    half3 t8 = SAMPLE_TEXTURE2D(_TerrainSplat8, sampler_TerrainSplat0, splatUV * _TerrainSplat8_ST.xy).rgb;

                    return
                        t0 * c0.r +
                        t1 * c0.g +
                        t2 * c0.b +
                        t3 * c0.a +
                        t4 * c1.r +
                        t5 * c1.g +
                        t6 * c1.b +
                        t7 * c1.a +
                        t8 * c2.r;

                #else
                    return half3(1,1,1);
                #endif
            }

            // --------------------------------------------------
            // Fragment
            // --------------------------------------------------
            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                clip(i.distFade - 0.01);

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                clip(tex.a - _Cutoff);

                //Gradient
                float g = saturate((i.localPos.y + _GradientOffset) * 2.0);
                half3 gradCol = lerp(_BottomColor.rgb, _TopColor.rgb, g);

                // Distance color
                float d = distance(_WorldSpaceCameraPos, i.positionWS);
                float df = saturate((d - _NearDistance) / (_FarDistance - _NearDistance));
                half3 distCol = lerp(_NearColor.rgb, _FarColor.rgb, df);

                // float d = distance(_WorldSpaceCameraPos, i.positionWS);
                // float df = saturate((d - _NearDistance) / (_FarDistance - _NearDistance));

                //half3 baseCol = lerp(_NearColor.rgb, _FarColor.rgb, df) * tex.rgb;

                half3 baseCol = lerp(distCol, gradCol, _GradientStrength) * tex.rgb;
                //half3 baseCol = lerp(_NearColor, _FarColor, df) * tex.rgb;
                #if defined(USE_TERRAIN_COLOR)
                    half3 terrainCol = SampleTerrainColor(i.bladeRootWS);
                    baseCol = lerp(baseCol, baseCol * terrainCol, _TerrainBlend);
                #endif  

                // // Fake AO
                // float ao = pow(saturate(1.0 - i.localPos.y), _AOExponent);
                // ao = lerp(1.0, ao, _AOStrength);
                // baseCol *= ao;

                // Lighting
                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalWS = normalize(i.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                inputData.fogCoord = i.fogCoord;

                Light light = GetMainLight(inputData.shadowCoord);
                float3 grassNormal = float3(0,1,0);
                float ndl = saturate(dot(grassNormal, light.direction));

                // stylized ramp — removes harsh contrast
                ndl = ndl * 0.6 + 0.4;

                // soften shadows on grass
                float shadow = lerp(1.0, light.shadowAttenuation, 0.5);

                half3 col = baseCol * light.color * (ndl * shadow + _AmbientStrength);
                col = MixFog(col, i.fogCoord);

                return half4(col, 1);
            }

            ENDHLSL
        }
        
        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _InteractorPositions[10];
            int _InteractorCount;
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _WindSpeed;
                float _WindStrength;
                float _WindScale;
                float _WindHeightStart;
                float _WindHeightEnd;
                float _Cutoff;
                float _InteractionStrength;
                float _InteractionRadius;
                float _InteractionFalloff;
                float _GrassRecovery;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Reuse the same helper for shadow pass (must exist in this scope)
            float3 ApplyInteractionBend_Shadow(float3 worldPos, float3 localPos)
            {
                float3 bendOffset = float3(0.0, 0.0, 0.0);
                float tipFactor = saturate((localPos.y + 0.5));
                if (_InteractorCount <= 0) return bendOffset;

                for (int i = 0; i < _InteractorCount; ++i)
                {
                    float3 center = _InteractorPositions[i].xyz;
                    float3 toCenter = worldPos - center;
                    float d = length(toCenter);
                    if (d < _InteractionRadius && d > 0.0001)
                    {
                        float falloff = pow(1.0 - (d / _InteractionRadius), max(0.001, _InteractionFalloff));
                        float3 dir = normalize(toCenter);
                        float targetStrength = _InteractionStrength * falloff * tipFactor;
                        float recovery = saturate(_GrassRecovery * 0.02);
                        float strength = targetStrength * recovery;
                        bendOffset.x += dir.x * strength;
                        bendOffset.z += dir.z * strength;
                    }
                }
                return bendOffset;
            }
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                
                // Apply wind with height-based influence
                float3 worldPos = positionInputs.positionWS;
                float normalizedHeight = saturate((input.positionOS.y + 0.5) / 1.0);
                float windInfluence = smoothstep(_WindHeightStart, _WindHeightEnd, normalizedHeight);
                
                float time = _Time.y * _WindSpeed;
                float2 windUV = worldPos.xz * _WindScale;
                float windNoise = noise(windUV + float2(time, time * 0.5));
                windNoise += noise(windUV * 2.0 + float2(time * 1.5, time)) * 0.5;
                
                float3 windOffset = float3(
                    sin(windNoise * 6.28318) * _WindStrength * windInfluence,
                    0,
                    cos(windNoise * 6.28318) * _WindStrength * windInfluence * 0.7
                );
                
                input.positionOS.xyz += mul((float3x3)unity_WorldToObject, windOffset);

                // recompute world pos for bending
                positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                worldPos = positionInputs.positionWS;

                // apply interaction bending in shadow pass
                // use the original object local pos from input.positionOS (approx)
                float3 bendWorld = ApplyInteractionBend_Shadow(worldPos, input.positionOS.xyz);
                input.positionOS.xyz += mul((float3x3)unity_WorldToObject, bendWorld);
                
                positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionInputs.positionWS, normalWS, _MainLightPosition.xyz));
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Alpha clipping for shadows
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
        
        // Depth only pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
