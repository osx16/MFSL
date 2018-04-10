using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MFSL.ViewModels
{
    public class UpdateFileStatusViewModel
    {
        [Required]
        [Display(Name = "File No: ")]
        public int FileNo { get; set; }

        [Required]
        [Display(Name = "Officer: ")]
        public string Officer { get; set; }

        [Required]
        [Display(Name = "Member No: ")]
        public int MemberNo { get; set; }

        [Required]
        [Display(Name = "File Status: ")]
        public string FileStatus { get; set; }

        [Display(Name = "Comment: ")]
        public string Comment { get; set; }
    }
}