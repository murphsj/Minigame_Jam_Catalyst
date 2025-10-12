using UnityEngine;

public class Util_CATACLYST
{
    public static byte floatToColorByte(float num)
    {
        return (byte)(num * 255);
    }

    public static Color32 color32FromFloat4(Vector4 float4)
    {
        return new Color32(
            floatToColorByte(float4.x),
            floatToColorByte(float4.y),
            floatToColorByte(float4.z),
            floatToColorByte(float4.w)
        );
    }
}