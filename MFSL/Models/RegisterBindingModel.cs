using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MFSL.Models
{
    public class RegisterBindingModel
    {
        [Required]
        [Display(Name = "VNPF No: ")]
        public int VnpfNo { get; set; }

        [Required]
        [Display(Name = "Loan No: ")]
        public int LoanNo { get; set; }

        [Required]
        [Display(Name = "First Name: ")]
        public string EmpFname { get; set; }

        [Display(Name = "Middle Name: ")]
        public string EmpMname { get; set; }

        [Required]
        [Display(Name = "Last Name: ")]
        public string EmpLname { get; set; }

        public DateTime DateCreated { get; set; }
        //----------------------------------------------
        [Required]
        [Display(Name = "User Roles")]
        public string UserRoles { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}