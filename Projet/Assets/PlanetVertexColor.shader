Shader "Custom/PlanetVertexColor" {
    Properties {
        _Glossiness ("Brillance", Range(0,1)) = 0.1
        _Metallic ("Métallique", Range(0,1)) = 0.0
    }
    SubShader {
        // Indique à Unity que ce matériau est complètement solide et opaque
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // "fullforwardshadows" assure que vos montagnes projetteront des ombres correctes
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
            // L'instruction clé : elle récupère la couleur (du Gradient) injectée dans le maillage par notre C#
            float4 color : COLOR; 
        };

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Assigne la couleur du sommet à la couleur finale du pixel (Albedo)
            o.Albedo = IN.color.rgb; 
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}