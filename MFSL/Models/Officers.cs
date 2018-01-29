using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MFSL.Models
{
    public class Officers
    {
        public string OfficerId { get; set; }
        public int VnpfNo { get; set; }
        public int LoanNo { get; set; }
        public string EmpFname { get; set; }
        public string EmpMname { get; set; }
        public string EmpLname { get; set; }
        public System.DateTime DateCreated { get; set; }
    }
}