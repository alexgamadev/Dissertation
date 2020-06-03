Shader "Custom/Terrain"
{
    Properties
    {
        _GrassColor ("Grass Color", Color) = (0,1,0,1)
        _RockColor ("Rock Color", Color) = (1,1,1,1)
        _GrassSlopeThreshold ("Grass Slope Threshold", Range(0, 1)) = .5
        _GrassBlendAmount ("Grass Blend Amount", Range(0, 1)) = .5
        [Toggle(SHOW_MAP_DATA)] _ShowMapData ("Show Map Data", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma shader_feature SHOW_MAP_DATA

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            #if defined(SHOW_MAP_DATA)
				float mapData;
			#endif
            
        };

        half _MaxHeight;
        half _GrassSlopeThreshold;
        half _GrassBlendAmount;
        fixed4 _GrassColor;
        fixed4 _RockColor;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float slope = 1 - IN.worldNormal.y; //Slope = 0 when terrain completely flat;
            float grassBlendHeight = _GrassSlopeThreshold * (1 - _GrassBlendAmount);
            float grassWeight = 1 - saturate((slope-grassBlendHeight)/(_GrassSlopeThreshold-grassBlendHeight));
            o.Albedo = _GrassColor * grassWeight + _RockColor * (1 - grassWeight);
            #if defined(SHOW_MAP_DATA)
				o.Albedo = float3(0, 1, 0);
			#endif
        }
        ENDCG
    }
}
