using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;


namespace AmenityScale.Data
{
    public static class Database
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private static string GetConnectionString()
        {
            return Database.ConnectionString;
        }
    }
}
