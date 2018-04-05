using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTServices.Models
{
    public class FileUpdateDTO
    {
        public int FileNo { get; set; }
        public string Officer { get; set; }
        public string Approver { get; set; }
        public string PaymentOfficer { get; set; }
        public string CollateralOfficer { get; set; }
        public byte[] LoanApplication { get; set; }
        public byte[] PaymentAdvice { get; set; }
        public byte[] ChequeCopy { get; set; }
        public byte[] CollateralCertificate { get; set; }
    }
}