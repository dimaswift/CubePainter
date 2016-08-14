Shader "CubePainter/SpriteGradient" {
	Properties{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Left Color", Color) = (1,1,1,1)
		_Color2("Right Color", Color) = (1,1,1,1)
		_Ofset("Offset", Float) = 1
	}
		SubShader{
		Tags{ "Queue" = "Background"  "IgnoreProjector" = "True" }
		LOD 100

		ZWrite Off

		Pass{
		CGPROGRAM
#pragma vertex vert  
#pragma fragment frag
#include "UnityCG.cginc"

		fixed4 _Color;
	fixed4 _Color2;

	struct v2f {
		float4 pos : SV_POSITION;
		fixed4 col : COLOR;
	};

	v2f vert(appdata_full v)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.col = lerp(_Color,_Color2, v.texcoord.y);
		//            o.col = half4( v.vertex.y, 0, 0, 1);
		return o;
	}


	float4 frag(v2f i) : COLOR{
		float4 c = i.col;

		return c;
	}
		ENDCG
	}
	}
}