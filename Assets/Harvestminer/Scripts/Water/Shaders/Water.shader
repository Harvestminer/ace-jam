Shader "Harvestminer/Water" {
    Properties {
        _Color ("Color", Color) = ( 0,0,0,0 )

        _WaveSpeed ("Wave Speed", Range (0, 10)) = 1
        _WaveHeight ("Wave Height", Range (0, 5)) = 1
        _WaveFrequency ("Wave Frequency", Range (0.5, 5)) = 1
    }

    SubShader {
        Tags { "Queue" = "Transparent"}
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : POSITION;
                float4 color : COLOR;
            };

            float4 _Color;
            uniform float4 _LightColor0;

            float _WaveSpeed;
            float _WaveHeight;
            float _WaveFrequency;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 vertex = mul(unity_ObjectToWorld, v.vertex).xyz;

                o.pos.y += sin(_Time.y * _WaveSpeed + (vertex.z + vertex.x) * _WaveFrequency) * _WaveHeight;

                // Calculate normal using central difference method
                float dx = _WaveFrequency * _WaveHeight * cos(_Time.y * _WaveSpeed + (vertex.z + (vertex.x + 0.01)) * _WaveFrequency);
                float dz = _WaveFrequency * _WaveHeight * cos(_Time.y * _WaveSpeed + ((vertex.z + 0.01) + vertex.x) * _WaveFrequency);

                float3 normal = normalize(float3(-dx, 1.0, -dz));
                float3 normalDirection = normalize(mul(float4(normal, 0.0),unity_WorldToObject).xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float atten = 1.0;

				float3 diffuseReflection = atten *_LightColor0.xyz*max(0.0,dot(normalDirection, lightDirection));
				float3 lightFinal = diffuseReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;

				o.color = float4(lightFinal*_Color.rgb, 1.0);


                return o;
            }

            float4 frag(v2f o) : SV_TARGET 
            {
                return o.color;
            }
            ENDCG
        }
    }
}