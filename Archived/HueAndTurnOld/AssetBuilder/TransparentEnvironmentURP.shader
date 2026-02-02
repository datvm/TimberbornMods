Shader "Shader Graphs/TransparentEnvironmentURP"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_MainTex("Albedo", 2D) = "white" {}
        [NoScaleOffset]_ColorMask("Color Mask", 2D) = "white" {}
        [NoScaleOffset]_AmbientOcclusion("Ambient Occlusion", 2D) = "white" {}
        [NoScaleOffset]_MetallicGlossMap("Metallic Map", 2D) = "black" {}
        [ToggleUI]_CutoutWithAlpha("Cutout With Alpha", Float) = 0
        [NoScaleOffset]_CutoutTex("Cutout texture", 2D) = "white" {}
        _Cutoff("Cutout With Alpha Threshold", Range(0, 1)) = 0.5
        [Normal][NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}
        [NoScaleOffset]_DetailAlbedoMap("Detail Albedo", 2D) = "grey" {}
        [NoScaleOffset]_DetailAlbedoMap2("Detail Albedo UV2", 2D) = "black" {}
        _DetailAlbedoUV2Color("Detail Albedo UV2 Color", Color) = (0.7450981, 0.7450981, 0.7450981, 1)
        [NoScaleOffset]_DetailAlbedoMap3("Detail Albedo UV3", 2D) = "black" {}
        _DetailAlbedoUV3Color("Detail Albedo UV3 Color", Color) = (1, 1, 1, 0)
        _DetailAlbedoUV3Gradient("Detail Albedo UV3 Gradient", Float) = 0.025
        _EmissionColor("Emission Color", Color) = (0, 0, 0, 0)
        [NoScaleOffset]_LightingMap("LightingMap", 2D) = "black" {}
        [HideInInspector]_LightingStrength("LightingStrength", Range(0, 1)) = 1
        _LightingHue("LightingHue", Range(0, 1)) = 0
        _Grayscale("Grayscale", Range(0, 1)) = 0
        [ToggleUI]_MainUVFromCoordinates("Replace main UV with coordinates", Float) = 0
        _MainUVFromCoordinatesScale("Replaced main UV scale", Float) = 1
        _HeightCutoff("HeightCutoff", Float) = -10000
        _Alpha("Alpha", Float) = 1
        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            "DisableBatching"="False"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalLitSubTarget"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
        #pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
        #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _LIGHT_LAYERS
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _LIGHT_COOKIES
        #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
        #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 color;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float3 AbsoluteWorldSpacePosition;
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion : INTERP3;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord : INTERP4;
            #endif
             float4 tangentWS : INTERP5;
             float4 texCoord0 : INTERP6;
             float4 texCoord1 : INTERP7;
             float4 texCoord2 : INTERP8;
             float4 color : INTERP9;
             float4 fogFactorAndVertexLight : INTERP10;
             float3 positionWS : INTERP11;
             float3 normalWS : INTERP12;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.texCoord1.xyzw = input.texCoord1;
            output.texCoord2.xyzw = input.texCoord2;
            output.color.xyzw = input.color;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.texCoord1 = input.texCoord1.xyzw;
            output.texCoord2 = input.texCoord2.xyzw;
            output.color = input.color.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _ColorMask_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _AmbientOcclusion_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float _CutoutWithAlpha;
        float4 _CutoutTex_TexelSize;
        float _Cutoff;
        float4 _BumpMap_TexelSize;
        float4 _DetailAlbedoMap_TexelSize;
        float4 _DetailAlbedoMap3_TexelSize;
        float4 _DetailAlbedoUV3Color;
        float4 _DetailAlbedoMap2_TexelSize;
        float4 _EmissionColor;
        float4 _LightingMap_TexelSize;
        float _LightingHue;
        float _LightingStrength;
        float _Grayscale;
        float _MainUVFromCoordinates;
        float _MainUVFromCoordinatesScale;
        float _HeightCutoff;
        float4 _DetailAlbedoUV2Color;
        float _DetailAlbedoUV3Gradient;
        float _Alpha;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_ColorMask);
        SAMPLER(sampler_ColorMask);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AmbientOcclusion);
        SAMPLER(sampler_AmbientOcclusion);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_CutoutTex);
        SAMPLER(sampler_CutoutTex);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_DetailAlbedoMap);
        SAMPLER(sampler_DetailAlbedoMap);
        TEXTURE2D(_DetailAlbedoMap3);
        SAMPLER(sampler_DetailAlbedoMap3);
        TEXTURE2D(_DetailAlbedoMap2);
        SAMPLER(sampler_DetailAlbedoMap2);
        TEXTURE2D(_LightingMap);
        SAMPLER(sampler_LightingMap);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Maximum_float(float A, float B, out float Out)
        {
            Out = max(A, B);
        }
        
        struct Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float
        {
        };
        
        void SG_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float(float4 _UV, float _Width, Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float IN, out float Mask_1)
        {
        float _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float = _Width;
        float4 _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4 = _UV;
        float _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[0];
        float _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[1];
        float _Split_e846490befc44be4b6cf8357397b95fe_B_3_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[2];
        float _Split_e846490befc44be4b6cf8357397b95fe_A_4_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[3];
        float _Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float;
        Unity_Smoothstep_float(float(0), _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float, _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float, _Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float);
        float _Property_1de719632cea484faf21576b5c594898_Out_0_Float = _Width;
        float _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float;
        Unity_OneMinus_float(_Property_1de719632cea484faf21576b5c594898_Out_0_Float, _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float);
        float _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float;
        Unity_Smoothstep_float(float(1), _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float, _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float, _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float);
        float _Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float;
        Unity_Multiply_float_float(_Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float, _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float, _Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float);
        float _Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float;
        Unity_Smoothstep_float(float(0), _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float, _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float, _Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float);
        float _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float;
        Unity_Smoothstep_float(float(1), _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float, _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float, _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float);
        float _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float;
        Unity_Multiply_float_float(_Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float, _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float, _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float);
        float _Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float, _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float, _Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float);
        float _Property_e9852eb9b1364bb5b71a1bd94e4c7c79_Out_0_Float = _Width;
        float _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float;
        Unity_Step_float(_Property_e9852eb9b1364bb5b71a1bd94e4c7c79_Out_0_Float, float(1E-05), _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float);
        float _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float;
        Unity_Maximum_float(_Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float, _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float, _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float);
        Mask_1 = _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float;
        }
        
        void Unity_Hue_Normalized_float(float3 In, float Offset, out float3 Out)
        {
            // RGB to HSV
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
            float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
            float D = Q.x - min(Q.w, Q.y);
            float E = 1e-10;
            float V = (D == 0) ? Q.x : (Q.x + E);
            float3 hsv = float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), V);
        
            float hue = hsv.x + Offset;
            hsv.x = (hue < 0)
                    ? hue + 1
                    : (hue > 1)
                        ? hue - 1
                        : hue;
        
            // HSV to RGB
            float4 K2 = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
            float3 P2 = abs(frac(hsv.xxx + K2.xyz) * 6.0 - K2.www);
            Out = hsv.z * lerp(K2.xxx, saturate(P2 - K2.xxx), hsv.y);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Maximum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = max(A, B);
        }
        
        void Unity_Saturation_float(float3 In, float Saturation, out float3 Out)
        {
            float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
            Out =  luma.xxx + Saturation.xxx * (In - luma.xxx);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        struct Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float
        {
        };
        
        void SG_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float(float4 Vector4_2FA38543, float Vector1_29F6190A, float4 Vector4_28591918, Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float IN, out float4 OutAlbedo_1, out float4 OutEmission_2)
        {
        float4 _Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4 = Vector4_2FA38543;
        float _Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float = Vector1_29F6190A;
        float _OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float;
        Unity_OneMinus_float(_Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float, _OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float);
        float4 _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4;
        Unity_Multiply_float4_float4(_Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4, (_OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float.xxxx), _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4);
        float3 _Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3;
        Unity_Saturation_float((_Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4.xyz), float(0), _Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3);
        float4 Color_5f902b19e2dbee89ab43094f187a0eb4 = IsGammaSpace() ? float4(0.3962264, 0.3962264, 0.3962264, 0) : float4(SRGBToLinear(float3(0.3962264, 0.3962264, 0.3962264)), 0);
        float _Float_2fbe6741b5143883a862a8fbcabcc386_Out_0_Float = float(0.5);
        float3 _Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3;
        Unity_Lerp_float3(_Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3, (Color_5f902b19e2dbee89ab43094f187a0eb4.xyz), (_Float_2fbe6741b5143883a862a8fbcabcc386_Out_0_Float.xxx), _Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3);
        float3 _Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3, (_Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float.xxx), _Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3);
        float4 _Property_2939c3ea1e430783b7acaeb9a6240d53_Out_0_Vector4 = Vector4_28591918;
        float3 _Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3;
        Unity_Add_float3(_Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3, (_Property_2939c3ea1e430783b7acaeb9a6240d53_Out_0_Vector4.xyz), _Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3);
        OutAlbedo_1 = _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4;
        OutEmission_2 = (float4(_Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3, 1.0));
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap);
            float4 _UV_3896d539f97f4bd8babc947a396ab6d9_Out_0_Vector4 = IN.uv0;
            float4 _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.tex, _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.samplerstate, _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.GetTransformedUV((_UV_3896d539f97f4bd8babc947a396ab6d9_Out_0_Vector4.xy)) );
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_R_4_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.r;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_G_5_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.g;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_B_6_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.b;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_A_7_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.a;
            float4 _Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4, float4(2, 2, 2, 2), _Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4);
            UnityTexture2D _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean = _MainUVFromCoordinates;
            float _Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float = _MainUVFromCoordinatesScale;
            float4 _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4 = IN.AbsoluteWorldSpacePosition.xzxx;
            float4 _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float.xxxx), _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4);
            float4 _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4 = IN.uv0;
            float4 _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4;
            Unity_Branch_float4(_Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4, _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4, _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4);
            float4 _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.tex, _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.samplerstate, _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_R_4_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.r;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_G_5_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.g;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_B_6_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.b;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_A_7_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.a;
            float4 _Property_5b1d641fcde95684bb9a78f1d90b0f08_Out_0_Vector4 = _Color;
            float4 _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_5b1d641fcde95684bb9a78f1d90b0f08_Out_0_Vector4, _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4, _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4);
            UnityTexture2D _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_ColorMask);
            float4 _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.tex, _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.samplerstate, _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_R_4_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.r;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_G_5_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.g;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_B_6_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.b;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_A_7_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.a;
            float4 _Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4;
            Unity_Lerp_float4(_SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4, _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4, (_SampleTexture2D_3915df53a6424dddabe34b4432df379f_R_4_Float.xxxx), _Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4);
            float4 _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4;
            Unity_Saturate_float4(IN.VertexColor, _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4);
            float4 _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4, _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4, _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4);
            float4 _Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4, _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4, _Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4);
            float4 _Property_88d305cd73a646ceb547d741c837dbd5_Out_0_Vector4 = _DetailAlbedoUV2Color;
            UnityTexture2D _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap2);
            float4 _UV_35b3885c2cb245b2a63ae1909aebd057_Out_0_Vector4 = IN.uv1;
            float4 _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.tex, _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.samplerstate, _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.GetTransformedUV((_UV_35b3885c2cb245b2a63ae1909aebd057_Out_0_Vector4.xy)) );
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_R_4_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.r;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_G_5_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.g;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_B_6_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.b;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_A_7_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.a;
            float4 _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_88d305cd73a646ceb547d741c837dbd5_Out_0_Vector4, _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4, _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4);
            float _Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_R_4_Float, _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_A_7_Float, _Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float);
            float _Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float, 2, _Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float);
            float _Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float;
            Unity_Saturate_float(_Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float, _Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float);
            float4 _Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4;
            Unity_Lerp_float4(_Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4, _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4, (_Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float.xxxx), _Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4);
            UnityTexture2D _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap3);
            float4 _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.tex, _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.samplerstate, _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.GetTransformedUV(IN.uv2.xy) );
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_R_4_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.r;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_G_5_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.g;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_B_6_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.b;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_A_7_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.a;
            float4 _Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4, (_SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_A_7_Float.xxxx), _Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4);
            float4 _UV_648b68fa22f642daa873cffc512b2b17_Out_0_Vector4 = IN.uv2;
            float _Property_4913f3e665a84dc89bc96d4bd3a51974_Out_0_Float = _DetailAlbedoUV3Gradient;
            Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989;
            float _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float;
            SG_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float(_UV_648b68fa22f642daa873cffc512b2b17_Out_0_Vector4, _Property_4913f3e665a84dc89bc96d4bd3a51974_Out_0_Float, _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989, _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float);
            float4 _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4, (_EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float.xxxx), _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4);
            float _Split_8208581c2bcc443ab0479c6098551fba_R_1_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[0];
            float _Split_8208581c2bcc443ab0479c6098551fba_G_2_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[1];
            float _Split_8208581c2bcc443ab0479c6098551fba_B_3_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[2];
            float _Split_8208581c2bcc443ab0479c6098551fba_A_4_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[3];
            float4 _Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4;
            Unity_Lerp_float4(_Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4, _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4, (_Split_8208581c2bcc443ab0479c6098551fba_A_4_Float.xxxx), _Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4);
            float _Property_9133ccdf2f1c0f89913ba46e3c9a0199_Out_0_Float = _Grayscale;
            float4 _Property_6fefa341a6d19a85b76571c18da13700_Out_0_Vector4 = _EmissionColor;
            float _Property_635b04a2e39946a8b3f2bd5e93ac4ece_Out_0_Float = _LightingStrength;
            UnityTexture2D _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_LightingMap);
            float4 _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.tex, _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.samplerstate, _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_R_4_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.r;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_G_5_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.g;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_B_6_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.b;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_A_7_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.a;
            float _Property_b59eb330b570476fabb0c7eca2048a86_Out_0_Float = _LightingHue;
            float3 _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3;
            Unity_Hue_Normalized_float((_SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.xyz), _Property_b59eb330b570476fabb0c7eca2048a86_Out_0_Float, _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3);
            float3 _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Property_635b04a2e39946a8b3f2bd5e93ac4ece_Out_0_Float.xxx), _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3, _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3);
            float3 _Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3;
            Unity_Maximum_float3((_Property_6fefa341a6d19a85b76571c18da13700_Out_0_Vector4.xyz), _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3, _Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3);
            Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6;
            float4 _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4;
            float4 _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4;
            SG_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float(_Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4, _Property_9133ccdf2f1c0f89913ba46e3c9a0199_Out_0_Float, (float4(_Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3, 1.0)), _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6, _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4, _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4);
            UnityTexture2D _Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BumpMap);
            float4 _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D.tex, _Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D.samplerstate, _Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4);
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_R_4_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.r;
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_G_5_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.g;
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_B_6_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.b;
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_A_7_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.a;
            UnityTexture2D _Property_7742161dcede4cf98e95d8cc11eb1f5b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MetallicGlossMap);
            float4 _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_7742161dcede4cf98e95d8cc11eb1f5b_Out_0_Texture2D.tex, _Property_7742161dcede4cf98e95d8cc11eb1f5b_Out_0_Texture2D.samplerstate, _Property_7742161dcede4cf98e95d8cc11eb1f5b_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_R_4_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4.r;
            float _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_G_5_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4.g;
            float _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_B_6_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4.b;
            float _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_A_7_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4.a;
            float _Split_6d44c41d8d004448a9a86b271e819d73_R_1_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4[0];
            float _Split_6d44c41d8d004448a9a86b271e819d73_G_2_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4[1];
            float _Split_6d44c41d8d004448a9a86b271e819d73_B_3_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4[2];
            float _Split_6d44c41d8d004448a9a86b271e819d73_A_4_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4[3];
            UnityTexture2D _Property_c4c6ceb3accf49e1b82b7dd35162f2c5_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_AmbientOcclusion);
            float4 _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c4c6ceb3accf49e1b82b7dd35162f2c5_Out_0_Texture2D.tex, _Property_c4c6ceb3accf49e1b82b7dd35162f2c5_Out_0_Texture2D.samplerstate, _Property_c4c6ceb3accf49e1b82b7dd35162f2c5_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_R_4_Float = _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4.r;
            float _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_G_5_Float = _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4.g;
            float _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_B_6_Float = _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4.b;
            float _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_A_7_Float = _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4.a;
            float _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float = _Alpha;
            float _Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean = _CutoutWithAlpha;
            float _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float = _Cutoff;
            float _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float = _HeightCutoff;
            float _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float;
            Unity_Branch_float(_Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean, _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float, _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float, _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float);
            float _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float = float(1000);
            float _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            Unity_Add_float(_Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float, _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float, _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float);
            surface.BaseColor = (_GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4.xyz);
            surface.NormalTS = (_SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.xyz);
            surface.Emission = (_GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4.xyz);
            surface.Metallic = _Split_6d44c41d8d004448a9a86b271e819d73_R_1_Float;
            surface.Smoothness = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_A_7_Float;
            surface.Occlusion = (_SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4).x;
            surface.Alpha = _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float;
            surface.AlphaClipThreshold = _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles3 glcore
        #pragma multi_compile_instancing
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
        #pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_GBUFFER
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 color;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float3 AbsoluteWorldSpacePosition;
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion : INTERP3;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord : INTERP4;
            #endif
             float4 tangentWS : INTERP5;
             float4 texCoord0 : INTERP6;
             float4 texCoord1 : INTERP7;
             float4 texCoord2 : INTERP8;
             float4 color : INTERP9;
             float4 fogFactorAndVertexLight : INTERP10;
             float3 positionWS : INTERP11;
             float3 normalWS : INTERP12;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.texCoord1.xyzw = input.texCoord1;
            output.texCoord2.xyzw = input.texCoord2;
            output.color.xyzw = input.color;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.texCoord1 = input.texCoord1.xyzw;
            output.texCoord2 = input.texCoord2.xyzw;
            output.color = input.color.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _ColorMask_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _AmbientOcclusion_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float _CutoutWithAlpha;
        float4 _CutoutTex_TexelSize;
        float _Cutoff;
        float4 _BumpMap_TexelSize;
        float4 _DetailAlbedoMap_TexelSize;
        float4 _DetailAlbedoMap3_TexelSize;
        float4 _DetailAlbedoUV3Color;
        float4 _DetailAlbedoMap2_TexelSize;
        float4 _EmissionColor;
        float4 _LightingMap_TexelSize;
        float _LightingHue;
        float _LightingStrength;
        float _Grayscale;
        float _MainUVFromCoordinates;
        float _MainUVFromCoordinatesScale;
        float _HeightCutoff;
        float4 _DetailAlbedoUV2Color;
        float _DetailAlbedoUV3Gradient;
        float _Alpha;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_ColorMask);
        SAMPLER(sampler_ColorMask);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AmbientOcclusion);
        SAMPLER(sampler_AmbientOcclusion);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_CutoutTex);
        SAMPLER(sampler_CutoutTex);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_DetailAlbedoMap);
        SAMPLER(sampler_DetailAlbedoMap);
        TEXTURE2D(_DetailAlbedoMap3);
        SAMPLER(sampler_DetailAlbedoMap3);
        TEXTURE2D(_DetailAlbedoMap2);
        SAMPLER(sampler_DetailAlbedoMap2);
        TEXTURE2D(_LightingMap);
        SAMPLER(sampler_LightingMap);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Maximum_float(float A, float B, out float Out)
        {
            Out = max(A, B);
        }
        
        struct Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float
        {
        };
        
        void SG_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float(float4 _UV, float _Width, Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float IN, out float Mask_1)
        {
        float _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float = _Width;
        float4 _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4 = _UV;
        float _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[0];
        float _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[1];
        float _Split_e846490befc44be4b6cf8357397b95fe_B_3_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[2];
        float _Split_e846490befc44be4b6cf8357397b95fe_A_4_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[3];
        float _Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float;
        Unity_Smoothstep_float(float(0), _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float, _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float, _Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float);
        float _Property_1de719632cea484faf21576b5c594898_Out_0_Float = _Width;
        float _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float;
        Unity_OneMinus_float(_Property_1de719632cea484faf21576b5c594898_Out_0_Float, _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float);
        float _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float;
        Unity_Smoothstep_float(float(1), _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float, _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float, _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float);
        float _Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float;
        Unity_Multiply_float_float(_Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float, _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float, _Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float);
        float _Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float;
        Unity_Smoothstep_float(float(0), _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float, _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float, _Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float);
        float _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float;
        Unity_Smoothstep_float(float(1), _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float, _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float, _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float);
        float _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float;
        Unity_Multiply_float_float(_Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float, _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float, _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float);
        float _Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float, _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float, _Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float);
        float _Property_e9852eb9b1364bb5b71a1bd94e4c7c79_Out_0_Float = _Width;
        float _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float;
        Unity_Step_float(_Property_e9852eb9b1364bb5b71a1bd94e4c7c79_Out_0_Float, float(1E-05), _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float);
        float _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float;
        Unity_Maximum_float(_Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float, _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float, _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float);
        Mask_1 = _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float;
        }
        
        void Unity_Hue_Normalized_float(float3 In, float Offset, out float3 Out)
        {
            // RGB to HSV
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
            float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
            float D = Q.x - min(Q.w, Q.y);
            float E = 1e-10;
            float V = (D == 0) ? Q.x : (Q.x + E);
            float3 hsv = float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), V);
        
            float hue = hsv.x + Offset;
            hsv.x = (hue < 0)
                    ? hue + 1
                    : (hue > 1)
                        ? hue - 1
                        : hue;
        
            // HSV to RGB
            float4 K2 = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
            float3 P2 = abs(frac(hsv.xxx + K2.xyz) * 6.0 - K2.www);
            Out = hsv.z * lerp(K2.xxx, saturate(P2 - K2.xxx), hsv.y);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Maximum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = max(A, B);
        }
        
        void Unity_Saturation_float(float3 In, float Saturation, out float3 Out)
        {
            float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
            Out =  luma.xxx + Saturation.xxx * (In - luma.xxx);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        struct Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float
        {
        };
        
        void SG_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float(float4 Vector4_2FA38543, float Vector1_29F6190A, float4 Vector4_28591918, Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float IN, out float4 OutAlbedo_1, out float4 OutEmission_2)
        {
        float4 _Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4 = Vector4_2FA38543;
        float _Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float = Vector1_29F6190A;
        float _OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float;
        Unity_OneMinus_float(_Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float, _OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float);
        float4 _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4;
        Unity_Multiply_float4_float4(_Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4, (_OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float.xxxx), _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4);
        float3 _Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3;
        Unity_Saturation_float((_Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4.xyz), float(0), _Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3);
        float4 Color_5f902b19e2dbee89ab43094f187a0eb4 = IsGammaSpace() ? float4(0.3962264, 0.3962264, 0.3962264, 0) : float4(SRGBToLinear(float3(0.3962264, 0.3962264, 0.3962264)), 0);
        float _Float_2fbe6741b5143883a862a8fbcabcc386_Out_0_Float = float(0.5);
        float3 _Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3;
        Unity_Lerp_float3(_Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3, (Color_5f902b19e2dbee89ab43094f187a0eb4.xyz), (_Float_2fbe6741b5143883a862a8fbcabcc386_Out_0_Float.xxx), _Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3);
        float3 _Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3, (_Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float.xxx), _Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3);
        float4 _Property_2939c3ea1e430783b7acaeb9a6240d53_Out_0_Vector4 = Vector4_28591918;
        float3 _Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3;
        Unity_Add_float3(_Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3, (_Property_2939c3ea1e430783b7acaeb9a6240d53_Out_0_Vector4.xyz), _Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3);
        OutAlbedo_1 = _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4;
        OutEmission_2 = (float4(_Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3, 1.0));
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap);
            float4 _UV_3896d539f97f4bd8babc947a396ab6d9_Out_0_Vector4 = IN.uv0;
            float4 _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.tex, _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.samplerstate, _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.GetTransformedUV((_UV_3896d539f97f4bd8babc947a396ab6d9_Out_0_Vector4.xy)) );
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_R_4_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.r;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_G_5_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.g;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_B_6_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.b;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_A_7_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.a;
            float4 _Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4, float4(2, 2, 2, 2), _Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4);
            UnityTexture2D _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean = _MainUVFromCoordinates;
            float _Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float = _MainUVFromCoordinatesScale;
            float4 _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4 = IN.AbsoluteWorldSpacePosition.xzxx;
            float4 _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float.xxxx), _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4);
            float4 _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4 = IN.uv0;
            float4 _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4;
            Unity_Branch_float4(_Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4, _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4, _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4);
            float4 _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.tex, _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.samplerstate, _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_R_4_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.r;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_G_5_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.g;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_B_6_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.b;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_A_7_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.a;
            float4 _Property_5b1d641fcde95684bb9a78f1d90b0f08_Out_0_Vector4 = _Color;
            float4 _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_5b1d641fcde95684bb9a78f1d90b0f08_Out_0_Vector4, _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4, _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4);
            UnityTexture2D _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_ColorMask);
            float4 _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.tex, _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.samplerstate, _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_R_4_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.r;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_G_5_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.g;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_B_6_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.b;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_A_7_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.a;
            float4 _Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4;
            Unity_Lerp_float4(_SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4, _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4, (_SampleTexture2D_3915df53a6424dddabe34b4432df379f_R_4_Float.xxxx), _Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4);
            float4 _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4;
            Unity_Saturate_float4(IN.VertexColor, _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4);
            float4 _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4, _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4, _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4);
            float4 _Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4, _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4, _Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4);
            float4 _Property_88d305cd73a646ceb547d741c837dbd5_Out_0_Vector4 = _DetailAlbedoUV2Color;
            UnityTexture2D _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap2);
            float4 _UV_35b3885c2cb245b2a63ae1909aebd057_Out_0_Vector4 = IN.uv1;
            float4 _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.tex, _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.samplerstate, _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.GetTransformedUV((_UV_35b3885c2cb245b2a63ae1909aebd057_Out_0_Vector4.xy)) );
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_R_4_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.r;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_G_5_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.g;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_B_6_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.b;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_A_7_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.a;
            float4 _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_88d305cd73a646ceb547d741c837dbd5_Out_0_Vector4, _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4, _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4);
            float _Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_R_4_Float, _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_A_7_Float, _Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float);
            float _Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float, 2, _Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float);
            float _Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float;
            Unity_Saturate_float(_Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float, _Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float);
            float4 _Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4;
            Unity_Lerp_float4(_Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4, _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4, (_Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float.xxxx), _Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4);
            UnityTexture2D _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap3);
            float4 _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.tex, _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.samplerstate, _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.GetTransformedUV(IN.uv2.xy) );
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_R_4_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.r;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_G_5_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.g;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_B_6_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.b;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_A_7_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.a;
            float4 _Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4, (_SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_A_7_Float.xxxx), _Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4);
            float4 _UV_648b68fa22f642daa873cffc512b2b17_Out_0_Vector4 = IN.uv2;
            float _Property_4913f3e665a84dc89bc96d4bd3a51974_Out_0_Float = _DetailAlbedoUV3Gradient;
            Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989;
            float _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float;
            SG_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float(_UV_648b68fa22f642daa873cffc512b2b17_Out_0_Vector4, _Property_4913f3e665a84dc89bc96d4bd3a51974_Out_0_Float, _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989, _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float);
            float4 _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4, (_EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float.xxxx), _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4);
            float _Split_8208581c2bcc443ab0479c6098551fba_R_1_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[0];
            float _Split_8208581c2bcc443ab0479c6098551fba_G_2_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[1];
            float _Split_8208581c2bcc443ab0479c6098551fba_B_3_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[2];
            float _Split_8208581c2bcc443ab0479c6098551fba_A_4_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[3];
            float4 _Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4;
            Unity_Lerp_float4(_Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4, _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4, (_Split_8208581c2bcc443ab0479c6098551fba_A_4_Float.xxxx), _Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4);
            float _Property_9133ccdf2f1c0f89913ba46e3c9a0199_Out_0_Float = _Grayscale;
            float4 _Property_6fefa341a6d19a85b76571c18da13700_Out_0_Vector4 = _EmissionColor;
            float _Property_635b04a2e39946a8b3f2bd5e93ac4ece_Out_0_Float = _LightingStrength;
            UnityTexture2D _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_LightingMap);
            float4 _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.tex, _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.samplerstate, _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_R_4_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.r;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_G_5_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.g;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_B_6_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.b;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_A_7_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.a;
            float _Property_b59eb330b570476fabb0c7eca2048a86_Out_0_Float = _LightingHue;
            float3 _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3;
            Unity_Hue_Normalized_float((_SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.xyz), _Property_b59eb330b570476fabb0c7eca2048a86_Out_0_Float, _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3);
            float3 _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Property_635b04a2e39946a8b3f2bd5e93ac4ece_Out_0_Float.xxx), _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3, _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3);
            float3 _Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3;
            Unity_Maximum_float3((_Property_6fefa341a6d19a85b76571c18da13700_Out_0_Vector4.xyz), _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3, _Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3);
            Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6;
            float4 _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4;
            float4 _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4;
            SG_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float(_Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4, _Property_9133ccdf2f1c0f89913ba46e3c9a0199_Out_0_Float, (float4(_Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3, 1.0)), _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6, _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4, _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4);
            UnityTexture2D _Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BumpMap);
            float4 _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D.tex, _Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D.samplerstate, _Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4);
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_R_4_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.r;
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_G_5_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.g;
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_B_6_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.b;
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_A_7_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.a;
            UnityTexture2D _Property_7742161dcede4cf98e95d8cc11eb1f5b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MetallicGlossMap);
            float4 _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_7742161dcede4cf98e95d8cc11eb1f5b_Out_0_Texture2D.tex, _Property_7742161dcede4cf98e95d8cc11eb1f5b_Out_0_Texture2D.samplerstate, _Property_7742161dcede4cf98e95d8cc11eb1f5b_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_R_4_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4.r;
            float _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_G_5_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4.g;
            float _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_B_6_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4.b;
            float _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_A_7_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4.a;
            float _Split_6d44c41d8d004448a9a86b271e819d73_R_1_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4[0];
            float _Split_6d44c41d8d004448a9a86b271e819d73_G_2_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4[1];
            float _Split_6d44c41d8d004448a9a86b271e819d73_B_3_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4[2];
            float _Split_6d44c41d8d004448a9a86b271e819d73_A_4_Float = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_RGBA_0_Vector4[3];
            UnityTexture2D _Property_c4c6ceb3accf49e1b82b7dd35162f2c5_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_AmbientOcclusion);
            float4 _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c4c6ceb3accf49e1b82b7dd35162f2c5_Out_0_Texture2D.tex, _Property_c4c6ceb3accf49e1b82b7dd35162f2c5_Out_0_Texture2D.samplerstate, _Property_c4c6ceb3accf49e1b82b7dd35162f2c5_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_R_4_Float = _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4.r;
            float _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_G_5_Float = _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4.g;
            float _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_B_6_Float = _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4.b;
            float _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_A_7_Float = _SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4.a;
            float _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float = _Alpha;
            float _Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean = _CutoutWithAlpha;
            float _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float = _Cutoff;
            float _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float = _HeightCutoff;
            float _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float;
            Unity_Branch_float(_Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean, _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float, _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float, _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float);
            float _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float = float(1000);
            float _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            Unity_Add_float(_Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float, _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float, _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float);
            surface.BaseColor = (_GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4.xyz);
            surface.NormalTS = (_SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.xyz);
            surface.Emission = (_GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4.xyz);
            surface.Metallic = _Split_6d44c41d8d004448a9a86b271e819d73_R_1_Float;
            surface.Smoothness = _SampleTexture2D_00b7855f54d3467c9e0725c99c0a0d40_A_7_Float;
            surface.Occlusion = (_SampleTexture2D_3a300eb096bc495b89a5e6f2bb816957_RGBA_0_Vector4).x;
            surface.Alpha = _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float;
            surface.AlphaClipThreshold = _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
        
        // Render State
        Cull Back
        ZTest LEqual
        ZWrite On
        ColorMask 0
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_NORMAL_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _ColorMask_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _AmbientOcclusion_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float _CutoutWithAlpha;
        float4 _CutoutTex_TexelSize;
        float _Cutoff;
        float4 _BumpMap_TexelSize;
        float4 _DetailAlbedoMap_TexelSize;
        float4 _DetailAlbedoMap3_TexelSize;
        float4 _DetailAlbedoUV3Color;
        float4 _DetailAlbedoMap2_TexelSize;
        float4 _EmissionColor;
        float4 _LightingMap_TexelSize;
        float _LightingHue;
        float _LightingStrength;
        float _Grayscale;
        float _MainUVFromCoordinates;
        float _MainUVFromCoordinatesScale;
        float _HeightCutoff;
        float4 _DetailAlbedoUV2Color;
        float _DetailAlbedoUV3Gradient;
        float _Alpha;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_ColorMask);
        SAMPLER(sampler_ColorMask);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AmbientOcclusion);
        SAMPLER(sampler_AmbientOcclusion);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_CutoutTex);
        SAMPLER(sampler_CutoutTex);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_DetailAlbedoMap);
        SAMPLER(sampler_DetailAlbedoMap);
        TEXTURE2D(_DetailAlbedoMap3);
        SAMPLER(sampler_DetailAlbedoMap3);
        TEXTURE2D(_DetailAlbedoMap2);
        SAMPLER(sampler_DetailAlbedoMap2);
        TEXTURE2D(_LightingMap);
        SAMPLER(sampler_LightingMap);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float = _Alpha;
            float _Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean = _CutoutWithAlpha;
            float _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float = _Cutoff;
            float _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float = _HeightCutoff;
            float _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float;
            Unity_Branch_float(_Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean, _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float, _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float, _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float);
            float _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float = float(1000);
            float _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            Unity_Add_float(_Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float, _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float, _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float);
            surface.Alpha = _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float;
            surface.AlphaClipThreshold = _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "MotionVectors"
            Tags
            {
                "LightMode" = "MotionVectors"
            }
        
        // Render State
        Cull Back
        ZTest LEqual
        ZWrite On
        ColorMask RG
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.5
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_MOTION_VECTORS
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _ColorMask_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _AmbientOcclusion_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float _CutoutWithAlpha;
        float4 _CutoutTex_TexelSize;
        float _Cutoff;
        float4 _BumpMap_TexelSize;
        float4 _DetailAlbedoMap_TexelSize;
        float4 _DetailAlbedoMap3_TexelSize;
        float4 _DetailAlbedoUV3Color;
        float4 _DetailAlbedoMap2_TexelSize;
        float4 _EmissionColor;
        float4 _LightingMap_TexelSize;
        float _LightingHue;
        float _LightingStrength;
        float _Grayscale;
        float _MainUVFromCoordinates;
        float _MainUVFromCoordinatesScale;
        float _HeightCutoff;
        float4 _DetailAlbedoUV2Color;
        float _DetailAlbedoUV3Gradient;
        float _Alpha;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_ColorMask);
        SAMPLER(sampler_ColorMask);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AmbientOcclusion);
        SAMPLER(sampler_AmbientOcclusion);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_CutoutTex);
        SAMPLER(sampler_CutoutTex);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_DetailAlbedoMap);
        SAMPLER(sampler_DetailAlbedoMap);
        TEXTURE2D(_DetailAlbedoMap3);
        SAMPLER(sampler_DetailAlbedoMap3);
        TEXTURE2D(_DetailAlbedoMap2);
        SAMPLER(sampler_DetailAlbedoMap2);
        TEXTURE2D(_LightingMap);
        SAMPLER(sampler_LightingMap);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float = _Alpha;
            float _Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean = _CutoutWithAlpha;
            float _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float = _Cutoff;
            float _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float = _HeightCutoff;
            float _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float;
            Unity_Branch_float(_Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean, _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float, _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float, _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float);
            float _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float = float(1000);
            float _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            Unity_Add_float(_Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float, _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float, _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float);
            surface.Alpha = _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float;
            surface.AlphaClipThreshold = _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/MotionVectorPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }
        
        // Render State
        Cull Back
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALS
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float3 AbsoluteWorldSpacePosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 tangentWS : INTERP0;
             float4 texCoord0 : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _ColorMask_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _AmbientOcclusion_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float _CutoutWithAlpha;
        float4 _CutoutTex_TexelSize;
        float _Cutoff;
        float4 _BumpMap_TexelSize;
        float4 _DetailAlbedoMap_TexelSize;
        float4 _DetailAlbedoMap3_TexelSize;
        float4 _DetailAlbedoUV3Color;
        float4 _DetailAlbedoMap2_TexelSize;
        float4 _EmissionColor;
        float4 _LightingMap_TexelSize;
        float _LightingHue;
        float _LightingStrength;
        float _Grayscale;
        float _MainUVFromCoordinates;
        float _MainUVFromCoordinatesScale;
        float _HeightCutoff;
        float4 _DetailAlbedoUV2Color;
        float _DetailAlbedoUV3Gradient;
        float _Alpha;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_ColorMask);
        SAMPLER(sampler_ColorMask);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AmbientOcclusion);
        SAMPLER(sampler_AmbientOcclusion);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_CutoutTex);
        SAMPLER(sampler_CutoutTex);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_DetailAlbedoMap);
        SAMPLER(sampler_DetailAlbedoMap);
        TEXTURE2D(_DetailAlbedoMap3);
        SAMPLER(sampler_DetailAlbedoMap3);
        TEXTURE2D(_DetailAlbedoMap2);
        SAMPLER(sampler_DetailAlbedoMap2);
        TEXTURE2D(_LightingMap);
        SAMPLER(sampler_LightingMap);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 NormalTS;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BumpMap);
            float _Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean = _MainUVFromCoordinates;
            float _Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float = _MainUVFromCoordinatesScale;
            float4 _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4 = IN.AbsoluteWorldSpacePosition.xzxx;
            float4 _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float.xxxx), _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4);
            float4 _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4 = IN.uv0;
            float4 _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4;
            Unity_Branch_float4(_Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4, _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4, _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4);
            float4 _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D.tex, _Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D.samplerstate, _Property_1e3240792a18bd889f3bb247c6a9cc7e_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4);
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_R_4_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.r;
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_G_5_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.g;
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_B_6_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.b;
            float _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_A_7_Float = _SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.a;
            float _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float = _Alpha;
            float _Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean = _CutoutWithAlpha;
            float _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float = _Cutoff;
            float _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float = _HeightCutoff;
            float _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float;
            Unity_Branch_float(_Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean, _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float, _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float, _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float);
            float _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float = float(1000);
            float _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            Unity_Add_float(_Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float, _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float, _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float);
            surface.NormalTS = (_SampleTexture2D_3e70c317fa862085904fa6daba0d7b8b_RGBA_0_Vector4.xyz);
            surface.Alpha = _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float;
            surface.AlphaClipThreshold = _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature _ EDITOR_VISUALIZATION
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_COLOR
        #define ATTRIBUTES_NEED_INSTANCEID
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        #define _FOG_FRAGMENT 1
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 AbsoluteWorldSpacePosition;
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 texCoord1 : INTERP1;
             float4 texCoord2 : INTERP2;
             float4 color : INTERP3;
             float3 positionWS : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.texCoord1.xyzw = input.texCoord1;
            output.texCoord2.xyzw = input.texCoord2;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.texCoord1 = input.texCoord1.xyzw;
            output.texCoord2 = input.texCoord2.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _ColorMask_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _AmbientOcclusion_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float _CutoutWithAlpha;
        float4 _CutoutTex_TexelSize;
        float _Cutoff;
        float4 _BumpMap_TexelSize;
        float4 _DetailAlbedoMap_TexelSize;
        float4 _DetailAlbedoMap3_TexelSize;
        float4 _DetailAlbedoUV3Color;
        float4 _DetailAlbedoMap2_TexelSize;
        float4 _EmissionColor;
        float4 _LightingMap_TexelSize;
        float _LightingHue;
        float _LightingStrength;
        float _Grayscale;
        float _MainUVFromCoordinates;
        float _MainUVFromCoordinatesScale;
        float _HeightCutoff;
        float4 _DetailAlbedoUV2Color;
        float _DetailAlbedoUV3Gradient;
        float _Alpha;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_ColorMask);
        SAMPLER(sampler_ColorMask);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AmbientOcclusion);
        SAMPLER(sampler_AmbientOcclusion);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_CutoutTex);
        SAMPLER(sampler_CutoutTex);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_DetailAlbedoMap);
        SAMPLER(sampler_DetailAlbedoMap);
        TEXTURE2D(_DetailAlbedoMap3);
        SAMPLER(sampler_DetailAlbedoMap3);
        TEXTURE2D(_DetailAlbedoMap2);
        SAMPLER(sampler_DetailAlbedoMap2);
        TEXTURE2D(_LightingMap);
        SAMPLER(sampler_LightingMap);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Maximum_float(float A, float B, out float Out)
        {
            Out = max(A, B);
        }
        
        struct Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float
        {
        };
        
        void SG_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float(float4 _UV, float _Width, Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float IN, out float Mask_1)
        {
        float _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float = _Width;
        float4 _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4 = _UV;
        float _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[0];
        float _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[1];
        float _Split_e846490befc44be4b6cf8357397b95fe_B_3_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[2];
        float _Split_e846490befc44be4b6cf8357397b95fe_A_4_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[3];
        float _Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float;
        Unity_Smoothstep_float(float(0), _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float, _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float, _Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float);
        float _Property_1de719632cea484faf21576b5c594898_Out_0_Float = _Width;
        float _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float;
        Unity_OneMinus_float(_Property_1de719632cea484faf21576b5c594898_Out_0_Float, _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float);
        float _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float;
        Unity_Smoothstep_float(float(1), _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float, _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float, _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float);
        float _Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float;
        Unity_Multiply_float_float(_Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float, _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float, _Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float);
        float _Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float;
        Unity_Smoothstep_float(float(0), _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float, _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float, _Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float);
        float _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float;
        Unity_Smoothstep_float(float(1), _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float, _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float, _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float);
        float _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float;
        Unity_Multiply_float_float(_Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float, _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float, _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float);
        float _Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float, _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float, _Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float);
        float _Property_e9852eb9b1364bb5b71a1bd94e4c7c79_Out_0_Float = _Width;
        float _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float;
        Unity_Step_float(_Property_e9852eb9b1364bb5b71a1bd94e4c7c79_Out_0_Float, float(1E-05), _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float);
        float _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float;
        Unity_Maximum_float(_Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float, _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float, _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float);
        Mask_1 = _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float;
        }
        
        void Unity_Hue_Normalized_float(float3 In, float Offset, out float3 Out)
        {
            // RGB to HSV
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
            float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
            float D = Q.x - min(Q.w, Q.y);
            float E = 1e-10;
            float V = (D == 0) ? Q.x : (Q.x + E);
            float3 hsv = float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), V);
        
            float hue = hsv.x + Offset;
            hsv.x = (hue < 0)
                    ? hue + 1
                    : (hue > 1)
                        ? hue - 1
                        : hue;
        
            // HSV to RGB
            float4 K2 = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
            float3 P2 = abs(frac(hsv.xxx + K2.xyz) * 6.0 - K2.www);
            Out = hsv.z * lerp(K2.xxx, saturate(P2 - K2.xxx), hsv.y);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Maximum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = max(A, B);
        }
        
        void Unity_Saturation_float(float3 In, float Saturation, out float3 Out)
        {
            float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
            Out =  luma.xxx + Saturation.xxx * (In - luma.xxx);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        struct Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float
        {
        };
        
        void SG_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float(float4 Vector4_2FA38543, float Vector1_29F6190A, float4 Vector4_28591918, Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float IN, out float4 OutAlbedo_1, out float4 OutEmission_2)
        {
        float4 _Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4 = Vector4_2FA38543;
        float _Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float = Vector1_29F6190A;
        float _OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float;
        Unity_OneMinus_float(_Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float, _OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float);
        float4 _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4;
        Unity_Multiply_float4_float4(_Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4, (_OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float.xxxx), _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4);
        float3 _Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3;
        Unity_Saturation_float((_Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4.xyz), float(0), _Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3);
        float4 Color_5f902b19e2dbee89ab43094f187a0eb4 = IsGammaSpace() ? float4(0.3962264, 0.3962264, 0.3962264, 0) : float4(SRGBToLinear(float3(0.3962264, 0.3962264, 0.3962264)), 0);
        float _Float_2fbe6741b5143883a862a8fbcabcc386_Out_0_Float = float(0.5);
        float3 _Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3;
        Unity_Lerp_float3(_Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3, (Color_5f902b19e2dbee89ab43094f187a0eb4.xyz), (_Float_2fbe6741b5143883a862a8fbcabcc386_Out_0_Float.xxx), _Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3);
        float3 _Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3, (_Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float.xxx), _Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3);
        float4 _Property_2939c3ea1e430783b7acaeb9a6240d53_Out_0_Vector4 = Vector4_28591918;
        float3 _Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3;
        Unity_Add_float3(_Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3, (_Property_2939c3ea1e430783b7acaeb9a6240d53_Out_0_Vector4.xyz), _Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3);
        OutAlbedo_1 = _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4;
        OutEmission_2 = (float4(_Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3, 1.0));
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 Emission;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap);
            float4 _UV_3896d539f97f4bd8babc947a396ab6d9_Out_0_Vector4 = IN.uv0;
            float4 _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.tex, _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.samplerstate, _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.GetTransformedUV((_UV_3896d539f97f4bd8babc947a396ab6d9_Out_0_Vector4.xy)) );
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_R_4_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.r;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_G_5_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.g;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_B_6_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.b;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_A_7_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.a;
            float4 _Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4, float4(2, 2, 2, 2), _Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4);
            UnityTexture2D _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean = _MainUVFromCoordinates;
            float _Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float = _MainUVFromCoordinatesScale;
            float4 _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4 = IN.AbsoluteWorldSpacePosition.xzxx;
            float4 _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float.xxxx), _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4);
            float4 _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4 = IN.uv0;
            float4 _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4;
            Unity_Branch_float4(_Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4, _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4, _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4);
            float4 _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.tex, _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.samplerstate, _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_R_4_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.r;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_G_5_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.g;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_B_6_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.b;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_A_7_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.a;
            float4 _Property_5b1d641fcde95684bb9a78f1d90b0f08_Out_0_Vector4 = _Color;
            float4 _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_5b1d641fcde95684bb9a78f1d90b0f08_Out_0_Vector4, _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4, _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4);
            UnityTexture2D _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_ColorMask);
            float4 _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.tex, _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.samplerstate, _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_R_4_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.r;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_G_5_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.g;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_B_6_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.b;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_A_7_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.a;
            float4 _Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4;
            Unity_Lerp_float4(_SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4, _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4, (_SampleTexture2D_3915df53a6424dddabe34b4432df379f_R_4_Float.xxxx), _Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4);
            float4 _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4;
            Unity_Saturate_float4(IN.VertexColor, _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4);
            float4 _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4, _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4, _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4);
            float4 _Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4, _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4, _Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4);
            float4 _Property_88d305cd73a646ceb547d741c837dbd5_Out_0_Vector4 = _DetailAlbedoUV2Color;
            UnityTexture2D _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap2);
            float4 _UV_35b3885c2cb245b2a63ae1909aebd057_Out_0_Vector4 = IN.uv1;
            float4 _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.tex, _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.samplerstate, _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.GetTransformedUV((_UV_35b3885c2cb245b2a63ae1909aebd057_Out_0_Vector4.xy)) );
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_R_4_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.r;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_G_5_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.g;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_B_6_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.b;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_A_7_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.a;
            float4 _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_88d305cd73a646ceb547d741c837dbd5_Out_0_Vector4, _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4, _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4);
            float _Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_R_4_Float, _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_A_7_Float, _Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float);
            float _Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float, 2, _Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float);
            float _Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float;
            Unity_Saturate_float(_Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float, _Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float);
            float4 _Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4;
            Unity_Lerp_float4(_Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4, _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4, (_Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float.xxxx), _Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4);
            UnityTexture2D _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap3);
            float4 _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.tex, _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.samplerstate, _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.GetTransformedUV(IN.uv2.xy) );
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_R_4_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.r;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_G_5_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.g;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_B_6_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.b;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_A_7_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.a;
            float4 _Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4, (_SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_A_7_Float.xxxx), _Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4);
            float4 _UV_648b68fa22f642daa873cffc512b2b17_Out_0_Vector4 = IN.uv2;
            float _Property_4913f3e665a84dc89bc96d4bd3a51974_Out_0_Float = _DetailAlbedoUV3Gradient;
            Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989;
            float _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float;
            SG_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float(_UV_648b68fa22f642daa873cffc512b2b17_Out_0_Vector4, _Property_4913f3e665a84dc89bc96d4bd3a51974_Out_0_Float, _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989, _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float);
            float4 _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4, (_EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float.xxxx), _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4);
            float _Split_8208581c2bcc443ab0479c6098551fba_R_1_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[0];
            float _Split_8208581c2bcc443ab0479c6098551fba_G_2_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[1];
            float _Split_8208581c2bcc443ab0479c6098551fba_B_3_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[2];
            float _Split_8208581c2bcc443ab0479c6098551fba_A_4_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[3];
            float4 _Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4;
            Unity_Lerp_float4(_Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4, _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4, (_Split_8208581c2bcc443ab0479c6098551fba_A_4_Float.xxxx), _Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4);
            float _Property_9133ccdf2f1c0f89913ba46e3c9a0199_Out_0_Float = _Grayscale;
            float4 _Property_6fefa341a6d19a85b76571c18da13700_Out_0_Vector4 = _EmissionColor;
            float _Property_635b04a2e39946a8b3f2bd5e93ac4ece_Out_0_Float = _LightingStrength;
            UnityTexture2D _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_LightingMap);
            float4 _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.tex, _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.samplerstate, _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_R_4_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.r;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_G_5_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.g;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_B_6_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.b;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_A_7_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.a;
            float _Property_b59eb330b570476fabb0c7eca2048a86_Out_0_Float = _LightingHue;
            float3 _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3;
            Unity_Hue_Normalized_float((_SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.xyz), _Property_b59eb330b570476fabb0c7eca2048a86_Out_0_Float, _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3);
            float3 _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Property_635b04a2e39946a8b3f2bd5e93ac4ece_Out_0_Float.xxx), _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3, _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3);
            float3 _Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3;
            Unity_Maximum_float3((_Property_6fefa341a6d19a85b76571c18da13700_Out_0_Vector4.xyz), _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3, _Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3);
            Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6;
            float4 _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4;
            float4 _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4;
            SG_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float(_Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4, _Property_9133ccdf2f1c0f89913ba46e3c9a0199_Out_0_Float, (float4(_Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3, 1.0)), _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6, _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4, _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4);
            float _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float = _Alpha;
            float _Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean = _CutoutWithAlpha;
            float _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float = _Cutoff;
            float _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float = _HeightCutoff;
            float _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float;
            Unity_Branch_float(_Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean, _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float, _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float, _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float);
            float _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float = float(1000);
            float _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            Unity_Add_float(_Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float, _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float, _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float);
            surface.BaseColor = (_GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4.xyz);
            surface.Emission = (_GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4.xyz);
            surface.Alpha = _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float;
            surface.AlphaClipThreshold = _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _ColorMask_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _AmbientOcclusion_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float _CutoutWithAlpha;
        float4 _CutoutTex_TexelSize;
        float _Cutoff;
        float4 _BumpMap_TexelSize;
        float4 _DetailAlbedoMap_TexelSize;
        float4 _DetailAlbedoMap3_TexelSize;
        float4 _DetailAlbedoUV3Color;
        float4 _DetailAlbedoMap2_TexelSize;
        float4 _EmissionColor;
        float4 _LightingMap_TexelSize;
        float _LightingHue;
        float _LightingStrength;
        float _Grayscale;
        float _MainUVFromCoordinates;
        float _MainUVFromCoordinatesScale;
        float _HeightCutoff;
        float4 _DetailAlbedoUV2Color;
        float _DetailAlbedoUV3Gradient;
        float _Alpha;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_ColorMask);
        SAMPLER(sampler_ColorMask);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AmbientOcclusion);
        SAMPLER(sampler_AmbientOcclusion);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_CutoutTex);
        SAMPLER(sampler_CutoutTex);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_DetailAlbedoMap);
        SAMPLER(sampler_DetailAlbedoMap);
        TEXTURE2D(_DetailAlbedoMap3);
        SAMPLER(sampler_DetailAlbedoMap3);
        TEXTURE2D(_DetailAlbedoMap2);
        SAMPLER(sampler_DetailAlbedoMap2);
        TEXTURE2D(_LightingMap);
        SAMPLER(sampler_LightingMap);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float = _Alpha;
            float _Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean = _CutoutWithAlpha;
            float _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float = _Cutoff;
            float _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float = _HeightCutoff;
            float _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float;
            Unity_Branch_float(_Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean, _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float, _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float, _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float);
            float _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float = float(1000);
            float _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            Unity_Add_float(_Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float, _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float, _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float);
            surface.Alpha = _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float;
            surface.AlphaClipThreshold = _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 AbsoluteWorldSpacePosition;
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 texCoord1 : INTERP1;
             float4 texCoord2 : INTERP2;
             float4 color : INTERP3;
             float3 positionWS : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.texCoord1.xyzw = input.texCoord1;
            output.texCoord2.xyzw = input.texCoord2;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.texCoord1 = input.texCoord1.xyzw;
            output.texCoord2 = input.texCoord2.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _ColorMask_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _AmbientOcclusion_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float _CutoutWithAlpha;
        float4 _CutoutTex_TexelSize;
        float _Cutoff;
        float4 _BumpMap_TexelSize;
        float4 _DetailAlbedoMap_TexelSize;
        float4 _DetailAlbedoMap3_TexelSize;
        float4 _DetailAlbedoUV3Color;
        float4 _DetailAlbedoMap2_TexelSize;
        float4 _EmissionColor;
        float4 _LightingMap_TexelSize;
        float _LightingHue;
        float _LightingStrength;
        float _Grayscale;
        float _MainUVFromCoordinates;
        float _MainUVFromCoordinatesScale;
        float _HeightCutoff;
        float4 _DetailAlbedoUV2Color;
        float _DetailAlbedoUV3Gradient;
        float _Alpha;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_ColorMask);
        SAMPLER(sampler_ColorMask);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AmbientOcclusion);
        SAMPLER(sampler_AmbientOcclusion);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_CutoutTex);
        SAMPLER(sampler_CutoutTex);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_DetailAlbedoMap);
        SAMPLER(sampler_DetailAlbedoMap);
        TEXTURE2D(_DetailAlbedoMap3);
        SAMPLER(sampler_DetailAlbedoMap3);
        TEXTURE2D(_DetailAlbedoMap2);
        SAMPLER(sampler_DetailAlbedoMap2);
        TEXTURE2D(_LightingMap);
        SAMPLER(sampler_LightingMap);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Maximum_float(float A, float B, out float Out)
        {
            Out = max(A, B);
        }
        
        struct Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float
        {
        };
        
        void SG_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float(float4 _UV, float _Width, Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float IN, out float Mask_1)
        {
        float _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float = _Width;
        float4 _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4 = _UV;
        float _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[0];
        float _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[1];
        float _Split_e846490befc44be4b6cf8357397b95fe_B_3_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[2];
        float _Split_e846490befc44be4b6cf8357397b95fe_A_4_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[3];
        float _Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float;
        Unity_Smoothstep_float(float(0), _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float, _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float, _Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float);
        float _Property_1de719632cea484faf21576b5c594898_Out_0_Float = _Width;
        float _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float;
        Unity_OneMinus_float(_Property_1de719632cea484faf21576b5c594898_Out_0_Float, _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float);
        float _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float;
        Unity_Smoothstep_float(float(1), _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float, _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float, _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float);
        float _Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float;
        Unity_Multiply_float_float(_Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float, _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float, _Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float);
        float _Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float;
        Unity_Smoothstep_float(float(0), _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float, _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float, _Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float);
        float _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float;
        Unity_Smoothstep_float(float(1), _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float, _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float, _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float);
        float _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float;
        Unity_Multiply_float_float(_Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float, _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float, _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float);
        float _Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float, _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float, _Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float);
        float _Property_e9852eb9b1364bb5b71a1bd94e4c7c79_Out_0_Float = _Width;
        float _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float;
        Unity_Step_float(_Property_e9852eb9b1364bb5b71a1bd94e4c7c79_Out_0_Float, float(1E-05), _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float);
        float _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float;
        Unity_Maximum_float(_Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float, _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float, _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float);
        Mask_1 = _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float;
        }
        
        void Unity_Hue_Normalized_float(float3 In, float Offset, out float3 Out)
        {
            // RGB to HSV
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
            float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
            float D = Q.x - min(Q.w, Q.y);
            float E = 1e-10;
            float V = (D == 0) ? Q.x : (Q.x + E);
            float3 hsv = float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), V);
        
            float hue = hsv.x + Offset;
            hsv.x = (hue < 0)
                    ? hue + 1
                    : (hue > 1)
                        ? hue - 1
                        : hue;
        
            // HSV to RGB
            float4 K2 = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
            float3 P2 = abs(frac(hsv.xxx + K2.xyz) * 6.0 - K2.www);
            Out = hsv.z * lerp(K2.xxx, saturate(P2 - K2.xxx), hsv.y);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Maximum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = max(A, B);
        }
        
        void Unity_Saturation_float(float3 In, float Saturation, out float3 Out)
        {
            float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
            Out =  luma.xxx + Saturation.xxx * (In - luma.xxx);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        struct Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float
        {
        };
        
        void SG_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float(float4 Vector4_2FA38543, float Vector1_29F6190A, float4 Vector4_28591918, Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float IN, out float4 OutAlbedo_1, out float4 OutEmission_2)
        {
        float4 _Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4 = Vector4_2FA38543;
        float _Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float = Vector1_29F6190A;
        float _OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float;
        Unity_OneMinus_float(_Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float, _OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float);
        float4 _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4;
        Unity_Multiply_float4_float4(_Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4, (_OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float.xxxx), _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4);
        float3 _Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3;
        Unity_Saturation_float((_Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4.xyz), float(0), _Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3);
        float4 Color_5f902b19e2dbee89ab43094f187a0eb4 = IsGammaSpace() ? float4(0.3962264, 0.3962264, 0.3962264, 0) : float4(SRGBToLinear(float3(0.3962264, 0.3962264, 0.3962264)), 0);
        float _Float_2fbe6741b5143883a862a8fbcabcc386_Out_0_Float = float(0.5);
        float3 _Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3;
        Unity_Lerp_float3(_Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3, (Color_5f902b19e2dbee89ab43094f187a0eb4.xyz), (_Float_2fbe6741b5143883a862a8fbcabcc386_Out_0_Float.xxx), _Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3);
        float3 _Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3, (_Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float.xxx), _Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3);
        float4 _Property_2939c3ea1e430783b7acaeb9a6240d53_Out_0_Vector4 = Vector4_28591918;
        float3 _Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3;
        Unity_Add_float3(_Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3, (_Property_2939c3ea1e430783b7acaeb9a6240d53_Out_0_Vector4.xyz), _Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3);
        OutAlbedo_1 = _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4;
        OutEmission_2 = (float4(_Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3, 1.0));
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap);
            float4 _UV_3896d539f97f4bd8babc947a396ab6d9_Out_0_Vector4 = IN.uv0;
            float4 _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.tex, _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.samplerstate, _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.GetTransformedUV((_UV_3896d539f97f4bd8babc947a396ab6d9_Out_0_Vector4.xy)) );
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_R_4_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.r;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_G_5_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.g;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_B_6_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.b;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_A_7_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.a;
            float4 _Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4, float4(2, 2, 2, 2), _Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4);
            UnityTexture2D _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean = _MainUVFromCoordinates;
            float _Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float = _MainUVFromCoordinatesScale;
            float4 _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4 = IN.AbsoluteWorldSpacePosition.xzxx;
            float4 _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float.xxxx), _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4);
            float4 _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4 = IN.uv0;
            float4 _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4;
            Unity_Branch_float4(_Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4, _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4, _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4);
            float4 _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.tex, _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.samplerstate, _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_R_4_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.r;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_G_5_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.g;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_B_6_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.b;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_A_7_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.a;
            float4 _Property_5b1d641fcde95684bb9a78f1d90b0f08_Out_0_Vector4 = _Color;
            float4 _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_5b1d641fcde95684bb9a78f1d90b0f08_Out_0_Vector4, _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4, _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4);
            UnityTexture2D _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_ColorMask);
            float4 _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.tex, _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.samplerstate, _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_R_4_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.r;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_G_5_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.g;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_B_6_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.b;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_A_7_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.a;
            float4 _Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4;
            Unity_Lerp_float4(_SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4, _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4, (_SampleTexture2D_3915df53a6424dddabe34b4432df379f_R_4_Float.xxxx), _Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4);
            float4 _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4;
            Unity_Saturate_float4(IN.VertexColor, _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4);
            float4 _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4, _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4, _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4);
            float4 _Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4, _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4, _Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4);
            float4 _Property_88d305cd73a646ceb547d741c837dbd5_Out_0_Vector4 = _DetailAlbedoUV2Color;
            UnityTexture2D _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap2);
            float4 _UV_35b3885c2cb245b2a63ae1909aebd057_Out_0_Vector4 = IN.uv1;
            float4 _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.tex, _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.samplerstate, _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.GetTransformedUV((_UV_35b3885c2cb245b2a63ae1909aebd057_Out_0_Vector4.xy)) );
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_R_4_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.r;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_G_5_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.g;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_B_6_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.b;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_A_7_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.a;
            float4 _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_88d305cd73a646ceb547d741c837dbd5_Out_0_Vector4, _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4, _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4);
            float _Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_R_4_Float, _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_A_7_Float, _Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float);
            float _Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float, 2, _Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float);
            float _Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float;
            Unity_Saturate_float(_Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float, _Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float);
            float4 _Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4;
            Unity_Lerp_float4(_Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4, _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4, (_Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float.xxxx), _Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4);
            UnityTexture2D _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap3);
            float4 _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.tex, _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.samplerstate, _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.GetTransformedUV(IN.uv2.xy) );
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_R_4_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.r;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_G_5_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.g;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_B_6_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.b;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_A_7_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.a;
            float4 _Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4, (_SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_A_7_Float.xxxx), _Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4);
            float4 _UV_648b68fa22f642daa873cffc512b2b17_Out_0_Vector4 = IN.uv2;
            float _Property_4913f3e665a84dc89bc96d4bd3a51974_Out_0_Float = _DetailAlbedoUV3Gradient;
            Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989;
            float _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float;
            SG_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float(_UV_648b68fa22f642daa873cffc512b2b17_Out_0_Vector4, _Property_4913f3e665a84dc89bc96d4bd3a51974_Out_0_Float, _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989, _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float);
            float4 _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4, (_EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float.xxxx), _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4);
            float _Split_8208581c2bcc443ab0479c6098551fba_R_1_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[0];
            float _Split_8208581c2bcc443ab0479c6098551fba_G_2_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[1];
            float _Split_8208581c2bcc443ab0479c6098551fba_B_3_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[2];
            float _Split_8208581c2bcc443ab0479c6098551fba_A_4_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[3];
            float4 _Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4;
            Unity_Lerp_float4(_Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4, _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4, (_Split_8208581c2bcc443ab0479c6098551fba_A_4_Float.xxxx), _Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4);
            float _Property_9133ccdf2f1c0f89913ba46e3c9a0199_Out_0_Float = _Grayscale;
            float4 _Property_6fefa341a6d19a85b76571c18da13700_Out_0_Vector4 = _EmissionColor;
            float _Property_635b04a2e39946a8b3f2bd5e93ac4ece_Out_0_Float = _LightingStrength;
            UnityTexture2D _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_LightingMap);
            float4 _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.tex, _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.samplerstate, _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_R_4_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.r;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_G_5_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.g;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_B_6_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.b;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_A_7_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.a;
            float _Property_b59eb330b570476fabb0c7eca2048a86_Out_0_Float = _LightingHue;
            float3 _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3;
            Unity_Hue_Normalized_float((_SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.xyz), _Property_b59eb330b570476fabb0c7eca2048a86_Out_0_Float, _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3);
            float3 _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Property_635b04a2e39946a8b3f2bd5e93ac4ece_Out_0_Float.xxx), _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3, _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3);
            float3 _Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3;
            Unity_Maximum_float3((_Property_6fefa341a6d19a85b76571c18da13700_Out_0_Vector4.xyz), _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3, _Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3);
            Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6;
            float4 _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4;
            float4 _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4;
            SG_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float(_Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4, _Property_9133ccdf2f1c0f89913ba46e3c9a0199_Out_0_Float, (float4(_Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3, 1.0)), _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6, _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4, _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4);
            float _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float = _Alpha;
            float _Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean = _CutoutWithAlpha;
            float _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float = _Cutoff;
            float _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float = _HeightCutoff;
            float _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float;
            Unity_Branch_float(_Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean, _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float, _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float, _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float);
            float _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float = float(1000);
            float _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            Unity_Add_float(_Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float, _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float, _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float);
            surface.BaseColor = (_GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4.xyz);
            surface.Alpha = _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float;
            surface.AlphaClipThreshold = _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Universal 2D"
            Tags
            {
                "LightMode" = "Universal2D"
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 AbsoluteWorldSpacePosition;
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 texCoord1 : INTERP1;
             float4 texCoord2 : INTERP2;
             float4 color : INTERP3;
             float3 positionWS : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.texCoord1.xyzw = input.texCoord1;
            output.texCoord2.xyzw = input.texCoord2;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.texCoord1 = input.texCoord1.xyzw;
            output.texCoord2 = input.texCoord2.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _ColorMask_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _AmbientOcclusion_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float _CutoutWithAlpha;
        float4 _CutoutTex_TexelSize;
        float _Cutoff;
        float4 _BumpMap_TexelSize;
        float4 _DetailAlbedoMap_TexelSize;
        float4 _DetailAlbedoMap3_TexelSize;
        float4 _DetailAlbedoUV3Color;
        float4 _DetailAlbedoMap2_TexelSize;
        float4 _EmissionColor;
        float4 _LightingMap_TexelSize;
        float _LightingHue;
        float _LightingStrength;
        float _Grayscale;
        float _MainUVFromCoordinates;
        float _MainUVFromCoordinatesScale;
        float _HeightCutoff;
        float4 _DetailAlbedoUV2Color;
        float _DetailAlbedoUV3Gradient;
        float _Alpha;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_ColorMask);
        SAMPLER(sampler_ColorMask);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AmbientOcclusion);
        SAMPLER(sampler_AmbientOcclusion);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_CutoutTex);
        SAMPLER(sampler_CutoutTex);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_DetailAlbedoMap);
        SAMPLER(sampler_DetailAlbedoMap);
        TEXTURE2D(_DetailAlbedoMap3);
        SAMPLER(sampler_DetailAlbedoMap3);
        TEXTURE2D(_DetailAlbedoMap2);
        SAMPLER(sampler_DetailAlbedoMap2);
        TEXTURE2D(_LightingMap);
        SAMPLER(sampler_LightingMap);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Maximum_float(float A, float B, out float Out)
        {
            Out = max(A, B);
        }
        
        struct Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float
        {
        };
        
        void SG_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float(float4 _UV, float _Width, Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float IN, out float Mask_1)
        {
        float _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float = _Width;
        float4 _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4 = _UV;
        float _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[0];
        float _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[1];
        float _Split_e846490befc44be4b6cf8357397b95fe_B_3_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[2];
        float _Split_e846490befc44be4b6cf8357397b95fe_A_4_Float = _Property_00376f9da00f4418949a3de784046c7a_Out_0_Vector4[3];
        float _Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float;
        Unity_Smoothstep_float(float(0), _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float, _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float, _Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float);
        float _Property_1de719632cea484faf21576b5c594898_Out_0_Float = _Width;
        float _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float;
        Unity_OneMinus_float(_Property_1de719632cea484faf21576b5c594898_Out_0_Float, _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float);
        float _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float;
        Unity_Smoothstep_float(float(1), _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float, _Split_e846490befc44be4b6cf8357397b95fe_R_1_Float, _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float);
        float _Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float;
        Unity_Multiply_float_float(_Smoothstep_66b7afbdc967448f8ffa9b90730982e6_Out_3_Float, _Smoothstep_a8241f2b8fa948738050131a0713e8fa_Out_3_Float, _Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float);
        float _Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float;
        Unity_Smoothstep_float(float(0), _Property_bef08b7609554cf088cdcfdef887cfa4_Out_0_Float, _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float, _Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float);
        float _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float;
        Unity_Smoothstep_float(float(1), _OneMinus_22438e5f363c453dab5ca5b2005546c4_Out_1_Float, _Split_e846490befc44be4b6cf8357397b95fe_G_2_Float, _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float);
        float _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float;
        Unity_Multiply_float_float(_Smoothstep_ca6abbc9c74d4cf6ab5c05d993529f1c_Out_3_Float, _Smoothstep_6d27b351b69b4d5db7d11dd22d791376_Out_3_Float, _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float);
        float _Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_d2361efef25f4d1bab80082299cbc057_Out_2_Float, _Multiply_581e09a4020b44d08f08877070eb1681_Out_2_Float, _Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float);
        float _Property_e9852eb9b1364bb5b71a1bd94e4c7c79_Out_0_Float = _Width;
        float _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float;
        Unity_Step_float(_Property_e9852eb9b1364bb5b71a1bd94e4c7c79_Out_0_Float, float(1E-05), _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float);
        float _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float;
        Unity_Maximum_float(_Multiply_ad49be1c00104d43921baf249b55e56a_Out_2_Float, _Step_916dd95d4cbd488abfb17ed1552e53e3_Out_2_Float, _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float);
        Mask_1 = _Maximum_8c65ddf102cd4c76ab20c091cac05888_Out_2_Float;
        }
        
        void Unity_Hue_Normalized_float(float3 In, float Offset, out float3 Out)
        {
            // RGB to HSV
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
            float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
            float D = Q.x - min(Q.w, Q.y);
            float E = 1e-10;
            float V = (D == 0) ? Q.x : (Q.x + E);
            float3 hsv = float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), V);
        
            float hue = hsv.x + Offset;
            hsv.x = (hue < 0)
                    ? hue + 1
                    : (hue > 1)
                        ? hue - 1
                        : hue;
        
            // HSV to RGB
            float4 K2 = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
            float3 P2 = abs(frac(hsv.xxx + K2.xyz) * 6.0 - K2.www);
            Out = hsv.z * lerp(K2.xxx, saturate(P2 - K2.xxx), hsv.y);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Maximum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = max(A, B);
        }
        
        void Unity_Saturation_float(float3 In, float Saturation, out float3 Out)
        {
            float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
            Out =  luma.xxx + Saturation.xxx * (In - luma.xxx);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        struct Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float
        {
        };
        
        void SG_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float(float4 Vector4_2FA38543, float Vector1_29F6190A, float4 Vector4_28591918, Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float IN, out float4 OutAlbedo_1, out float4 OutEmission_2)
        {
        float4 _Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4 = Vector4_2FA38543;
        float _Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float = Vector1_29F6190A;
        float _OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float;
        Unity_OneMinus_float(_Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float, _OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float);
        float4 _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4;
        Unity_Multiply_float4_float4(_Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4, (_OneMinus_68ca2ec4acf5238fa9b1a6c388f24a24_Out_1_Float.xxxx), _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4);
        float3 _Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3;
        Unity_Saturation_float((_Property_3a0f6d5e34c3838b9ea6b27d8b1e761a_Out_0_Vector4.xyz), float(0), _Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3);
        float4 Color_5f902b19e2dbee89ab43094f187a0eb4 = IsGammaSpace() ? float4(0.3962264, 0.3962264, 0.3962264, 0) : float4(SRGBToLinear(float3(0.3962264, 0.3962264, 0.3962264)), 0);
        float _Float_2fbe6741b5143883a862a8fbcabcc386_Out_0_Float = float(0.5);
        float3 _Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3;
        Unity_Lerp_float3(_Saturation_29ba512bb6a12685a98be69abec76a02_Out_2_Vector3, (Color_5f902b19e2dbee89ab43094f187a0eb4.xyz), (_Float_2fbe6741b5143883a862a8fbcabcc386_Out_0_Float.xxx), _Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3);
        float3 _Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Lerp_e5ea9fe43fb7ab86b837d6671cd9b50b_Out_3_Vector3, (_Property_48f3c18765074d8a9483cc59a095cac2_Out_0_Float.xxx), _Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3);
        float4 _Property_2939c3ea1e430783b7acaeb9a6240d53_Out_0_Vector4 = Vector4_28591918;
        float3 _Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3;
        Unity_Add_float3(_Multiply_e6c446c02b724685a3dfd8621df25511_Out_2_Vector3, (_Property_2939c3ea1e430783b7acaeb9a6240d53_Out_0_Vector4.xyz), _Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3);
        OutAlbedo_1 = _Multiply_d1a93d47d3067588b775684437d567c6_Out_2_Vector4;
        OutEmission_2 = (float4(_Add_e6b5b1f1d7fd748fbda7e44ff5b3b690_Out_2_Vector3, 1.0));
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap);
            float4 _UV_3896d539f97f4bd8babc947a396ab6d9_Out_0_Vector4 = IN.uv0;
            float4 _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.tex, _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.samplerstate, _Property_c94b148769c4467da46885b7f152f039_Out_0_Texture2D.GetTransformedUV((_UV_3896d539f97f4bd8babc947a396ab6d9_Out_0_Vector4.xy)) );
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_R_4_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.r;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_G_5_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.g;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_B_6_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.b;
            float _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_A_7_Float = _SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4.a;
            float4 _Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_68c3dba0b05a4b9c85c9d6c40f10a5b3_RGBA_0_Vector4, float4(2, 2, 2, 2), _Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4);
            UnityTexture2D _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean = _MainUVFromCoordinates;
            float _Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float = _MainUVFromCoordinatesScale;
            float4 _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4 = IN.AbsoluteWorldSpacePosition.xzxx;
            float4 _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Property_8c65309be6cccb87ad93b05074f1c91a_Out_0_Float.xxxx), _Swizzle_aa37039dd6608e8393cc736bf1decf4b_Out_1_Vector4, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4);
            float4 _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4 = IN.uv0;
            float4 _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4;
            Unity_Branch_float4(_Property_8d64da136f403e8aa7a16c845f1c9a4d_Out_0_Boolean, _Multiply_a3523208db5ef9828aca0d8a33abb92a_Out_2_Vector4, _UV_0fbdec6e28c042889a71a3a0cbca0f5a_Out_0_Vector4, _Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4);
            float4 _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.tex, _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.samplerstate, _Property_78f3e2c4c7fafa87978b8b110ed85334_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_R_4_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.r;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_G_5_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.g;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_B_6_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.b;
            float _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_A_7_Float = _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4.a;
            float4 _Property_5b1d641fcde95684bb9a78f1d90b0f08_Out_0_Vector4 = _Color;
            float4 _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_5b1d641fcde95684bb9a78f1d90b0f08_Out_0_Vector4, _SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4, _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4);
            UnityTexture2D _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_ColorMask);
            float4 _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.tex, _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.samplerstate, _Property_27501a656be94646b68346fbd09b1238_Out_0_Texture2D.GetTransformedUV((_Branch_3a054f07bf5c908abf24521948fa3e0c_Out_3_Vector4.xy)) );
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_R_4_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.r;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_G_5_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.g;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_B_6_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.b;
            float _SampleTexture2D_3915df53a6424dddabe34b4432df379f_A_7_Float = _SampleTexture2D_3915df53a6424dddabe34b4432df379f_RGBA_0_Vector4.a;
            float4 _Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4;
            Unity_Lerp_float4(_SampleTexture2D_2c82db7e9cbf508aa415c87ef18ffb0e_RGBA_0_Vector4, _Multiply_c87bc4b195708783a88cb75fc4477007_Out_2_Vector4, (_SampleTexture2D_3915df53a6424dddabe34b4432df379f_R_4_Float.xxxx), _Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4);
            float4 _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4;
            Unity_Saturate_float4(IN.VertexColor, _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4);
            float4 _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Lerp_18dd2fa556bf489d8929d7f83b28e3c9_Out_3_Vector4, _Saturate_9252a8501d574039ad8aeee69879c75d_Out_1_Vector4, _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4);
            float4 _Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_98488fd6c1234f64b928987675d68d33_Out_2_Vector4, _Multiply_adc6d40dffed8b84b2439838627a0f0b_Out_2_Vector4, _Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4);
            float4 _Property_88d305cd73a646ceb547d741c837dbd5_Out_0_Vector4 = _DetailAlbedoUV2Color;
            UnityTexture2D _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap2);
            float4 _UV_35b3885c2cb245b2a63ae1909aebd057_Out_0_Vector4 = IN.uv1;
            float4 _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.tex, _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.samplerstate, _Property_761d6ba87b924cf3aeef735def0b8ffc_Out_0_Texture2D.GetTransformedUV((_UV_35b3885c2cb245b2a63ae1909aebd057_Out_0_Vector4.xy)) );
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_R_4_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.r;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_G_5_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.g;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_B_6_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.b;
            float _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_A_7_Float = _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4.a;
            float4 _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_88d305cd73a646ceb547d741c837dbd5_Out_0_Vector4, _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_RGBA_0_Vector4, _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4);
            float _Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_R_4_Float, _SampleTexture2D_5f00a41b4ad14bae86c151e135cf0212_A_7_Float, _Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float);
            float _Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fa9e306dc9174f4ba159790057dd8e51_Out_2_Float, 2, _Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float);
            float _Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float;
            Unity_Saturate_float(_Multiply_b8b4eba689e648d4a03dea9516e8f6b6_Out_2_Float, _Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float);
            float4 _Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4;
            Unity_Lerp_float4(_Multiply_3dbf944d03784584a68f4ae3bb4f0b36_Out_2_Vector4, _Multiply_aea9b997b56e4b3fae905e42a32ddea0_Out_2_Vector4, (_Saturate_106089f88ab447beb0a394d7ec526459_Out_1_Float.xxxx), _Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4);
            UnityTexture2D _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_DetailAlbedoMap3);
            float4 _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.tex, _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.samplerstate, _Property_cdfeaf867ab14fbc95f244ffa2da33a0_Out_0_Texture2D.GetTransformedUV(IN.uv2.xy) );
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_R_4_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.r;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_G_5_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.g;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_B_6_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.b;
            float _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_A_7_Float = _SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4.a;
            float4 _Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_RGBA_0_Vector4, (_SampleTexture2D_01e3ea8403894d42ad299cfbd05fc9c6_A_7_Float.xxxx), _Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4);
            float4 _UV_648b68fa22f642daa873cffc512b2b17_Out_0_Vector4 = IN.uv2;
            float _Property_4913f3e665a84dc89bc96d4bd3a51974_Out_0_Float = _DetailAlbedoUV3Gradient;
            Bindings_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989;
            float _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float;
            SG_EdgeUVMask_2efc32a882725a94b87d2bf6424fb078_float(_UV_648b68fa22f642daa873cffc512b2b17_Out_0_Vector4, _Property_4913f3e665a84dc89bc96d4bd3a51974_Out_0_Float, _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989, _EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float);
            float4 _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_3bdd9a5244e4444cacbed45f843cb925_Out_2_Vector4, (_EdgeUVMask_1dcccdd898c94f89bf6b3e44f29d7989_Mask_1_Float.xxxx), _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4);
            float _Split_8208581c2bcc443ab0479c6098551fba_R_1_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[0];
            float _Split_8208581c2bcc443ab0479c6098551fba_G_2_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[1];
            float _Split_8208581c2bcc443ab0479c6098551fba_B_3_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[2];
            float _Split_8208581c2bcc443ab0479c6098551fba_A_4_Float = _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4[3];
            float4 _Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4;
            Unity_Lerp_float4(_Lerp_c9295eb7f37347e1bc4db68c7b3dd479_Out_3_Vector4, _Multiply_f0238776b24344efb7bfb6b04a294068_Out_2_Vector4, (_Split_8208581c2bcc443ab0479c6098551fba_A_4_Float.xxxx), _Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4);
            float _Property_9133ccdf2f1c0f89913ba46e3c9a0199_Out_0_Float = _Grayscale;
            float4 _Property_6fefa341a6d19a85b76571c18da13700_Out_0_Vector4 = _EmissionColor;
            float _Property_635b04a2e39946a8b3f2bd5e93ac4ece_Out_0_Float = _LightingStrength;
            UnityTexture2D _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_LightingMap);
            float4 _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.tex, _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.samplerstate, _Property_4cb111ecb8d44fb584004218f98219b4_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_R_4_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.r;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_G_5_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.g;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_B_6_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.b;
            float _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_A_7_Float = _SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.a;
            float _Property_b59eb330b570476fabb0c7eca2048a86_Out_0_Float = _LightingHue;
            float3 _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3;
            Unity_Hue_Normalized_float((_SampleTexture2D_4987b246546245d18ace23fdec9ab9c5_RGBA_0_Vector4.xyz), _Property_b59eb330b570476fabb0c7eca2048a86_Out_0_Float, _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3);
            float3 _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Property_635b04a2e39946a8b3f2bd5e93ac4ece_Out_0_Float.xxx), _Hue_2c96827dd44a46ceb30bcf1fe9f7c20b_Out_2_Vector3, _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3);
            float3 _Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3;
            Unity_Maximum_float3((_Property_6fefa341a6d19a85b76571c18da13700_Out_0_Vector4.xyz), _Multiply_a40863a0e5f247fe933f122a368bc22d_Out_2_Vector3, _Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3);
            Bindings_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6;
            float4 _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4;
            float4 _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4;
            SG_GrayscaleSubgraph_fa34f31be22604e8ab30e78227d14d2b_float(_Lerp_6a4e14425f8d4c6dbda2632726954bb6_Out_3_Vector4, _Property_9133ccdf2f1c0f89913ba46e3c9a0199_Out_0_Float, (float4(_Maximum_69b6be2c4c534a4e83647d6c0daf0fdb_Out_2_Vector3, 1.0)), _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6, _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4, _GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutEmission_2_Vector4);
            float _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float = _Alpha;
            float _Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean = _CutoutWithAlpha;
            float _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float = _Cutoff;
            float _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float = _HeightCutoff;
            float _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float;
            Unity_Branch_float(_Property_87c056d17683398bab9ddb4b456fe9b0_Out_0_Boolean, _Property_6ee02c925509998ea9fa874b8416611e_Out_0_Float, _Property_afad77d9ccb3479ab87d0d3b8e2424ae_Out_0_Float, _Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float);
            float _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float = float(1000);
            float _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            Unity_Add_float(_Branch_a4c9f12c36af13829a5025cd14b2aac4_Out_3_Float, _Float_175d1fbd4ea843eeb800d50f731d6d29_Out_0_Float, _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float);
            surface.BaseColor = (_GrayscaleSubgraph_77db3309cfa1ff8695e3f07fa8518bb6_OutAlbedo_1_Vector4.xyz);
            surface.Alpha = _Property_930f4a0418cd44daa38730dcb9373754_Out_0_Float;
            surface.AlphaClipThreshold = _Add_2ff09b18408347faad428be6c9c18752_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphLitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    FallBack "Hidden/Shader Graph/FallbackError"
}