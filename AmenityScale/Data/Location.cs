using AmenityScale.Models;
using AmenityScale.Models.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AmenityScale.Data
{
    public class LocationDataAccess
    {
        public List<LocationDTO> ReadAll()
        {

            return new List<LocationDTO>();
        }

        public void Create(LocationDTO dto)
        {
            // TODO: sp_Location_Create
        }

        public void Update(LocationDTO dto)
        {
            // TODO: sp_Location_Update
        }

        public void Delete(int locationId)
        {
            // TODO: sp_Location_Delete
        }

        public List<SubdivisionDTO> ReadSubdivisions()
        {
            return new List<SubdivisionDTO>();
        }
    }
}

