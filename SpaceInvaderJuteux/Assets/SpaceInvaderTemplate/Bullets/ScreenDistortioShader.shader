Shader "Custom/WavyTrailShaderAdvancedColor"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {} // Texture principale
        _NoiseTex ("Noise Texture", 2D) = "black" {} // Texture de bruit
        _DistortionStrength ("Distortion Strength", Range(0, 0.2)) = 0.05 // Intensité de la distorsion
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 2.0 // Vitesse de l'animation
        _WaveFrequency ("Wave Frequency", Range(0, 20)) = 10.0 // Fréquence des vagues
        _RandomFactor ("Randomness", Range(0, 1)) = 0.5 // Facteur aléatoire
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.2)) = 0.1 // Amplitude des ondes
        _TrailColor ("Trail Color", Color) = (1,1,1,1) // Couleur de la traînée
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Transparence
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _TrailColor;
            float _DistortionStrength;
            float _WaveSpeed;
            float _WaveFrequency;
            float _RandomFactor;
            float _WaveAmplitude;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                float4 ScreenPos = ComputeScreenPos(o.vertex);

                ScreenPos /= ScreenPos.w;
                o.uv2 = float2(ScreenPos.x, ScreenPos.y);

                
                float2 noiseValue = tex2Dlod(_NoiseTex, o.uv2).rg * 2.0 - 1.0;

                float2 distortedUV = i.uv + noiseValue * _DistortionStrength;
                o.vertex.x += distortedUV.x;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //float time = _Time.y * _WaveSpeed;

                //// Génère un mouvement ondulant
                //float waveX = sin(i.uv.y * _WaveFrequency + time) * _WaveAmplitude;
                //float waveY = cos(i.uv.x * _WaveFrequency + time) * _WaveAmplitude;

                //// Ajoute un effet de bruit
                //float2 noiseUV = i.uv + float2(waveX, waveY) * _RandomFactor;
                //float2 noiseValue = tex2D(_NoiseTex, noiseUV).rg * 2.0 - 1.0;

                //// Applique la distorsion finale
                //float2 distortedUV = i.uv + noiseValue * _DistortionStrength + float2(waveX, waveY);
                
                //// Récupère la couleur de la texture et applique la couleur du trail
                //fixed4 col = tex2D(_MainTex, distortedUV) * _TrailColor;

                //return col;


                                float2 noiseValue = tex2D(_NoiseTex, i.uv2).rg * 2.0 - 1.0;

                                float2 distortedUV = i.uv + noiseValue * _DistortionStrength;
                                fixed4 col = tex2D(_MainTex, distortedUV) * _TrailColor;


                return col;
            }
            ENDCG
        }
    }
}
