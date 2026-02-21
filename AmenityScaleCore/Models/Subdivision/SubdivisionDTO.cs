/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2015-26-01  Greeley                 Init DTO
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmenityScaleCore.Models.Subdivision
{
    public class SubdivisionDTO
    {
        public int SubdivisionID { get; set; }
        public int CountryID { get; set; }
        public string SubdivisionCode { get; set; }
        public string SubdivisionName { get; set; }
    }
}
