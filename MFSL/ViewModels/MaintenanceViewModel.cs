﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MFSL.ViewModels
{
    public class MaintenanceViewModel
    {
        [Required]
        [Display(Name = "File No: ")]
        public int FileNo { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        [Display(Name = "Select File")]
        public HttpPostedFileBase MaintenanceForm { get; set; }
    }
}