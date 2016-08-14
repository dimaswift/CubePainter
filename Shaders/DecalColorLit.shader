Shader "CubePainter/Decal Color Lit" 
{
	Properties
	{
		_Color("Color", Color) = (0,0,0,1)
		_Decal("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		#pragma surface surf Lambert
		struct Input 
		{
			float2 uv_Decal;
		};

		sampler2D _Decal;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 decal = tex2D(_Decal, IN.uv_Decal);
			o.Albedo = _Color * (1.0 - (decal.a)) + (decal.rgba * decal.a);
		}
		ENDCG
	}
	Fallback "Diffuse"
}