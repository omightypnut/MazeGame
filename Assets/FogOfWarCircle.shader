Shader "Custom/FogOfWarCircle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Fog Color", Color) = (0,0,0,0.9)
        _PlayerPos ("Player Pos", Vector) = (0.5,0.5,0,0)
        _VisionRadius ("Vision Radius", Float) = 0.15
        _Softness ("Softness", Float) = 0.03
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _PlayerPos;
            float _VisionRadius;
            float _Softness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = distance(i.uv, _PlayerPos.xy);
                float alphaMask = smoothstep(_VisionRadius, _VisionRadius + _Softness, dist);

                fixed4 col = _Color;
                col.a *= alphaMask;
                return col;
            }
            ENDCG
        }
    }
}