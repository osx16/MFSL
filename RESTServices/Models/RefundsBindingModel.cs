using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTServices.Models
{
    public class RefundsBindingModel
    {
        public int FileNo { get; set; }
        public System.DateTime RequestDate { get; set; }
        public byte[] PaymentRequest { get; set; }
        public byte[] LoanStatement { get; set; }
        public byte[] ReconciliationSheet { get; set; }
        public byte[] RefundChequeCopy { get; set; }
        public string FileStatus { get; set; }
        public string Comment { get; set; }
    }
}