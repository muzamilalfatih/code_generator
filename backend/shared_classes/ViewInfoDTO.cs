using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared_classes
{
    public class viewInfoDTO
    {
        public viewInfoDTO(string viewName, string columnName, string columnType)
        {
            this.viewName = viewName;
            this.columnName = columnName;
            this.columnType = columnType;
        }

        public string viewName {  get; set; }
        public string columnName { get; set; }
        public string  columnType { get; set; }

    }
}
