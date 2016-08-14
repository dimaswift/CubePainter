Shader "CubePainter/UI Gradient" {
	Properties{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Color 1", Color) = (1,1,1,1)
		_Color2("Color 2", Color) = (1,1,1,1)


		// these six unused properties are required when a shader
		// is used in the UI system, or you get a warning.
		// look to UI-Default.shader to see these.
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
		// see for example
		// http://answers.unity3d.com/questions/980924/ui-mask-with-shader.html

	}

		SubShader{
		Tags{ "Queque" = "Transparent" }
		Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha

		//	Blend One OneMinusSrcAlpha

		Pass{
		CGPROGRAM
#pragma vertex vert  
#pragma fragment frag
#include "UnityCG.cginc"


		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color : COLOR;
			half2 texcoord  : TEXCOORD0;
		};

		fixed4 _Color;
		fixed4 _Color2;
		float _Offset;
		v2f vert(appdata_t IN)
		{
			v2f OUT;
			OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
			OUT.texcoord = IN.texcoord;
			OUT.color = IN.color * lerp(_Color, _Color2, IN.texcoord.y - _Offset);
			return OUT;
		}

		sampler2D _MainTex;

		fixed4 frag(v2f IN) : SV_Target
		{
			fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
			c.rgb *= c.a;
			return c;
		}
		ENDCG
	}
	}
}