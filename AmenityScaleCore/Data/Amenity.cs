/// <summary>
/// Version         Date        Coder           Remarks
/// 0.1             2015-26-01  Greeley         Connected to Database using similar logic to Clays DataAccess Project.
/// 0.2             2026-07-02  Greeley         Added get amenties in radius SP call
/// 0.3             2026-07-03  Patrick         Added get amenties in isochrone SP call
/// 0.4             2026-12-03  Cody            Added and fixed the baseweight not being read, Added insertlocation for created point


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using AmenityScaleCore.Models.AmenitiesInRadius;
using AmenityScaleCore.Models.Amenity;
using AmenityScaleCore.Models.Category;
using AmenityScaleCore.Models.Location;
using AmenityScaleCore.Models.Subdivision;



namespace AmenityScaleCore.Data
{

    public class AmenityDataAccess
    {
        private static String GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public List<AmenityDTO> ReadAll()
        {
            var list = new List<AmenityDTO>();

            System.Data.SqlClient.SqlDataReader r =
              PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), "sp_Amenity_Read");

            while (r.Read())
            {
                list.Add(MapAmenityDTO<AmenityDTO>(r));
            }

            return list;
        }

        public void Create(AmenityDTO dto)
        {
            object scalar = PDM.Data.SqlHelper.ExecuteScalar(
              GetConnectionString(),
              "sp_Amenity_Create",
              dto.Name,
              dto.CategoryID,
              dto.Street,
              dto.City,
              dto.SubdivisionID,
              DbOrNull(dto.Latitude),
              DbOrNull(dto.Longitude),
              DbOrNull(dto.LocationWKT)
            );
        }

        public void Update(AmenityDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.AmenityID <= 0) throw new Exception("AmenityID is required for update.");
            if (dto.CategoryID <= 0) throw new Exception("Category is required.");
            if (dto.SubdivisionID <= 0) throw new Exception("Subdivision is required.");

            object latObj = dto.Latitude.HasValue ? (object)dto.Latitude.Value : DBNull.Value;
            object lngObj = dto.Longitude.HasValue ? (object)dto.Longitude.Value : DBNull.Value;
            object wktObj = string.IsNullOrWhiteSpace(dto.LocationWKT)
                ? DBNull.Value
                : (object)dto.LocationWKT;

            PDM.Data.SqlHelper.ExecuteNonQuery(
                GetConnectionString(),
                "sp_Amenity_Update",
                new System.Data.SqlClient.SqlParameter("@AmenityID", dto.AmenityID),
                new System.Data.SqlClient.SqlParameter("@Name", dto.Name),
                new System.Data.SqlClient.SqlParameter("@CategoryID", dto.CategoryID),
                new System.Data.SqlClient.SqlParameter("@Street", dto.Street ?? string.Empty),
                new System.Data.SqlClient.SqlParameter("@City", dto.City ?? string.Empty),
                new System.Data.SqlClient.SqlParameter("@SubdivisionID", dto.SubdivisionID),
                new System.Data.SqlClient.SqlParameter("@Latitude", latObj),
                new System.Data.SqlClient.SqlParameter("@Longitude", lngObj),
                new System.Data.SqlClient.SqlParameter("@LocationWKT", wktObj)
            );
        }

        public void Delete(int amenityId)
        {
            if (amenityId <= 0) throw new Exception("AmenityID is required for delete.");

            PDM.Data.SqlHelper.ExecuteNonQuery(
                GetConnectionString(),
                "sp_Amenity_Delete",
                new System.Data.SqlClient.SqlParameter("@AmenityID", amenityId)
            );
        }

        public List<CategoryDTO> ReadCategories()
        {
            var list = new List<CategoryDTO>();

            System.Data.SqlClient.SqlDataReader r =
              PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), "sp_AmenityCategory_Read");
            while (r.Read())
            {
                var dto = new CategoryDTO
                {
                    CategoryID = Convert.ToInt32(r["CategoryID"]),
                    CategoryName = r["CategoryName"].ToString(),
                    BaseWeight = Convert.ToInt32(r["BaseWeight"]),
                    IsNegative = Convert.ToBoolean(r["IsNegative"])
                };

                list.Add(dto);
            }


            return list;
        }

        public List<SubdivisionDTO> ReadSubdivisions()
        {

            var list = new List<SubdivisionDTO>();

            System.Data.SqlClient.SqlDataReader r =
              PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), "sp_Subdivision_Read");
            while (r.Read())
            {
                var dto = new SubdivisionDTO
                {
                    SubdivisionID = Convert.ToInt32(r["SubdivisionID"]),
                    CountryID = Convert.ToInt32(r["CountryID"]),
                    SubdivisionCode = Convert.ToString(r["SubdivisionCode"]),
                    SubdivisionName = Convert.ToString(r["SubdivisionName"])
                };

                list.Add(dto);
            }

            return list;
        }

        // Untested code.
        public List<AmenitiesInRadiusDTO> GetAmenitiesInRadius(userLocationRequestDTO userLocation)
        {
            var list = new List<AmenitiesInRadiusDTO>();
            System.Data.SqlClient.SqlDataReader d =
              PDM.Data.SqlHelper.ExecuteReader(
                GetConnectionString(),
                "sp_Amenity_GetInRadius",
                new System.Data.SqlClient.SqlParameter("@Latitude", userLocation.lat),
                new System.Data.SqlClient.SqlParameter("@Longitude", userLocation.lng),
                new System.Data.SqlClient.SqlParameter("@SearchRadiusMeters", userLocation.radius)
              );
            while (d.Read())
            {
                list.Add(MapAmenityDTO<AmenitiesInRadiusDTO>(d));
            }
            return list;
        }


        // Getting amenities within a isochrone
        public List<AmenitiesInRadiusDTO> GetAmenitiesInIsochrone(string wkt)
        {
            var list = new List<AmenitiesInRadiusDTO>();
            System.Data.SqlClient.SqlDataReader d =
              PDM.Data.SqlHelper.ExecuteReader(
                GetConnectionString(),
                "sp_Amenity_GetInIsochrone",
                new System.Data.SqlClient.SqlParameter("@PolygonWKT", wkt)
              );
            while (d.Read())
            {
                list.Add(MapAmenityDTO<AmenitiesInRadiusDTO>(d));
            }
            return list;
        }



        // T is either AmenityDTO or AmenitiesInRadius, both of which share the same properties. This allows us to reuse the mapping logic.
        // T = Generic Type 
        private T MapAmenityDTO<T>(System.Data.SqlClient.SqlDataReader r) where T : AmenityDTO, new()
        {
            var row = new T
            {
                AmenityID = Convert.ToInt32(r["AmenityID"]),
                Name = r["Name"]?.ToString() ?? "",
                CategoryID = Convert.ToInt32(r["CategoryID"]),
                CategoryName = r["CategoryName"]?.ToString() ?? "",
                Street = r["Street"]?.ToString() ?? "",
                City = r["City"]?.ToString() ?? "",
                SubdivisionID = Convert.ToInt32(r["SubdivisionID"]),
                GeometryType = r["GeometryType"]?.ToString() ?? "",
                LocationWKT = r["LocationWKT"]?.ToString() ?? ""
            };

            row.Latitude = r["Latitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Latitude"]);
            row.Longitude = r["Longitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Longitude"]);

            // Up till this point, the function was assuming the type was AmenityDTO. This is where the type might be different and allows us to 
            // use a different type for our returned amenities.
            //if (row is AmenitiesInRadiusDTO searchResult && HasColumn(r, "DistanceInMeters"))
            //{
            //    searchResult.DistanceInMeters = Convert.ToDouble(r["DistanceInMeters"]);
            //}
            if (row is AmenitiesInRadiusDTO searchResult)
            {
                if (HasColumn(r, "DistanceInMeters"))
                    searchResult.DistanceInMeters = Convert.ToDouble(r["DistanceInMeters"]);

                if (HasColumn(r, "BaseWeight"))
                    searchResult.BaseWeight = Convert.ToInt32(r["BaseWeight"]);
            }

            return row;
        }

        private bool HasColumn(System.Data.SqlClient.SqlDataReader r, string columnName)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (r.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static object DbOrNull(object v) => v ?? DBNull.Value;
        public void InsertLocation(LocationWithScoreDTO loc)
        {
            var parameters = new[]
            {
                new System.Data.SqlClient.SqlParameter("@LocationName", loc.LocationName ?? string.Empty),
                new System.Data.SqlClient.SqlParameter("@StreetNumber", loc.StreetNumber ?? string.Empty),
                new System.Data.SqlClient.SqlParameter("@Street", loc.Street ?? string.Empty),
                new System.Data.SqlClient.SqlParameter("@City", loc.City ?? string.Empty),
                new System.Data.SqlClient.SqlParameter("@SubdivisionID", loc.SubdivisionID),
                new System.Data.SqlClient.SqlParameter("@Latitude", loc.Latitude ?? 0),
                new System.Data.SqlClient.SqlParameter("@Longitude", loc.Longitude ?? 0),
                new System.Data.SqlClient.SqlParameter("@LocationWKT", loc.LocationWKT ?? string.Empty),
                new System.Data.SqlClient.SqlParameter("@GeometryType", loc.GeometryType ?? "POINT"),
                new System.Data.SqlClient.SqlParameter("@CalculatedScore", loc.CalculatedScore),
                new System.Data.SqlClient.SqlParameter("@CreatedDate", loc.CreatedDate),
            };

            // Call sp_Location_Create
            PDM.Data.SqlHelper.ExecuteNonQuery(GetConnectionString(), "sp_Location_Create", parameters);
        }
    }


}