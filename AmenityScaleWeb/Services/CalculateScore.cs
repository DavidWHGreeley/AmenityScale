using AmenityScaleCore.Models.AmenitiesInRadius;
using AmenityScaleWeb.MathHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             26-02-16    Cody                    calculate Score.
/// 0.2             26-03-07    Patrick                 updated for isochrones

namespace AmenityScaleWeb.Services
{
    public class CalculateScore
    {
        // Dictionary to store 
        private static readonly Dictionary<int, double> decayMultipliers = new Dictionary<int, double>
        {
            { 1, 1.0 },
            { 2, 0.8 },
            { 3, 0.6 },
            { 4, 0.4 }
        };

        public static double CalculateTotalScore(Dictionary<int, List<AmenitiesInRadiusDTO>> rings)
        {
            double finalScore = 0.0;
            // Use a hash set instead of a list to prevent counting the same amenity twice
            HashSet<int> processedIds = new HashSet<int>();

            // Sort the keys in rings before converting them to a list just to double check
            var sortedMinutes = rings.Keys.OrderBy(m => m).ToList();

            foreach (var isochroneNumber in sortedMinutes)
            {
                // Set multiplier to 0 if the ring number is not in the decayMultipliers dictionary 
                double decay = decayMultipliers.ContainsKey(isochroneNumber) ? decayMultipliers[isochroneNumber] : 0;

                foreach (var amenity in rings[isochroneNumber])
                {
                    // Skip amenities that have already been counted
                    if (processedIds.Contains(amenity.AmenityID)) continue;

                    double contribution = (double)amenity.BaseWeight * decay;

                    // For good vs bad amenities
                    if (amenity.IsNegative == 0)
                        finalScore += contribution;
                    else
                        finalScore -= contribution;

                    processedIds.Add(amenity.AmenityID);
                }
            }

            return finalScore;
        }
        
    }
}