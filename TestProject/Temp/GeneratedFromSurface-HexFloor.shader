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
 
        
	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
	

	// ---- forward rendering base pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile_fwdbase
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#define UNITY_PASS_FORWARDBASE
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 19 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

        //#pragma surface surf Lambert
 
		 
		float4 BlendMethod( float4 a, float4 b ){
		
			return a * b; 					// Default
//		    return (a+b-1); 				// Severe.
//			return (1-(1-a)*b);				// Nice effect (keeps white border) but inverts & lightens colours.
			
		}
		
        sampler2D _MainTex;
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
        

// vertex-to-fragment interpolation data
#ifdef LIGHTMAP_OFF
struct v2f_surf {
  float4 pos : SV_POSITION;
  float4 pack0 : TEXCOORD0;
  fixed4 color : COLOR0;
  fixed3 normal : TEXCOORD1;
  fixed3 vlight : TEXCOORD2;
  LIGHTING_COORDS(3,4)
};
#endif
#ifndef LIGHTMAP_OFF
struct v2f_surf {
  float4 pos : SV_POSITION;
  float4 pack0 : TEXCOORD0;
  fixed4 color : COLOR0;
  float2 lmap : TEXCOORD1;
  LIGHTING_COORDS(2,3)
};
#endif
#ifndef LIGHTMAP_OFF
float4 unity_LightmapST;
#endif
float4 _MainTex_ST;
float4 _BlendTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _BlendTex);
  o.color = v.color;
  #ifndef LIGHTMAP_OFF
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif
  float3 worldN = mul((float3x3)_Object2World, SCALED_NORMAL);
  #ifdef LIGHTMAP_OFF
  o.normal = worldN;
  #endif

  // SH/ambient and vertex lights
  #ifdef LIGHTMAP_OFF
  float3 shlight = ShadeSH9 (float4(worldN,1.0));
  o.vlight = shlight;
  #ifdef VERTEXLIGHT_ON
  float3 worldPos = mul(_Object2World, v.vertex).xyz;
  o.vlight += Shade4PointLights (
    unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
    unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
    unity_4LightAtten0, worldPos, worldN );
  #endif // VERTEXLIGHT_ON
  #endif // LIGHTMAP_OFF

  // pass lighting information to pixel shader
  TRANSFER_VERTEX_TO_FRAGMENT(o);
  return o;
}
#ifndef LIGHTMAP_OFF
sampler2D unity_Lightmap;
#ifndef DIRLIGHTMAP_OFF
sampler2D unity_LightmapInd;
#endif
#endif

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  #ifdef UNITY_COMPILER_HLSL
  Input surfIN = (Input)0;
  #else
  Input surfIN;
  #endif
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_BlendTex = IN.pack0.zw;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  #ifdef LIGHTMAP_OFF
  o.Normal = IN.normal;
  #endif

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  fixed atten = LIGHT_ATTENUATION(IN);
  fixed4 c = 0;

  // realtime lighting: call lighting function
  #ifdef LIGHTMAP_OFF
  c = LightingLambert (o, _WorldSpaceLightPos0.xyz, atten);
  #endif // LIGHTMAP_OFF || DIRLIGHTMAP_OFF
  #ifdef LIGHTMAP_OFF
  c.rgb += o.Albedo * IN.vlight;
  #endif // LIGHTMAP_OFF

  // lightmaps:
  #ifndef LIGHTMAP_OFF
    #ifndef DIRLIGHTMAP_OFF
      // directional lightmaps
      fixed4 lmtex = tex2D(unity_Lightmap, IN.lmap.xy);
      fixed4 lmIndTex = tex2D(unity_LightmapInd, IN.lmap.xy);
      half3 lm = LightingLambert_DirLightmap(o, lmtex, lmIndTex, 0).rgb;
    #else // !DIRLIGHTMAP_OFF
      // single lightmap
      fixed4 lmtex = tex2D(unity_Lightmap, IN.lmap.xy);
      fixed3 lm = DecodeLightmap (lmtex);
    #endif // !DIRLIGHTMAP_OFF

    // combine lightmaps with realtime shadows
    #ifdef SHADOWS_SCREEN
      #if (defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)) && defined(SHADER_API_MOBILE)
      c.rgb += o.Albedo * min(lm, atten*2);
      #else
      c.rgb += o.Albedo * max(min(lm,(atten*2)*lmtex.rgb), lm*atten);
      #endif
    #else // SHADOWS_SCREEN
      c.rgb += o.Albedo * lm;
    #endif // SHADOWS_SCREEN
  c.a = o.Alpha;
  #endif // LIGHTMAP_OFF

  return c;
}

