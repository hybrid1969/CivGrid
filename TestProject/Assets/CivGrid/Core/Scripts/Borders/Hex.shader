Shader "Hexagon"
{

// This shader is pretty inefficient on mobile, maybe find a way to do it without a surface shader. Its a transparent shader as I need
// it to be transparent for my implementation of fog, which appears beneath the world. Removing that capability should increase speed also.

    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
		_GridTex("Grid Texture", 2D) = "white" {}
        _BorderTex ("Border Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"IgnoreProjector"="True" "RenderType"="Opaque"}
        LOD 200
 
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
 
        CGPROGRAM
        #pragma surface surf Lambert
 
		 
		float4 BlendMethod( float4 a, float4 b ){
		
			return a * b; 					// Default
//		    return (a+b-1); 				// Severe.
//			return (1-(1-a)*b);				// Nice effect (keeps white border) but inverts & lightens colours.
			
		}
		
        sampler2D _MainTex;
		//sampler2D _GridTex;
        sampler2D _BlendTex;

        struct Input
        {
            float2 uv_MainTex;
    		float2 uv2_BlendTex;
    		float4 color: Color; // Vertex color
        };
 
       void surf (Input IN, inout SurfaceOutput o)
	   {
	     fixed4 mainCol = tex2D(_MainTex, IN.uv_MainTex);
	 	 fixed4 texTwoCol = BlendMethod( tex2D(_BlendTex, IN.uv2_BlendTex), IN.color.rgba );
	 
	     fixed4 mainOutput = mainCol.rgba * (1.0 - texTwoCol.a);
	     fixed4 blendOutput = texTwoCol.rgba * texTwoCol.a;        

	     o.Albedo = ( mainOutput.rgb + blendOutput.rgb );
	     o.Alpha = ( mainOutput.a + blendOutput.a );
	   }
        ENDCG
    } 
    FallBack "Diffuse"
}