using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection;
using System;
using AutoPortal.Models.DbModels;

namespace AutoPortal.Libs
{
    public class SQL : DbContext
    {
        public static DbContextOptions DefaultDbOptions
        {
            get
            {
                DbContextOptionsBuilder<SQL> optionsBuilder = new DbContextOptionsBuilder<SQL>();
                optionsBuilder.UseMySql(Startup.SQLConnectionString, Startup.v_MysqlVersion);
                return optionsBuilder.Options;
            }
        }

        public static DbContextOptions BuildOptions(string connectionString, MySqlServerVersion v_MysqlVersion)
        {
            DbContextOptionsBuilder<SQL> optionsBuilder = new DbContextOptionsBuilder<SQL>();
            optionsBuilder.UseMySql(connectionString, v_MysqlVersion);
            return optionsBuilder.Options;
        }

        public SQL(DbContextOptions<SQL> options) : base(options)
        { }

        public SQL() : base(SQL.DefaultDbOptions)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*modelBuilder.Entity<T>(e =>
            {
                e.HasKey(e => new { e.x, e.y });
            });*/

            base.OnModelCreating(modelBuilder);
        }

        #region Models

        public DbSet<User> users { get; set; }
        #endregion
    }
}
