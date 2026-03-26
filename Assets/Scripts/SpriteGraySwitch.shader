Shader "Custom/SpriteGraySwitch"
{
    Properties
    {
        // 基础Sprite纹理和颜色
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
            // 灰度开关：1=开启灰度，0=关闭灰度
            [MaterialToggle] _IsGray("是否灰度", Float) = 1
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            // 2D Sprite必备设置：关闭剔除、光照、深度写入，开启透明混合
            Cull Off
            Lighting Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                sampler2D _MainTex;
                fixed4 _Color;
                float _IsGray;

                v2f vert(appdata_t IN)
                {
                    v2f OUT;
                    OUT.vertex = UnityObjectToClipPos(IN.vertex);
                    OUT.texcoord = IN.texcoord;
                    OUT.color = IN.color * _Color;
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    // 1. 采样原始Sprite纹理
                    fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                    c.rgb *= c.a;

                    // 2. 灰度计算（如果_IsGray=1，执行灰度转换）
                    if (_IsGray > 0)
                    {
                        // 经典灰度公式：人眼对绿色最敏感，权重最高
                        float gray = dot(c.rgb, float3(0.299, 0.587, 0.114));
                        c.rgb = float3(gray, gray, gray);
                    }
                    return c;
                }
                ENDCG
            }
        }
}