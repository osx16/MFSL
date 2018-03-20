using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MFSL.ViewModels
{
    public class NewMemberViewModel
    {
        [Required]
        public string Member_Fullname { get; set; }

        [Required]
        public int Loan_Number { get; set; }

        [Required]
        public int VNPF_Number { get; set; }

    }
}