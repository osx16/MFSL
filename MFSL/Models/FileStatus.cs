using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MFSL.Models
{
    public class FileStatus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FileStatus()
        {
            this.MemberFile = new HashSet<MemberFile>();
        }

        public int FileStatusId { get; set; }
        public string FStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MemberFile> MemberFile { get; set; }
    }
}