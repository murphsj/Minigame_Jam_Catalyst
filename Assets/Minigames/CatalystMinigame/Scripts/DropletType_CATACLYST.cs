using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public enum DropletType
{
    None = 0,
    Blue = 1,
    Green = 2,
    Red = 3,
    Cyan = 4
}

public static class Extensions
{
    public static string getStateName(this DropletType dropletType)
    {
        return "droplet_" + (((int)dropletType)-1);
    }

    public static Vector4 getColor(this DropletType dropletType)
    {
        switch (dropletType)
        {
            case DropletType.None:
                return new Vector4(0, 0, 0, 0);
            case DropletType.Blue:
                return new Vector4(0.3f, 0.6f, 0.8f, 1);
            case DropletType.Green:
                return new Vector4(0.3f, 0.9f, 0.3f, 1);
            case DropletType.Red:
                return new Vector4(0.9f, 0.1f, 0.1f, 1);
            case DropletType.Cyan:
                return new Vector4(0.153f, 0.92f, 0.67f, 1); //six seven
            default:
                return new Vector4(0, 0, 0, 0);
        }
    }
}