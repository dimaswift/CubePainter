Shader "CubePainter/Decal Diffuse Lit" 
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Decal("Decal", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		#pragma surface surf Lambert
		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_Decal;
		};
	 
		sampler2D _MainTex;
		sampler2D _Decal;

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 mainCol = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 texTwoCol = tex2D(_Decal, IN.uv_Decal);

			fixed4 mainOutput = mainCol.rgba * (1.0 - (texTwoCol.a));
			fixed4 blendOutput = texTwoCol.rgba * texTwoCol.a;

			o.Albedo = mainOutput.rgb + blendOutput.rgb;
		}
		ENDCG
	}
	Fallback "Diffuse"
}