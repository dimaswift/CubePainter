Shader "CubePainter/Screen Gradient Color Lit" {
        Properties {
                _Color ("Main Color", Color) = (1,1,1,1)
                _Color2 ("Main Color 2", Color) = (1,1,1,1)
        }
        SubShader {
            Tags { "RenderType"="Opaque" }


            CGPROGRAM
			#pragma surface surf Lambert

            fixed4 _Color;
			fixed4 _Color2;

            struct Input 
			{
                 float4 screenPos;
            };

            void surf (Input IN, inout SurfaceOutput o) 
            {
				float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
				fixed4 c = lerp(_Color, _Color2, screenUV.y);
                o.Albedo = c.rgb;
            }
            ENDCG
        } 
}