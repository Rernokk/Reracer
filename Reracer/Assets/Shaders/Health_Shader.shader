Shader "MyShaders/Health_Shader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
    _Value ("Health Value", Range(0,1)) = 1
    _Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
    Blend SrcAlpha OneMinusSrcAlpha

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
      fixed _Value;
      fixed4 _Color;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _Color;
        if (i.uv.x > _Value){
          col.a = .25;
        }
				return col;
			}
			ENDCG
		}
	}
}
