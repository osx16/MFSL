using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MFSL.Models
{
    public class MemberFile
    {
        public int FileNo { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string OfficeId { get; set; }
        public int MemberNo { get; set; }
        public byte[] LoanApplication { get; set; }
        public byte[] LoanAgreement { get; set; }
        public byte[] GuaranteeCertificate { get; set; }
        public byte[] Amortisation { get; set; }
        public byte[] ChequeCopy { get; set; }
        public byte[] Eligibility { get; set; }
        public byte[] RequestLetter { get; set; }
        public byte[] EmployerLetter { get; set; }
        public byte[] Quotation { get; set; }
        public byte[] Payslip { get; set; }
        public byte[] BankAccStatement { get; set; }
        public byte[] LoanStatement { get; set; }
        public byte[] VNPFStatement { get; set; }
        public byte[] StandingOrder { get; set; }
        public byte[] CustomerID { get; set; }
        public System.Guid fileGUID { get; set; }
        public int FStatusId { get; set; }
    }
}