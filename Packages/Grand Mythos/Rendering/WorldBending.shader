Shader "WorldBending/Base"
{
    Properties
    {
        [Toggle] _Keyword ("Keyword", Float) = 0.0
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow fullforwardshadows vertex:vert
        #pragma multi_compile _ WorldBendingOn
        #pragma shader_feature _ _KEYWORD_ON

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        float3 WorldBendingSource;
        half WorldBendingCurveStrength, WorldBendingCurve;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v)
        {
            #if WorldBendingOn
            float3 wPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));
            float2 delta = (wPos - WorldBendingSource).xz;
            delta = pow(abs(delta), WorldBendingCurve) * -WorldBendingCurveStrength;
            wPos += float3(0, delta.x+delta.y, 0);
            v.vertex.xyz = mul(unity_WorldToObject, float4(wPos, 1));
            #endif
            //v.vertex.xyz += v.normal * 10;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            #if _KEYWORD_ON
            clip(o.Alpha-0.5);
            #endif
        }
        ENDCG
    }
    FallBack "Diffuse"
}
