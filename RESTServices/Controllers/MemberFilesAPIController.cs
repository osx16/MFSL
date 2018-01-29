using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using RESTServices.Models;
using Microsoft.AspNet.Identity;

namespace RESTServices.Controllers
{
    public class MemberFilesAPIController : ApiController
    {
        private MFSLEntities db = new MFSLEntities();
        private string OfficerID = System.Web.HttpContext.Current.User.Identity.GetUserId();

        [Route("api/MemberFilesAPI/GetAll")]
        public IQueryable<FileReferences> GetAllMemberFile()
        {
            return db.FileReferences;
        }

        [Route("api/MemberFilesAPI/GetFileForUser")]
        public IQueryable<FileReferences> GetFileForUser()
        {
            var UserId = User.Identity.GetUserId();
            return db.FileReferences.Where(x=>x.OfficerId == UserId)
                                    .OrderByDescending(x=>x.DateCreated);
        }

        [Route("api/MemberFilesAPI/GetFileByMemberNo/{MemberNo:int}")]
        public IQueryable<FileReferences> GetFileByMemberNo(int MemberNo)
        {
            var data = db.FileReferences.Where(x => x.MemberNo == MemberNo);
            return data;
        }

        [Route("api/MemberFilesAPI/GetMyFileByMemberNo/{MemberNo:int}")]
        public IQueryable<FileReferences> GetMyFileByMemberNo(int MemberNo)
        {
            var UserId = User.Identity.GetUserId();
            return db.FileReferences.Where(x => x.MemberNo == MemberNo && x.OfficerId == UserId);
        }

        [Route("api/MemberFilesAPI/GetMemberInfoByNo/{MemberNo:int}")]
        //[ResponseType(typeof(MemberFile))]
        public IEnumerable<vnpf_> GetMemberInfoByNo(int MemberNo)
        {
            return db.vnpf_.Where(f => f.VNPF_Number == MemberNo);
        }

        // GET: api/MemberFilesAPI/5
        [ResponseType(typeof(MemberFile))]
        public IHttpActionResult GetMemberFile(int id)
        {
            MemberFile memberFile = db.MemberFile.Find(id);
            if (memberFile == null)
            {
                return NotFound();
            }

            return Ok(memberFile);
        }

        // PUT: api/MemberFilesAPI/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutMemberFile(int id, MemberFile memberFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != memberFile.FileNo)
            {
                return BadRequest();
            }

            db.Entry(memberFile).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberFileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/MemberFilesAPI
        [ResponseType(typeof(MemberFile))]
        public IHttpActionResult PostMemberFile(MemberFile memberFile)
        {
            memberFile.OfficeId = User.Identity.GetUserId();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.MemberFile.Add(memberFile);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = memberFile.FileNo }, memberFile);
        }

        [Route("api/MemberFilesAPI/PostReference")]
        [ResponseType(typeof(MemberFile))]
        public IHttpActionResult PostReference(FileReferences fileRef)
        {
            var UserId = User.Identity.GetUserId();
            fileRef.OfficerId = UserId;
            var DateCreated = fileRef.DateCreated;
            var MemberNo = fileRef.MemberNo;
            var data = db.MemberFile.Where(x => x.OfficeId == UserId &&
                                             x.MemberNo == MemberNo)
                                             .OrderByDescending(x=>x.FileNo)
                                             .Select(x => x.FileNo)
                                             .ToList();
            fileRef.FileNo = data.First();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.FileReferences.Add(fileRef);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = fileRef.FileNo }, fileRef);
        }

        // DELETE: api/MemberFilesAPI/5
        [ResponseType(typeof(MemberFile))]
        public IHttpActionResult DeleteMemberFile(int id)
        {
            MemberFile memberFile = db.MemberFile.Find(id);
            if (memberFile == null)
            {
                return NotFound();
            }

            db.MemberFile.Remove(memberFile);
            db.SaveChanges();

            return Ok(memberFile);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MemberFileExists(int id)
        {
            return db.MemberFile.Count(e => e.FileNo == id) > 0;
        }
    }
}