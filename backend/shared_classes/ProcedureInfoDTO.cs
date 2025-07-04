using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared_classes
{
    public class ProcedureInfoDTO
    {
        public ProcedureInfoDTO(string procedureName, string? parameterName, string? dataType, string? parameterMode)
        {
            this.procedureName = procedureName;
            this.parameterName = parameterName;
            this.dataType = dataType;
            this.parameterMode = parameterMode;
        }

        public string procedureName { get; set; }
        public string? parameterName { get; set; }
        public string? dataType { get; set; }
        public string? parameterMode { get; set; }
    }
}
