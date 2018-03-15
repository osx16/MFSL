using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using RESTServices.Models;

namespace RESTServices.Controllers
{
    public class MemberFilesController : ApiController
    {
        private MFSLEntities db = new MFSLEntities();

        // GET: api/MemberFiles
        public IQueryable<MemberFile> GetMemberFile()
        {
            return db.MemberFile;
        }

        // GET: api/MemberFiles/5
        [ResponseType(typeof(MemberFile))]
        public async Task<IHttpActionResult> GetMemberFile(int id)
        {
            MemberFile memberFile = await db.MemberFile.FindAsync(id);
            if (memberFile == null)
            {
                return NotFound();
            }

            return Ok(memberFile);
        }

        // PUT: api/MemberFiles/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutMemberFile(int id, MemberFile memberFile)
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
                await db.SaveChangesAsync();
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

        // POST: api/MemberFiles
        [ResponseType(typeof(MemberFile))]
        public async Task<IHttpActionResult> PostMemberFile(MemberFile memberFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.MemberFile.Add(memberFile);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = memberFile.FileNo }, memberFile);
        }

        // DELETE: api/MemberFiles/5
        [ResponseType(typeof(MemberFile))]
        public async Task<IHttpActionResult> DeleteMemberFile(int id)
        {
            MemberFile memberFile = await db.MemberFile.FindAsync(id);
            if (memberFile == null)
            {
                return NotFound();
            }

            db.MemberFile.Remove(memberFile);
            await db.SaveChangesAsync();

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