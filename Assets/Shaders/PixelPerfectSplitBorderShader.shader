Shader "Custom/PixelPerfectSplitBorderShader"
{
    Properties
    {
        // Split color functionality
        _ColorA("Color A", Color) = (1,1,0,1)    // First color
        _ColorB("Color B", Color) = (1,0,0,1)    // Second color
        _Direction("Direction", Range(0,3)) = 0  // 0=Right,1=Left,2=Up,3=Down
        _Ratio("Ratio", Range(0,1)) = 0.5        // Split ratio

        // Border functionality
        _BorderColor("Border Color", Color) = (0,0,0,1)
        _BorderWidth("Border Width", Range(0,0.5)) = 0.05

        // Border edge visibility control
        [Toggle] _ShowLeft("Show Left Border", Float) = 1
        [Toggle] _ShowRight("Show Right Border", Float) = 1
        [Toggle] _ShowTop("Show Top Border", Float) = 1
        [Toggle] _ShowBottom("Show Bottom Border", Float) = 1

        // Border corner visibility control
        [Toggle] _ShowTopLeft("Show Top Left Corner", Float) = 1
        [Toggle] _ShowTopRight("Show Top Right Corner", Float) = 1
        [Toggle] _ShowBottomLeft("Show Bottom Left Corner", Float) = 1
        [Toggle] _ShowBottomRight("Show Bottom Right Corner", Float) = 1

        // Feature toggles
        [Toggle] _UseSplitColor("Use Split Color", Float) = 1
        [Toggle] _UseBorder("Use Border", Float) = 1

        // Pixel perfect settings
        _PixelsPerUnit("Pixels Per Unit", Float) = 100
        _PixelOffset("Pixel Offset", Vector) = (0,0,0,0)
    }

        SubShader
    {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual

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

            // Split color properties
            float4 _ColorA;
            float4 _ColorB;
            int _Direction;
            float _Ratio;

            // Border properties
            float4 _BorderColor;
            float _BorderWidth;

            // Edge visibility
            float _ShowLeft;
            float _ShowRight;
            float _ShowTop;
            float _ShowBottom;

            // Corner visibility
            float _ShowTopLeft;
            float _ShowTopRight;
            float _ShowBottomLeft;
            float _ShowBottomRight;

            // Feature toggles
            float _UseSplitColor;
            float _UseBorder;

            // Pixel perfect properties
            float _PixelsPerUnit;
            float4 _PixelOffset;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Apply pixel-perfect UV offset
                o.uv = v.uv + _PixelOffset.xy;
                return o;
            }

            // Get split color based on UV coordinates
            fixed4 GetSplitColor(float2 uv) {
                float t = 0;

                // Calculate gradient factor based on direction
                if (_Direction == 0) {          // Right (left -> right)
                    t = uv.x;
                }
                else if (_Direction == 1) {     // Left (right -> left)
                    t = 1.0 - uv.x;
                }
                else if (_Direction == 2) {     // Up (bottom -> top)
                    t = uv.y;
                }
                else if (_Direction == 3) {     // Down (top -> bottom)
                    t = 1.0 - uv.y;
                }

                // Return color based on UV position and ratio
                if (t < _Ratio)
                    return _ColorA;
                else
                    return _ColorB;
            }

            // Check if current position is in edge area (excluding corners) - Pixel Perfect version
            bool IsEdgeArea(float2 uv) {
                // Convert to pixel coordinates for precise calculation
                float2 pixelUV = uv * _PixelsPerUnit;
                float borderPixels = _BorderWidth * _PixelsPerUnit;

                // Use rounded values for consistent results
                float safeBorder = round(borderPixels);
                float2 safeUV = round(pixelUV);

                // Calculate safe inner bounds
                float innerStart = safeBorder;
                float innerEnd = _PixelsPerUnit - safeBorder;

                bool leftEdge = _ShowLeft > 0.5 && safeUV.x < safeBorder&&
                               safeUV.y >= innerStart && safeUV.y <= innerEnd;

                bool rightEdge = _ShowRight > 0.5 && safeUV.x > (_PixelsPerUnit - safeBorder) &&
                                safeUV.y >= innerStart && safeUV.y <= innerEnd;

                bool bottomEdge = _ShowBottom > 0.5 && safeUV.y < safeBorder&&
                                 safeUV.x >= innerStart && safeUV.x <= innerEnd;

                bool topEdge = _ShowTop > 0.5 && safeUV.y > (_PixelsPerUnit - safeBorder) &&
                              safeUV.x >= innerStart && safeUV.x <= innerEnd;

                return leftEdge || rightEdge || bottomEdge || topEdge;
            }

            // Check if current position is in corner area - Pixel Perfect version
            bool IsCornerArea(float2 uv) {
                // Convert to pixel coordinates for precise calculation
                float2 pixelUV = uv * _PixelsPerUnit;
                float borderPixels = _BorderWidth * _PixelsPerUnit;

                // Use rounded values for consistent results
                float safeBorder = round(borderPixels);
                float2 safeUV = round(pixelUV);

                bool topLeft = _ShowTopLeft > 0.5 && safeUV.x < safeBorder&&
                              safeUV.y >(_PixelsPerUnit - safeBorder);
                bool topRight = _ShowTopRight > 0.5 && safeUV.x > (_PixelsPerUnit - safeBorder) &&
                               safeUV.y > (_PixelsPerUnit - safeBorder);
                bool bottomLeft = _ShowBottomLeft > 0.5 && safeUV.x < safeBorder&&
                                 safeUV.y < safeBorder;
                bool bottomRight = _ShowBottomRight > 0.5 && safeUV.x > (_PixelsPerUnit - safeBorder) &&
                                  safeUV.y < safeBorder;

                return topLeft || topRight || bottomLeft || bottomRight;
            }

            // Check if current position is in any border area
            bool IsBorderArea(float2 uv) {
                if (_UseBorder < 0.5) return false;
                return IsEdgeArea(uv) || IsCornerArea(uv);
            }

            fixed4 frag(v2f i) : SV_Target {
                // First check if this is border area
                if (_UseBorder > 0.5 && IsBorderArea(i.uv)) {
                    return _BorderColor; // Border areas use solid border color
                }

            // For non-border areas, apply split color logic
            if (_UseSplitColor > 0.5) {
                return GetSplitColor(i.uv);
            }
            else {
                // If split color is disabled, use Color A as default
                return _ColorA;
            }
        }
        ENDCG
    }
    }

        CustomEditor "PixelPerfectSplitBorderShaderEditor"
}