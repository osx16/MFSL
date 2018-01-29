using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MFSL.Models
{
    public class vnpf_
    {
        public int RowId { get; set; }
        public Nullable<int> VNPF_Number { get; set; }
        public Nullable<int> Loan_Number { get; set; }
        public string Member_Fullname { get; set; }
        public string Loan_Balance { get; set; }
        public Nullable<double> Current_Repayment_Amount { get; set; }
    }
}