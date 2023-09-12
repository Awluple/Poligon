Shader "Custom/WindowMask"
{
    SubShader{
        Tags { "Queue" = "Transparent+1"}
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
        }
    }
}
