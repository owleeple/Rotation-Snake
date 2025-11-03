Shader "Custom/WorldSpaceBorder2D"
{
    Properties
    {
        _Color("Fill Color", Color) = (1,1,1,1)
        _BorderColor("Border Color", Color) = (0,0,0,1)
        _BorderWidth("Border Width (World Units)", Float) = 0.05
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 worldPos : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _BorderColor;
            float _BorderWidth;

            float2 _WorldMin; // xMin, yMin
            float2 _WorldMax; // xMax, yMax

            v2f vert(appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = worldPos.xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ???????????
                float2 minPos = mul(unity_ObjectToWorld, float4(-0.5, -0.5, 0, 1)).xy;
                float2 maxPos = mul(unity_ObjectToWorld, float4(0.5, 0.5, 0, 1)).xy;

                // ?????????
                float left = i.worldPos.x - minPos.x;
                float right = maxPos.x - i.worldPos.x;
                float bottom = i.worldPos.y - minPos.y;
                float top = maxPos.y - i.worldPos.y;

                // ????????
                if (left < _BorderWidth || right < _BorderWidth ||
                    top < _BorderWidth || bottom < _BorderWidth)
                {
                    return _BorderColor;
                }
                return _Color;
            }
            ENDCG
        }
    }
}
