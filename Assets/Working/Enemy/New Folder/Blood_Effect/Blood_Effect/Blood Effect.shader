Shader "Particles/BloodSimple"
{
    Properties
    {
        [Header(Color Controls)]
        [HDR] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _AlphaMin ("Alpha Clip Min", Range(0,1)) = 0.1
        _AlphaSoft ("Alpha Clip Softness", Range(0,1)) = 0.05

        [Header(Mask Controls)]
        _MainTex ("Mask Texture", 2D) = "white" {}
        _MaskStr ("Mask Strength", Range(0,1)) = 0.7
        _ChannelMask ("Channel Mask", Vector) = (1,0,0,0)

        [Header(Noise Controls)]
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseAlphaStr ("Noise Strength", Range(0,1)) = 0.8
        _ChannelMask2 ("Noise Channel Mask", Vector) = (1,0,0,0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            fixed4 _BaseColor;
            fixed _AlphaMin;
            fixed _AlphaSoft;
            fixed _MaskStr;
            fixed4 _ChannelMask;
            fixed _NoiseAlphaStr;
            fixed4 _ChannelMask2;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                UNITY_FOG_COORDS(1)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample mask
                fixed4 mask = tex2D(_MainTex, i.uv);
                mask = lerp(1, mask, _MaskStr);
                fixed alphaMask = dot(mask, _ChannelMask);

                // Sample noise
                fixed4 noise4 = tex2D(_NoiseTex, i.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw);
                fixed noise = dot(noise4, _ChannelMask2);
                noise = lerp(1, noise, _NoiseAlphaStr);

                // Alpha clip
                fixed alpha = i.color.a * alphaMask * noise;
                alpha = saturate((alpha - _AlphaMin) / max(_AlphaSoft, 0.001));

                fixed4 col = _BaseColor * i.color;
                col.a = alpha;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}