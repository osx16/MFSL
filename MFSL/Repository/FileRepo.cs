//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using MFSL.Models;

//namespace MFSL.Repository
//{
//    public class FileRepo:IFileRepo
//    {
//        private MFSLEntities _dataContext;

//        public FileRepo()
//        {
//            _dataContext = new MFSLEntities();
//        }
//        public IList<FileStatus> GetAllFileStatus()
//        {
//            var query = from i in _dataContext.FileStatus
//                        select i;
//            var content = query.ToList<FileStatus>();
//            return content;
//        }
//        public IList<MemberFile> FetchAllMemberFiles()
//        {
//            var query = (from i in _dataContext.MemberFile
//                         select i)
//                         .OrderBy(x=>x.MemberNo)
//                         .ToList();
//            return query;
//        }
//        public IList<MemberFile> FetchFileByMemberNo(int memberNo)
//        {
//            var query = (from i in _dataContext.MemberFile
//                         where i.MemberNo == memberNo
//                         select i)
//                         .OrderBy(x => x.MemberNo)
//                         .ToList();
//            return query;
//        }

//        public IList<MemberFile> FetchMyFilesByMemberNo(int memberNo, string UserId)
//        {
//            var query = (from i in _dataContext.MemberFile
//                         where i.MemberNo == memberNo &&
//                         i.OfficeId == UserId
//                         select i)
//             .OrderBy(x => x.MemberNo)
//             .ToList();
//            return query;
//        }


//    }
//}