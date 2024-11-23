Shader "Unlit/LEDDisplay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixShape("Pixel Shape Texture", 2D) = "white" {}
        _UV_X("Pixel num x", Range(10,1600)) = 960
        _UV_Y("Pixel num y", Range(10,1600)) = 360
        _Intensity("intensity", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex, _PixShape;
            float4 _MainTex_ST, _PixShape_ST;
            float _UV_X, _UV_Y, _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

		//ècâ°âΩå¬ï¿Ç◊ÇÈÇ©
                float2 uv_res = float2(_UV_X, _UV_Y);
                fixed4 col = tex2D(_MainTex,i.uv);
		
		//âÊëf
                float2 uv = i.uv * uv_res;
                float4 pix = tex2D(_PixShape, uv);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * pix * _Intensity;
            }
            ENDCG
        }
    }
}