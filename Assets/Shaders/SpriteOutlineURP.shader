Shader "Sprites/SpriteOutlineURP"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness (px)", Range(0,8)) = 1
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
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
            float4 _MainTex_TexelSize; // x=1/width, y=1/height

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos   = TransformObjectToHClip(v.vertex.xyz);
                o.uv    = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                float a = col.a;

                // 주변 8방향 탐색(알파 확장)
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

                // 내부는 원본 스프라이트
                return col;
            }
            ENDHLSL
        }
    }
}