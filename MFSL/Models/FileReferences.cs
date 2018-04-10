using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MFSL.Models
{
    public class FileReferences
    {
        public int ReferenceNo { get; set; }
        public string Branch { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string OfficerId { get; set; }
        public int MemberNo { get; set; }
        public int FileNo { get; set; }
        public string FileStatus { get; set; }
        public string EmployerType { get; set; }
        public string Officer { get; set; }
        public string Approver { get; set; }
        public string PaymentOfficer { get; set; }
        public string CollateralOfficer { get; set; }
        public string Comment { get; set; }
    }
}