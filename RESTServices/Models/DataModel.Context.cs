﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RESTServices.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MFSLEntities : DbContext
    {
        public MFSLEntities()
            : base("name=MFSLEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Branches> Branches { get; set; }
        public virtual DbSet<EmployeeStatus> EmployeeStatus { get; set; }
        public virtual DbSet<FileReferences> FileReferences { get; set; }
        public virtual DbSet<FileStatus> FileStatus { get; set; }
        public virtual DbSet<FileType> FileType { get; set; }
        public virtual DbSet<MemberFile> MemberFile { get; set; }
        public virtual DbSet<Officers> Officers { get; set; }
        public virtual DbSet<vnpf_> vnpf_ { get; set; }
    }
}
