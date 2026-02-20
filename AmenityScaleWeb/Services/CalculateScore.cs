using AmenityScaleCore.Models.AmenitiesInRadius;
using AmenityScaleWeb.MathHelpers;
using System.Collections.Generic;

/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             26-02-16    Cody                    calculate Score.
/// 
namespace AmenityScaleWeb.Services
{
    public class CalculateScore
    {
        public static double? CalcuateAmenityScore(List<AmenitiesInRadiusDTO> amenities, double radiusInMeters)
        {
            double positiveBaseValue = 10;
            double positiveScore = 0;
            double negativeScore = 0;

            if (amenities == null || amenities.Count == 0) return null;

            foreach (var amenity in amenities)
            {
                double distance = (double)amenity.DistanceInMeters;
                double decay = Mathhelper.Clamp(1 - (distance / radiusInMeters), 0, 1);
                if (amenity.IsNegative == 0)
                {
                    positiveScore += positiveBaseValue * decay;
                }
                else
                {
                    // Calcuate your Negative Amenties here
                }
            }
            return positiveScore;
        }
    }
}