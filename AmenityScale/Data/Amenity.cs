using AmenityScale.Models;
using AmenityScale.Models.Amenity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AmenityScale.Data
{

    public class AmenityDataAccess
    {
        public List<AmenityDTO> ReadAll()
        {
            return new List<AmenityDTO>();
        }

        public void Create(AmenityDTO dto)
        {
            // TODO: sp_Amenity_Create
        }

        public void Update(AmenityDTO dto)
        {
            // TODO: sp_Amenity_Update
        }

        public void Delete(int amenityId)
        {
            // TODO: sp_Amenity_Delete
        }

        public List<CategoryDTO> ReadCategories()
        {
            return new List<CategoryDTO>();
        }

        public List<SubdivisionDTO> ReadSubdivisions()
        {
            return new List<SubdivisionDTO>();
        }
    }
}