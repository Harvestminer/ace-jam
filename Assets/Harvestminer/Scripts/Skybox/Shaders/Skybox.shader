Shader "Harvestminer/Skybox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SunSize ("Sun Size", Range(0,1)) = 0.04
        _SunSizeConvergence("Sun Size Convergence", Range(1,10)) = 5
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #define MIE_G (-0.990)
            #define MIE_G2 0.9801

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            uniform half _SunSize;
            uniform half _SunSizeConvergence;
            
            // Calculates the Mie phase function
            half getMiePhase(half eyeCos, half eyeCos2)
            {
                half temp = 1.0 + MIE_G2 - 2.0 * MIE_G * eyeCos;
                temp = pow(temp, pow(_SunSize,0.65) * 10);
                temp = max(temp,1.0e-4); // prevent division by zero, esp. in half precision
                temp = 1.5 * ((1.0 - MIE_G2) / (2.0 + MIE_G2)) * (1.0 + eyeCos2) / temp;
                #if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
                    temp = pow(temp, .454545);
                #endif
                return temp;
            }

            half calcSunAttenuation(half3 lightPos, half3 ray)
            {
                half focusedEyeCos = pow(saturate(dot(lightPos, ray)), _SunSizeConvergence);
                return getMiePhase(-focusedEyeCos, focusedEyeCos * focusedEyeCos);
            }

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 vertex : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.pos = UnityObjectToClipPos(v.vertex);

                float3 eyeRay = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
                o.vertex = -eyeRay;

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half3 col = half3(0.0, 0.5, 1.0);

                half3 ray = normalize(i.vertex.xyz);

                // sample the texture
                col += float3(1, 1, 0) * calcSunAttenuation(_WorldSpaceLightPos0.xyz, -ray);
                return half4(col,1.0);
            }
            ENDCG
        }
    }
}
