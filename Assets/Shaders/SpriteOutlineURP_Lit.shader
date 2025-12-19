Shader "Sprites/SpriteOutlineURP_Lit"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness (px)", Range(0,8)) = 1
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.1
        
        // 2D Light 마스크
        [HideInInspector] _MaskTex ("Mask", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True" 
            "CanUseSpriteAtlas"="True" 
            "PreviewType"="Plane" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Tags { "LightMode"="Universal2D" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
            #pragma multi_compile _ DEBUG_DISPLAY
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineThickness;
                float _AlphaCutoff;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            
            // SHAPE_LIGHT 매크로는 CombinedShapeLightShared.hlsl include 전에 선언해야 함
            #if defined(USE_SHAPE_LIGHT_TYPE_0)
            SHAPE_LIGHT(0)
            #endif
            #if defined(USE_SHAPE_LIGHT_TYPE_1)
            SHAPE_LIGHT(1)
            #endif
            #if defined(USE_SHAPE_LIGHT_TYPE_2)
            SHAPE_LIGHT(2)
            #endif
            #if defined(USE_SHAPE_LIGHT_TYPE_3)
            SHAPE_LIGHT(3)
            #endif
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 lightingUV : TEXCOORD1;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.lightingUV = ComputeScreenPos(o.pos).xy;
                o.color = v.color * _Color;
                return o;
            }

            half4 CombinedShapeLightSharedCustom(half4 color, half4 mask, float2 lightingUV)
            {
                SurfaceData2D surfaceData;
                InputData2D inputData;
                
                surfaceData.albedo = color.rgb;
                surfaceData.alpha = color.a;
                surfaceData.mask = mask;
                surfaceData.normalTS = half3(0, 0, 1);
                
                inputData.uv = lightingUV;
                inputData.lightingUV = lightingUV;
                
                return CombinedShapeLightShared(surfaceData, inputData);
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                float a = col.a;

                // Outline 처리: 현재 픽셀이 투명하면 주변 검사
                if (a < _AlphaCutoff)
                {
                    float2 px = _MainTex_TexelSize.xy * _OutlineThickness;
                    float alphaN = 0.0;
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( px.x, 0)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-px.x, 0)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0,  px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, -px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( px.x,  px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-px.x,  px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( px.x, -px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-px.x, -px.y)).a);

                    if (alphaN >= _AlphaCutoff)
                    {
                        // Outline은 2D Light 영향 없이 원본 색상 그대로 출력
                        return float4(_OutlineColor.rgb, _OutlineColor.a * i.color.a);
                    }
                    else
                    {
                        return 0;
                    }
                }

                // 내부 픽셀: 2D Light 적용
                return CombinedShapeLightSharedCustom(col, half4(1,1,1,1), i.lightingUV);
            }
            ENDHLSL
        }
        
        // Normal Map Pass (2D Light의 노멀맵 지원용 - 필요 시)
        Pass
        {
            Tags { "LightMode"="NormalsRendering" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineThickness;
                float _AlphaCutoff;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float a = col.a * i.color.a;
                
                // Outline 영역도 노멀 출력
                if (a < _AlphaCutoff)
                {
                    float2 px = _MainTex_TexelSize.xy * _OutlineThickness;
                    float alphaN = 0.0;
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( px.x, 0)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-px.x, 0)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0,  px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, -px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( px.x,  px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-px.x,  px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( px.x, -px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-px.x, -px.y)).a);
                    
                    if (alphaN < _AlphaCutoff)
                        discard;
                }
                
                // 기본 노멀 (정면을 바라봄)
                return float4(0.5, 0.5, 1.0, 1.0);
            }
            ENDHLSL
        }
        
        // Unlit Fallback (2D Light가 없는 환경용)
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineThickness;
                float _AlphaCutoff;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                float a = col.a;

                if (a < _AlphaCutoff)
                {
                    float2 px = _MainTex_TexelSize.xy * _OutlineThickness;
                    float alphaN = 0.0;
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( px.x, 0)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-px.x, 0)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0,  px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, -px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( px.x,  px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-px.x,  px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( px.x, -px.y)).a);
                    alphaN = max(alphaN, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-px.x, -px.y)).a);

                    if (alphaN >= _AlphaCutoff)
                        return float4(_OutlineColor.rgb, _OutlineColor.a * i.color.a);
                    else
                        return 0;
                }

                return col;
            }
            ENDHLSL
        }
    }
}
