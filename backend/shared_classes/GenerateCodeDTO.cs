using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared_classes
{
    public class GenerateCodeDTO
    {

        public GenerateCodeDTO(string projectName, string databaseName)
        {
            this.projectName = projectName;
            this.databaseName = databaseName;
        }

        public string projectName { get; set; }
        public string databaseName { get; set; }


    }
}
