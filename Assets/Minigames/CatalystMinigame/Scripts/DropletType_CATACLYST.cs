using System;
using Unity.VisualScripting;

public enum DropletType
{
    Blue = 0,
    Green = 1,
    Red = 2,
    Cyan = 3
}

public static class Extensions
{
    public static string getSpriteName(this DropletType dropletType)
    {
        return "droplet_" + (int)dropletType;
    }
}