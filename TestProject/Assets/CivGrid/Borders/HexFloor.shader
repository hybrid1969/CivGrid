Shader "HexFloor"
{

// This shader is pretty inefficient on mobile, maybe find a way to do it without a surface shader. Its a transparent shader as I need
// it to be transparent for my implementation of fog, which appears beneath the world. Removing that capability should increase speed also.

    Properties
    {
//   		_Color ("_Color", Color) = (1,0,0,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BlendTex ("_BlendTex", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 200
 
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
 
        CGPROGRAM
        #pragma surface surf Lambert
 
        sampler2D _MainTex;
        sampler2D _BlendTex;
//        float4 _Color;
 
        struct Input
        {
            float2 uv_MainTex;
    		float2 uv2_BlendTex;
        };
 
       void surf (Input IN, inout SurfaceOutput o)
	   {
	     fixed4 mainCol = tex2D(_MainTex, IN.uv_MainTex);
	     fixed4 texTwoCol = tex2D(_BlendTex, IN.uv2_BlendTex);// * _Color;                         
	 
	     fixed4 mainOutput = mainCol.rgba * (1.0 - texTwoCol.a);
	     fixed4 blendOutput = texTwoCol.rgba * texTwoCol.a;        

	     o.Albedo = mainOutput.rgb + blendOutput.rgb;
	     o.Alpha = mainOutput.a + blendOutput.a;
	   }
        ENDCG
    } 
    FallBack "Diffuse"
}