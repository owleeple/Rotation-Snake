Shader "Custom/AdvancedBorderShader" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}

    // ??????
    _InnerColor("Inner Color", Color) = (1,1,1,1)
    [Toggle] _UseInnerColor("Use Inner Color", Float) = 0

        // ????zzzz
        _BorderColor("Border Color", Color) = (0,0,0,1)
        _BorderWidth("Border Width", Range(0,0.5)) = 0.05

        // ??????
        [Toggle] _ShowLeft("Show Left Border", Float) = 1
        [Toggle] _ShowRight("Show Right Border", Float) = 1
        [Toggle] _ShowTop("Show Top Border", Float) = 1
        [Toggle] _ShowBottom("Show Bottom Border", Float) = 1

        // ??????
        _LeftWidth("Left Width", Range(0,0.5)) = 0.05
        _RightWidth("Right Width", Range(0,0.5)) = 0.05
        _TopWidth("Top Width", Range(0,0.5)) = 0.05
        _BottomWidth("Bottom Width", Range(0,0.5)) = 0.05

        // ??????
        _LeftColor("Left Color", Color) = (0,0,0,1)
        _RightColor("Right Color", Color) = (0,0,0,1)
        _TopColor("Top Color", Color) = (0,0,0,1)
        _BottomColor("Bottom Color", Color) = (0,0,0,1)

        // ????????
        [Toggle] _UseIndividualSettings("Use Individual Settings", Float) = 0
    }

        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass {
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

                sampler2D _MainTex;
                float4 _MainTex_ST;

                // ??????
                float4 _InnerColor;
                float _UseInnerColor;

                // ????
                float4 _BorderColor;
                float _BorderWidth;

                // ??????
                float _ShowLeft;
                float _ShowRight;
                float _ShowTop;
                float _ShowBottom;

                // ??????
                float _LeftWidth;
                float _RightWidth;
                float _TopWidth;
                float _BottomWidth;
                float4 _LeftColor;
                float4 _RightColor;
                float4 _TopColor;
                float4 _BottomColor;
                float _UseIndividualSettings;

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    fixed4 originalColor = tex2D(_MainTex, i.uv);
                    fixed4 finalColor = originalColor;

                    // ????????????
                    if (_UseInnerColor > 0.5) {
                        finalColor = _InnerColor;
                    }

                    // ????
                    bool isBorder = false;
                    fixed4 borderColor = _BorderColor;

                    if (_UseIndividualSettings > 0.5) {
                        // ??????
                        if (_ShowLeft > 0.5 && i.uv.x < _LeftWidth) {
                            isBorder = true;
                            borderColor = _LeftColor;
                        }
                        else if (_ShowRight > 0.5 && i.uv.x > 1.0 - _RightWidth) {
                            isBorder = true;
                            borderColor = _RightColor;
                        }
                        else if (_ShowBottom > 0.5 && i.uv.y < _BottomWidth) {
                            isBorder = true;
                            borderColor = _BottomColor;
                        }
                        else if (_ShowTop > 0.5 && i.uv.y > 1.0 - _TopWidth) {
                            isBorder = true;
                            borderColor = _TopColor;
                        }
                    }
     else {
                        // ??????
                        bool left = _ShowLeft > 0.5 && i.uv.x < _BorderWidth;
                        bool right = _ShowRight > 0.5 && i.uv.x > 1.0 - _BorderWidth;
                        bool bottom = _ShowBottom > 0.5 && i.uv.y < _BorderWidth;
                        bool top = _ShowTop > 0.5 && i.uv.y > 1.0 - _BorderWidth;

                        isBorder = left || right || bottom || top;
                    }

                    // ??????????????
                    if (isBorder) {
                        return borderColor;
                    }

                    // ????????
                    return finalColor;
                }
                ENDCG
            }
    }

        CustomEditor "AdvancedBorderShaderEditor"
}