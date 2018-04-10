using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTServices.Models
{
    public class UpdateFileStatusViewModel
    {
        public int FileNo { get; set; }
        public string Officer { get; set; }
        public int MemberNo { get; set; }
        public string FileStatus { get; set; }
        public string Comment { get; set; }
    }
}