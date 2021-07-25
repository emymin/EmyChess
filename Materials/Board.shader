Shader "EmyChess/Board"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _Color2("Color 2",Color)=(1,1,1,1)
        _Indicator1("Legal Moves Indicator Color",Color)=(1,0.5,0,1)
        _Indicator2("Check Indicator Color",Color)=(1,0,0,1)
        _Indicator3("Debug Indicator Color",Color)=(0,1,0,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _TextureOpacity("Texture Mix",Range(0,1))=0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows 

        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color1;
        fixed4 _Color2;
        fixed4 _Indicator1;
        fixed4 _Indicator2;
        fixed4 _Indicator3;
        float _Indicators[64];
        float _TextureOpacity;


        UNITY_INSTANCING_BUFFER_START(Props)

        UNITY_INSTANCING_BUFFER_END(Props)

        float2 tri(float2 x) // triangular wave (used as integral of the square wave for the checkerboard pattern)
        {
            float2 h = frac(x * .5) - .5;
            return (1 - 2 * abs(h));
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = 1;
            float scale = 8;
            float2 uv = IN.uv_MainTex * scale;
            uv = clamp(uv, 0, scale);
            float2 id = floor(uv);

            float2 width = float2(max(abs(ddx(uv)), abs(ddy(uv))));
            float2 integral = float2((tri( clamp(uv + .5 * width,0,scale) ) - tri( clamp(uv - .5 * width,0,scale) )) / width);
            float square = saturate(.5 - .5 * integral.x * integral.y); //antialiased checkerboard

            
            
            
            c.rgb = lerp( _Color1.rgb, _Color2.rgb, square);
            c = lerp(c, tex2D(_MainTex, IN.uv_MainTex), _TextureOpacity);
            float indicator = _Indicators[id.x * 8 + id.y];
            float3 indicatorCol = c;
            if (indicator == 1) indicatorCol = _Indicator1;
            if (indicator == 2) indicatorCol = _Indicator2;
            if (indicator == 3) indicatorCol = _Indicator3;
            c.rgb = lerp(c.rgb, indicatorCol, 0.6);
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
