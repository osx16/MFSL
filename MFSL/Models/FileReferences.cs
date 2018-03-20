using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MFSL.Models
{
    public class FileReferences
    {
        public int ReferenceNo { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string OfficerId { get; set; }
        public int MemberNo { get; set; }
        public int FileNo { get; set; }
        public string FileStatus { get; set; }
        public string EmployerType { get; set; }
    }
}