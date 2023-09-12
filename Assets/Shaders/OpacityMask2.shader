Shader "Custom/OpacityMask2"
{
    Properties
    {
        _Transparency("Transparency", Range(0,0.5)) = 0.25
    }
    SubShader
    {
        Tags { "Queue" = "Transparent+1"}

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float _Transparency;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 colWithTransparency = fixed4(1.0, 1.0, 1.0, _Transparency);
                return colWithTransparency;
            }
            ENDCG
        }
    }
}
