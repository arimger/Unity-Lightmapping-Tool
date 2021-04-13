Shader "Hidden/Lightmap Blend"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_BlendTex("Blend Texture", 2D) = "white" {}
		_Blend("Texture Blend", Range(0,1)) = 0.0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
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

				sampler2D _MainTex;
				sampler2D _BlendTex;
				float4 _MainTex_ST;
				float4 _BlendTex_ST;

				half _Blend;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = lerp(tex2D(_MainTex, i.uv), tex2D(_BlendTex, i.uv), _Blend);
					return col;
				}
				ENDCG
			}
		}
}
