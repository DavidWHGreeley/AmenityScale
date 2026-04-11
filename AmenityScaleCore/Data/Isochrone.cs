/// <summary>
/// Version         Date        Coder           Remarks
/// 0.1             2026-01-04  Patrick         Create Read Isochrones

using AmenityScaleCore.Models.Isochrone;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace AmenityScaleCore.Data
{
    public class IsochroneDataAccess
    {
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public void Create(int locationID, int travelTime, string wkt)
        {
            PDM.Data.SqlHelper.ExecuteNonQuery(
                GetConnectionString(),
                "sp_InsertIsochrone",
                new SqlParameter("@LocationID", locationID),
                new SqlParameter("@TravelTime", travelTime),
                new SqlParameter("@WKT", wkt)
            );
        }

        public List<IsochroneDTO> Read(int locationID)
        {
            var list = new List<IsochroneDTO>();

            SqlDataReader r = PDM.Data.SqlHelper.ExecuteReader(
                GetConnectionString(),
                "sp_GetIsochrones",
                new SqlParameter("@LocationID", locationID)
            );

            while (r.Read())
            {
                list.Add(new IsochroneDTO
                {
                    IsochroneID = Convert.ToInt32(r["IsochroneID"]),
                    LocationID = Convert.ToInt32(r["LocationID"]),
                    TravelTime = Convert.ToInt32(r["TravelTime"]),
                    PolygonWKT = r["PolygonWKT"]?.ToString() ?? ""
                });
            }

            return list;
        }
    }
}