#ifndef GETLAYERCOLOR_INCLUDE
#define GETLAYERCOLOR_INCLUDE

// how many elements in LayerColors
static const int LAYER_COUNT = 10;
// how big the 'dip' in each layer is
static const float MENISCUS_MULT = 0.3;

uniform float4 _LayerColors[LAYER_COUNT];

void GetLayerColor_float(float2 UV, out float4 LayerColor)
{
    int index = (int)((UV.y * 10) + sin(UV.x * PI)*MENISCUS_MULT);
    LayerColor = _LayerColors[index];
}

#endif

