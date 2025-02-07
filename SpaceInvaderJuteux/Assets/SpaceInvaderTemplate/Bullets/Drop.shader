Shader"IIM/Drop"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DistortionTex ("Distortion", 2D) = "white" {}
        _Power("Power", Range(0,1)) = 0.5
        _Bias("Bias", Range(-1,1)) = 0
        _Clamp("Clamp", Range(0,0.5)) = 0.3
        _Seed("Seed", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 c : COLOR0;
            };

            sampler2D _MainTex;
            sampler2D _DistortionTex;
            float _Power;
            float _Clamp;
            float _Bias;
            float _Seed;
            float4 _MainTex_ST;
            float4 _DistortionTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
    
                float4 screenPosition = ComputeScreenPos(o.vertex); // Calcule la Screen position pour chaque vertex                
                screenPosition.xy = frac(screenPosition.xy + _Seed * 1234.56); // Décale la position par une valeur très grande. frac pour revenir entre 0 et 1
                screenPosition /= screenPosition.w; // Mmmm...
                float2 screenUV = TRANSFORM_TEX(screenPosition.xy, _DistortionTex); // Applique le tilling et l'offset
                float distortion = tex2Dlod(_DistortionTex, float4(screenUV, 0, 0)).r; // Lit la texture, MipMap de LOD 0
                distortion += _Bias; // Ajoute un biais si le bruit n'est pas bien centré sur 0.5, auquel cas le décalage se ferait essetiellement d'un côté
                float clampedDistortion = clamp(distortion.r + _Bias, _Clamp, 1 - _Clamp); // Rend rectiligne la déformation quand la goute s'écarte trop du centre. 
    
                float offset = (clampedDistortion * 2 - 1) * _Power; // Intensifie l'effet avec le power.
                o.vertex.x += offset; // Applique l'offset à la position du vertex.
    
                // Shader par défaut
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
        
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Shader par défaut                
                fixed4 col = tex2D(_MainTex, i.uv);                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
