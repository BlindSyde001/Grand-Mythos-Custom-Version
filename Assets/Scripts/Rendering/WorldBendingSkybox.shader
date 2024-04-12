Shader "WorldBending/Skybox"
{
    Properties{}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            ZWrite Off
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile _ WorldBendingOn

            #include "UnityCG.cginc"
            float3 WorldBendingSource;
            half WorldBendingCurveStrength, WorldBendingCurve;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            v2f vert (appdata v)
            {
                v2f o;

                #if WorldBendingOn
                float3 wPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));
                float2 delta = (wPos - WorldBendingSource).xz;
                delta = pow(abs(delta), WorldBendingCurve) * -WorldBendingCurveStrength;
                wPos += float3(0, delta.x+delta.y, 0);
                v.vertex.xyz = mul(unity_WorldToObject, float4(wPos, 1));
                #endif

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = mul(unity_ObjectToWorld, v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, i.normal, 0.0);
            }
            ENDCG
        }
    }
}