ENDCG

}

	// ---- forward rendering additive lights pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardAdd" }
		ZWrite Off Blend One One Fog { Color (0,0,0,0) }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile_fwdadd
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#define UNITY_PASS_FORWARDADD
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 19 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

        //#pragma surface surf Lambert
 
		 
		float4 BlendMethod( float4 a, float4 b ){
		
			return a * b; 					// Default
//		    return (a+b-1); 				// Severe.
//			return (1-(1-a)*b);				// Nice effect (keeps white border) but inverts & lightens colours.
			
		}
		
        sampler2D _MainTex;
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
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  float4 pack0 : TEXCOORD0;
  fixed4 color : COLOR0;
  fixed3 normal : TEXCOORD1;
  half3 lightDir : TEXCOORD2;
  LIGHTING_COORDS(3,4)
};
float4 _MainTex_ST;
float4 _BlendTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _BlendTex);
  o.color = v.color;
  o.normal = mul((float3x3)_Object2World, SCALED_NORMAL);
  float3 lightDir = WorldSpaceLightDir( v.vertex );
  o.lightDir = lightDir;

  // pass lighting information to pixel shader
  TRANSFER_VERTEX_TO_FRAGMENT(o);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  #ifdef UNITY_COMPILER_HLSL
  Input surfIN = (Input)0;
  #else
  Input surfIN;
  #endif
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_BlendTex = IN.pack0.zw;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  o.Normal = IN.normal;

  // call surface function
  surf (surfIN, o);
  #ifndef USING_DIRECTIONAL_LIGHT
  fixed3 lightDir = normalize(IN.lightDir);
  #else
  fixed3 lightDir = IN.lightDir;
  #endif
  fixed4 c = LightingLambert (o, lightDir, LIGHT_ATTENUATION(IN));
  c.a = 0.0;
  return c;
}

ENDCG

}

	// ---- deferred lighting base geometry pass:
	Pass {
		Name "PREPASS"
		Tags { "LightMode" = "PrePassBase" }
		Fog {Mode Off}

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf

#pragma exclude_renderers flash
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#define UNITY_PASS_PREPASSBASE
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 19 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

        //#pragma surface surf Lambert
 
		 
		float4 BlendMethod( float4 a, float4 b ){
		
			return a * b; 					// Default
//		    return (a+b-1); 				// Severe.
//			return (1-(1-a)*b);				// Nice effect (keeps white border) but inverts & lightens colours.
			
		}
		
        sampler2D _MainTex;
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
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  fixed3 normal : TEXCOORD0;
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.normal = mul((float3x3)_Object2World, SCALED_NORMAL);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  #ifdef UNITY_COMPILER_HLSL
  Input surfIN = (Input)0;
  #else
  Input surfIN;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  o.Normal = IN.normal;

  // call surface function
  surf (surfIN, o);

  // output normal and specular
  fixed4 res;
  res.rgb = o.Normal * 0.5 + 0.5;
  res.a = o.Specular;
  return res;
}

