Shader "Custom/Minimap2Layer"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Lighting Off
        ZWrite On
        ZTest LEqual
        Cull Off

        Pass
        {
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture;
            float4 _PlayerRoom; // x,y = room | z = radius | w = roomSize

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ───── Fog of war ─────
                float2 room = floor(i.world.xz / _PlayerRoom.w);
                float dist = max(
                    abs(room.x - _PlayerRoom.x),
                    abs(room.y - _PlayerRoom.y)
                );

                if (dist > _PlayerRoom.z)
                    return fixed4(0,0,0,1);

                // ───── Depth outline ─────
                float2 uv = i.screenPos.xy / i.screenPos.w;
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);

                float2 px = float2(1.0 / 512.0, 1.0 / 512.0);
                float depthR = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv + float2(px.x, 0));
                float depthU = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv + float2(0, px.y));

                float edge = abs(depth - depthR) + abs(depth - depthU);

                if (edge > 0.002)
                    return fixed4(1,1,1,1); // WHITE OUTLINE

                // ───── PLAYER VS ROOMS ─────
                // player is higher Y usually → brighter
                if (i.world.y > 1.5)
                    return fixed4(0,1,0,1); // PLAYER (green)

                // room fill
                return fixed4(0.08,0.08,0.08,1); // DARK ROOM
            }
            ENDCG
        }
    }
}
