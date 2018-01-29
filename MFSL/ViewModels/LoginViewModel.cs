using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MFSL.Helpers;
using System.ComponentModel.DataAnnotations;

namespace MFSL.ViewModels
{
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            Username = Settings.Username;
            Password = Settings.Password;
        }

        [Required]
        [Display(Name = "Username: ")]
        public string Username { get; set; }
        [Required]
        [Display(Name = "Password: ")]
        public string Password { get; set; }
    }
}