ENDCG

}

	// ---- deferred lighting final pass:
	Pass {
		Name "PREPASS"
		Tags { "LightMode" = "PrePassFinal" }
		ZWrite Off

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile_prepassfinal
#pragma exclude_renderers flash
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#define UNITY_PASS_PREPASSFINAL
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 19 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

        //#pragma surface surf Lambert
 
		 
		float4 BlendMethod( float4 a, float4 b ){
		
			return a * b; 					// Default
//		    return (a+b-1); 				// Severe.
//			return (1-(1-a)*b);				// Nice effect (keeps white border) but inverts & lightens colours.
			
		}
		
        sampler2D _MainTex;
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
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  float4 pack0 : TEXCOORD0;
  fixed4 color : COLOR0;
  float4 screen : TEXCOORD1;
#ifdef LIGHTMAP_OFF
  float3 vlight : TEXCOORD2;
#else
  float2 lmap : TEXCOORD2;
#ifdef DIRLIGHTMAP_OFF
  float4 lmapFadePos : TEXCOORD3;
#endif
#endif
};
#ifndef LIGHTMAP_OFF
float4 unity_LightmapST;
#endif
float4 _MainTex_ST;
float4 _BlendTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _BlendTex);
  o.color = v.color;
  o.screen = ComputeScreenPos (o.pos);
#ifndef LIGHTMAP_OFF
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(_Object2World, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-mul(UNITY_MATRIX_MV, v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  float3 worldN = mul((float3x3)_Object2World, SCALED_NORMAL);
  o.vlight = ShadeSH9 (float4(worldN,1.0));
#endif
  return o;
}
sampler2D _LightBuffer;
#if defined (SHADER_API_XBOX360) && defined (HDR_LIGHT_PREPASS_ON)
sampler2D _LightSpecBuffer;
#endif
#ifndef LIGHTMAP_OFF
sampler2D unity_Lightmap;
sampler2D unity_LightmapInd;
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  #ifdef UNITY_COMPILER_HLSL
  Input surfIN = (Input)0;
  #else
  Input surfIN;
  #endif
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_BlendTex = IN.pack0.zw;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;

  // call surface function
  surf (surfIN, o);
  half4 light = tex2Dproj (_LightBuffer, UNITY_PROJ_COORD(IN.screen));
#if defined (SHADER_API_GLES) || defined (SHADER_API_GLES3)
  light = max(light, half4(0.001));
#endif
#ifndef HDR_LIGHT_PREPASS_ON
  light = -log2(light);
#endif
#if defined (SHADER_API_XBOX360) && defined (HDR_LIGHT_PREPASS_ON)
  light.w = tex2Dproj (_LightSpecBuffer, UNITY_PROJ_COORD(IN.screen)).r;
#endif

  // add lighting from lightmaps / vertex / ambient:
  #ifndef LIGHTMAP_OFF
    #ifdef DIRLIGHTMAP_OFF
      // dual lightmaps
      fixed4 lmtex = tex2D(unity_Lightmap, IN.lmap.xy);
      fixed4 lmtex2 = tex2D(unity_LightmapInd, IN.lmap.xy);
      half lmFade = length (IN.lmapFadePos) * unity_LightmapFade.z + unity_LightmapFade.w;
      half3 lmFull = DecodeLightmap (lmtex);
      half3 lmIndirect = DecodeLightmap (lmtex2);
      half3 lm = lerp (lmIndirect, lmFull, saturate(lmFade));
      light.rgb += lm;
    #else
      // directional lightmaps
      fixed4 lmtex = tex2D(unity_Lightmap, IN.lmap.xy);
      fixed4 lmIndTex = tex2D(unity_LightmapInd, IN.lmap.xy);
      half4 lm = LightingLambert_DirLightmap(o, lmtex, lmIndTex, 0);
      light += lm;
    #endif
  #else
    light.rgb += IN.vlight;
  #endif
  half4 c = LightingLambert_PrePass (o, light);
  return c;
}

ENDCG

}

	// ---- end of surface shader generated code

#LINE 54

    } 
    FallBack "Diffuse"
}