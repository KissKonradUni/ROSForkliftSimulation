using System;

// ReSharper disable once CheckNamespace
namespace Boxes
{
    public static class Consts
    {
        public const int TypeCount = 4;
    }

    public enum Types
    {
        Blue = 0,
        Green = 1,
        Yellow = 2,
        Red = 3
    }

    public enum Zones
    {
        Mixed = 0,
        Blue = 1,
        Green = 2,
        Yellow = 3,
        Red = 4
    }
    
    public static class Helpers
    {
        public static string TypeName(Types type)
        {
            return type switch
            {
                Types.Blue => "Blue",
                Types.Green => "Green",
                Types.Yellow => "Yellow",
                Types.Red => "Red",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}