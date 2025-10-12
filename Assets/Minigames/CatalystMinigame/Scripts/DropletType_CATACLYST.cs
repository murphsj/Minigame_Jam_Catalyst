using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public enum DropletType_CATALYST
{
    None = 0,
    Blue = 1,
    Green = 2,
    Red = 3,
    Cyan = 4
}

public static class Extensions
{
    public static string getStateName(this DropletType_CATALYST dropletType)
    {
        return "droplet_" + (((int)dropletType)-1);
    }

    public static Vector4 getColor(this DropletType_CATALYST dropletType)
    {
        switch (dropletType)
        {
            case DropletType_CATALYST.None:
                return new Vector4(0, 0, 0, 0);
            case DropletType_CATALYST.Blue:
                return new Vector4(0.3f, 0.6f, 0.8f, 1);
            case DropletType_CATALYST.Green:
                return new Vector4(0.3f, 0.9f, 0.3f, 1);
            case DropletType_CATALYST.Red:
                return new Vector4(0.9f, 0.1f, 0.1f, 1);
            case DropletType_CATALYST.Cyan:
                return new Vector4(0.153f, 0.92f, 0.67f, 1); //six seven
            default:
                return new Vector4(0, 0, 0, 0);
        }
    }
}