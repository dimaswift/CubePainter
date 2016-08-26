Shader "CubePainter/Color Lit" {
	
    Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Emission("Emmisive Color", Color) = (0,0,0,0)

	}
	SubShader
	{
		Pass
		{
			Lighting On
			Material
			{
				Diffuse[_Color]
				Emission[_Emission]
			}
		}
	}
}
