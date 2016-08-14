Shader "CubePainter/Vertex Color Lit" {
	
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
	}

		SubShader{
		CGPROGRAM
#pragma surface surf Lambert
		struct Input {
		float4 color : COLOR;
	};
	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutput o) {
		o.Albedo = IN.color.rgb * _Color;
	}
	ENDCG
	}
}
