using System;



namespace AmenityScaleWeb.MathHelpers
{
    public class Mathhelper
    {
        public static double Clamp(double value, double min, double max)
        {
            return Math.Min(max, Math.Max(value, min));
        }
    }
}