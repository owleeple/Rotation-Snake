Shader "Custom/SplitTwoColors"
{
    Properties
    {
        _ColorA("Color A", Color) = (1,1,0,1)    // ??
        _ColorB("Color B", Color) = (1,0,0,1)    // ??
        _Direction("Direction", Range(0,3)) = 0  // 0=Right,1=Left,2=Up,3=Down
        _Ratio("Ratio", Range(0,1)) = 0.5        // ????
    }
        SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _ColorA;
            float4 _ColorB;
            int _Direction;
            float _Ratio;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float t = 0;

            // ??????????
            if (_Direction == 0) {          // Right (? -> ?)
                t = i.uv.x;
            }
else if (_Direction == 1) {   // Left (? -> ?)
 t = 1.0 - i.uv.x;
}
else if (_Direction == 2) {   // Up (? -> ?)
 t = i.uv.y;
}
else if (_Direction == 3) {   // Down (? -> ?)
 t = 1.0 - i.uv.y;
}

            // ?? UV ? ratio
            if (t < _Ratio)
                return _ColorA;
            else
                return _ColorB;
        }
        ENDCG
    }
    }
}
