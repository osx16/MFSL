using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTServices.Models
{
    public class FileUpdateDTO
    {
        public int FileNo { get; set; }
        public byte[] ChequeCopy { get; set; }
    }
}