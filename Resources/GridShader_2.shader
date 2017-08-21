Shader "DataTreeEditor/Line" {
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 frag(v2f_img i) : Color {
                return fixed4(1, 0, 0, 1);
            }
            ENDCG
        }
    }
}