Shader "AlphaBypass/Blitter"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        ZClip Off
        ZTest Off 
        ZWrite Off Cull Off 
        Pass
        {
            Name "AlphaCopy"
        
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag(Varyings input) : SV_Target0
            {        
                return LOAD_TEXTURE2D(_BlitTexture, input.positionCS.xy).a;
            }
            ENDHLSL
        }
        Pass
        {
            Name "AlphaCopy"
            ColorMask A
        
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag(Varyings input) : SV_Target0
            {        
                return LOAD_TEXTURE2D(_BlitTexture, input.positionCS.xy).r;
            }
            ENDHLSL
        }
    }
}
