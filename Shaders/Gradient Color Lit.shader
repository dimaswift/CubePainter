Shader "CubePainter/Gradient Color Lit" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
		_Color2("Main Color 2", Color) = (1,1,1,1)
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }


		CGPROGRAM
#pragma surface surf Lambert

		fixed4 _Color;
	fixed4 _Color2;
	sampler2D _MainTex;
	struct Input
	{
		float2 uv_MainTex;
	};

	void surf(Input IN, inout SurfaceOutput o)
	{
		fixed4 c = lerp(_Color, _Color2, IN.uv_MainTex.y);
		o.Albedo = c.rgb;
	}
	ENDCG
	}
}
