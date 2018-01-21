using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFSL.Models;

namespace MFSL.Repository
{
    public interface IFileRepo
    {
        IList<FileStatus> GetAllFileStatus();
        IList<MemberFile> FetchAllMemberFiles();
        IList<MemberFile> FetchFileByMemberNo(int memberNo);
        IList<MemberFile> FetchMyFilesByMemberNo(int memberNo, string UserId);
    }
}
