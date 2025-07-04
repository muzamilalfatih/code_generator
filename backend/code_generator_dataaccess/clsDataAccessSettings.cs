using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace code_generator_dataaccess
{
    public class clsDataAccessSettings
    {
        public static string ConnectionString(string database = "master")
        {
            return $"Server=localhost;Database={database};User Id=sa;Password=sa123456;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;";
        }
    }
}
