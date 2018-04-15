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
        public string LoanApprover { get; set; }
        public Nullable<System.DateTime> LoanApprovalDate { get; set; }
        public string PaymentOfficer { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public string CollateralOfficer { get; set; }
        public Nullable<System.DateTime> CollateralDate { get; set; }
        public string MaintenanceOfficer { get; set; }
        public Nullable<System.DateTime> PostingDate { get; set; }
        public Nullable<System.DateTime> MaintenanceApprover { get; set; }
        public Nullable<System.DateTime> MaintenanceApprovalDate { get; set; }
        public string RefundValidationOfficer { get; set; }
        public Nullable<System.DateTime> ValidationDate { get; set; }
        public string RefundApprovalOfficer { get; set; }
        public Nullable<System.DateTime> RefundApprovalDate { get; set; }
        public string RefundPaymentOfficer { get; set; }
        public Nullable<System.DateTime> RefundPaymentDate { get; set; }
        public string Comment { get; set; }
    }
}