using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MFSL.ViewModels
{
    public class FileUpdateViewModel
    {
        [Required]
        [Display(Name = "Member No: ")]
        public int FileNo { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        [Display(Name = "Select File")]
        public HttpPostedFileBase ChequeCopy { get; set; }
    }
}