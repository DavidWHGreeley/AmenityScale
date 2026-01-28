/// <summary>
/// Version         Date        Coder           Remarks
/// 0.1             2015-26-01  Greeley         Connected to Database using similar logic to Clays DataAccess Project.


using AmenityScale.Models;
using AmenityScale.Models.Amenity;
using AmenityScale.Models.Category;
using AmenityScale.Models.Subdivision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;



namespace AmenityScale.Data
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
                list.Add(MapAmenityDTO(r));
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

        private AmenityDTO MapAmenityDTO(System.Data.SqlClient.SqlDataReader r)
        {

            var row = new AmenityDTO
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

            return row;
        }
        private static object DbOrNull(object v) => v ?? DBNull.Value;

    }
}