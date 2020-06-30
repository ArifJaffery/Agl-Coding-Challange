using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.Models
{
    public class ErrorDetailModel
    {
        public string Code { get; set; }
        public string Target { get; set; }
        public string Message { get; set; }
    }
}
