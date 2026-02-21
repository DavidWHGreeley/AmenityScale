/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2015-26-01  Greeley                 Init DTO
/// 

namespace AmenityScaleCore.Models.Category
{
    public class CategoryDTO
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int BaseWeight { get; set; }
        public bool IsNegative { get; set; }
    }
}
