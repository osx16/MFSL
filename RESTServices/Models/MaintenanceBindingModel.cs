using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTServices.Models
{
    public class MaintenanceBindingModel
    {
        public int FileNo { get; set; }
        public byte[] MaintenanceForm { get; set; }
        public string FileStatus { get; set; }
        public string Comment { get; set; }
    }
}