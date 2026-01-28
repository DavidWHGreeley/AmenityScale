using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmenityScale.Models.Category
{
    public class CategoryDTO
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int BaseWeight { get; set; }
        public bool IsNegative { get; set; }
    }
}
