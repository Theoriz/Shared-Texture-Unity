Shader "Hidden/Invert"
{
Properties {
	_RenderTex ("Render texture", 2D) = "white" {}
}
	SubShader 
	{
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			SetTexture [_RenderTex] { combine texture }
		}
	}
}