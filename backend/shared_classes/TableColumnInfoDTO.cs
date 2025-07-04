using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared_classes
{
    public class TableColumnInfoDTO
    {
        public TableColumnInfoDTO(string tableName, string columnName, string dataType, bool isNullable)
        {
            this.tableName = tableName;
            this.columnName = columnName;
            this.dataType = dataType;
            this.isNullable = isNullable;
        }

        public string tableName { get; set; }
        public string columnName { get; set; }
        public string dataType { get; set; }
        public bool isNullable { get; set; }
    }
}
