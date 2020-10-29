Shader "Unlit/CelShader"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Main Tex Color", Color) = (1,1,1,1)
        _BumpMap("NormalMap", 2D) = "bump" {}

        _Outline_Bold("Outline Bold", Range(0, 1)) = 0.1

        _Band_Tex("Band LUT", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }

        cull front  
        Pass
        {
            CGPROGRAM
            #pragma vertex _VertexFuc
            #pragma fragment _FragmentFuc
            #include "UnityCG.cginc"

                struct ST_VertexInput  
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct ST_VertexOutput   
                {
                    float4 vertex : SV_POSITION;
                };

                float _Outline_Bold;

                ST_VertexOutput _VertexFuc(ST_VertexInput stInput)
                {
                    ST_VertexOutput stOutput;

                    float3 fNormalized_Normal = normalize(stInput.normal);  
                    float3 fOutline_Position = stInput.vertex + fNormalized_Normal * (_Outline_Bold * 0.1f); 

                    stOutput.vertex = UnityObjectToClipPos(fOutline_Position);    
                    return stOutput;
                }


                float4 _FragmentFuc(ST_VertexOutput i) : SV_Target
                {
                    return 0.0f;
                }

            ENDCG
        }

        cull back    
        CGPROGRAM

        #pragma surface surf _BandedLighting   

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Band_Tex;
            float2 uv_BumpMap;
        };

        struct SurfaceOutputCustom        
        {
            fixed3 Albedo;
            fixed3 Normal;
            fixed3 Emission;
            half Specular;
            fixed Gloss;
            fixed Alpha;

            float3 BandLUT;
        };

        sampler2D _MainTex;
        sampler2D _Band_Tex;
        sampler2D _BumpMap;

        float4 _Color;

        void surf(Input IN, inout SurfaceOutputCustom o)
        {
            float4 fMainTex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = fMainTex.rgb;
            o.Alpha = 1.0f;

            float4 fBandLUT = tex2D(_Band_Tex, IN.uv_Band_Tex);
            o.BandLUT = fBandLUT.rgb;

            float3 fNormalTex = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Normal = fNormalTex;
        }

        float4 Lighting_BandedLighting(SurfaceOutputCustom s, float3 lightDir, float3 viewDir, float atten)
        {
            float3 fBandedDiffuse;
            float fNDotL = dot(s.Normal, lightDir) * 0.5f + 0.5f; 
            fBandedDiffuse = tex2D(_Band_Tex, float2(fNDotL, 0.5f)).rgb;



            float3 fSpecularColor;
            float3 fHalfVector = normalize(lightDir + viewDir);
            float fHDotN = saturate(dot(fHalfVector, s.Normal));
            float fPowedHDotN = pow(fHDotN, 500.0f);

            float fSpecularSmooth = smoothstep(0.005, 0.01f, fPowedHDotN);
            fSpecularColor = fSpecularSmooth * 1.0f;



            float4 fFinalColor;
            fFinalColor.rgb = ((s.Albedo * _Color) + fSpecularColor) *
                                 fBandedDiffuse * _LightColor0.rgb * atten;
            fFinalColor.a = s.Alpha;

            return fFinalColor;
        }

        ENDCG
    }
}