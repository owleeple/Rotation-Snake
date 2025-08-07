// ???FXAA???Shader
Shader "Hidden/FXAA_Custom"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

        SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // Unity???????????

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // FXAA?????????
            fixed4 frag(v2f i) : SV_Target
            {
                // ????????
                fixed4 col = tex2D(_MainTex, i.uv);

            // ??FXAA????????????Unity?FXAA.hlsl?
            float2 uvOffset = _MainTex_TexelSize.xy;
            float lumaCenter = dot(col.rgb, float3(0.299, 0.587, 0.114));

            float lumaUp = dot(tex2D(_MainTex, i.uv + float2(0, uvOffset.y)).rgb, float3(0.299, 0.587, 0.114));
            float lumaDown = dot(tex2D(_MainTex, i.uv - float2(0, uvOffset.y)).rgb, float3(0.299, 0.587, 0.114));
            float lumaLeft = dot(tex2D(_MainTex, i.uv - float2(uvOffset.x, 0)).rgb, float3(0.299, 0.587, 0.114));
            float lumaRight = dot(tex2D(_MainTex, i.uv + float2(uvOffset.x, 0)).rgb, float3(0.299, 0.587, 0.114));

            // ??????
            float edgeHorz = abs(lumaLeft + lumaRight - 2 * lumaCenter);
            float edgeVert = abs(lumaUp + lumaDown - 2 * lumaCenter);
            float edge = max(edgeHorz, edgeVert);

            // ????
            if (edge > 0.1) { // ????
                fixed4 colUp = tex2D(_MainTex, i.uv + float2(0, uvOffset.y));
                fixed4 colDown = tex2D(_MainTex, i.uv - float2(0, uvOffset.y));
                fixed4 colLeft = tex2D(_MainTex, i.uv - float2(uvOffset.x, 0));
                fixed4 colRight = tex2D(_MainTex, i.uv + float2(uvOffset.x, 0));
                col = (col + colUp + colDown + colLeft + colRight) / 5;
            }

            return col;
        }
        ENDCG
    }
    }
}