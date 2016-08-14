Shader "CubePainter/Gradient Color" {
        Properties {
                _Color ("Main Color", Color) = (1,1,1,1)
                _Color2 ("Main Color 2", Color) = (1,1,1,1)
        }
        SubShader {
            Tags { "RenderType"="Opaque" }
			Lighting Off
			LOD 200
			
				
            CGPROGRAM
			#pragma surface surf NoLighting  noambient


            fixed4 _Color;
			fixed4 _Color2;

            struct Input {
                float4 screenPos;
            };
			fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
			{
				fixed4 c;
				c.rgb = s.Albedo;
				return c;
			}
            void surf (Input IN, inout SurfaceOutput o) 
            {
				float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
				fixed4 c = lerp(_Color, _Color2, screenUV.y);
                o.Albedo = c.rgb;
            }
            ENDCG
        } 

}