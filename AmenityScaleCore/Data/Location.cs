/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2015-26-01  Greeley & Patrick       Did a session where we brought a clone of Amenity Into Locaiton.
/// 0.2             2015-26-01  Greeley                 Forgot to do a map check for Geo type and WKT
/// 0.3             2015-31-01  Greeley                 Removed the OUT param from the SP. Removed the OUT from Create method
/// 


using AmenityScaleCore.Models;
using AmenityScaleCore.Models.Location;
using AmenityScaleCore.Models.Subdivision;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AmenityScaleCore.Data
{
    public class LocationDataAccess
    {
        private static String GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public List<LocationDTO> ReadAll()
        {
            var list = new List<LocationDTO>();

            SqlDataReader r =
                PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), "sp_Location_Read");

            while (r.Read())
            {
                list.Add(MapLocationDTO(r));
            }

            return list;
        }

        public void Create(LocationDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            PDM.Data.SqlHelper.ExecuteNonQuery(
                GetConnectionString(),
                "sp_Location_Create",
                new SqlParameter("@LocationName", dto.LocationName ?? string.Empty),
                new SqlParameter("@StreetNumber", dto.StreetNumber ?? string.Empty),
                new SqlParameter("@Street", dto.Street ?? string.Empty),
                new SqlParameter("@City", dto.City ?? string.Empty),
                new SqlParameter("@Latitude", dto.Latitude),
                new SqlParameter("@Longitude", dto.Longitude),
                new SqlParameter("@CalculatedScore", dto.CalculatedScore),
                new SqlParameter("@SubdivisionID", dto.SubdivisionID)
            );
        }

        public void Update(LocationDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.LocationID <= 0) throw new Exception("LocationID is required for update.");
            if (dto.SubdivisionID <= 0) throw new Exception("Subdivision is required.");

            PDM.Data.SqlHelper.ExecuteNonQuery(
                GetConnectionString(),
                "sp_Location_Update",
                new SqlParameter("@LocationID", dto.LocationID),
                new SqlParameter("@LocationName", dto.LocationName ?? string.Empty),
                new SqlParameter("@StreetNumber", dto.StreetNumber ?? string.Empty),
                new SqlParameter("@Street", dto.Street ?? string.Empty),
                new SqlParameter("@City", dto.City ?? string.Empty),
                new SqlParameter("@SubdivisionID", dto.SubdivisionID),
                new SqlParameter("@Latitude", dto.Latitude),
                new SqlParameter("@Longitude", dto.Longitude)
            );
        }

        public void Delete(int locationId)
        {
            if (locationId <= 0) throw new Exception("LocationID is required for delete.");

            PDM.Data.SqlHelper.ExecuteNonQuery(
                GetConnectionString(),
                "sp_Location_Delete",
                new SqlParameter("@LocationID", locationId)
            );
        }

        public List<SubdivisionDTO> ReadSubdivisions()
        {
            var list = new List<SubdivisionDTO>();

            SqlDataReader r =
                PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), "sp_Subdivision_Read");

            while (r.Read())
            {
                list.Add(new SubdivisionDTO
                {
                    SubdivisionID = Convert.ToInt32(r["SubdivisionID"]),
                    CountryID = Convert.ToInt32(r["CountryID"]),
                    SubdivisionCode = Convert.ToString(r["SubdivisionCode"]),
                    SubdivisionName = Convert.ToString(r["SubdivisionName"])
                });
            }

            return list;
        }

        private static LocationDTO MapLocationDTO(SqlDataReader r)
        {
            var row = new LocationDTO
            {
                LocationID = Convert.ToInt32(r["LocationID"]),
                LocationName = r["LocationName"]?.ToString() ?? "",
                StreetNumber = r["StreetNumber"]?.ToString() ?? "",
                Street = r["Street"]?.ToString() ?? "",
                City = r["City"]?.ToString() ?? "",
                SubdivisionID = Convert.ToInt32(r["SubdivisionID"]),
                GeometryType = r["GeometryType"]?.ToString() ?? "",
                LocationWKT = r["LocationWKT"]?.ToString() ?? ""
            };

            row.Latitude = r["Latitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Latitude"]);
            row.Longitude = r["Longitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Longitude"]);

            return row;
        }

        private static object DbOrNull(object v) => v ?? DBNull.Value;
    }
}