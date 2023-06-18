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
            modelBuilder.Entity<VehicleModel>(e =>
            {
                e.HasKey(e => new { e.make, e.model });
            });

            modelBuilder.Entity<UserRole>(e =>
            {
                e.HasKey(e => new { e.roleId, e.userId });
            });
            modelBuilder.Entity<VehiclePermission>(e =>
            {
                e.HasKey(e => new { e.vehicle_id, e.permission, e.target_id, e.target_type });
            });

            base.OnModelCreating(modelBuilder);
        }

        #region Models

        public DbSet<User> users { get; set; }
        public DbSet<BodyType> bodyTypes { get; set; }
        public DbSet<Models.DbModels.DriveType> driveTypes { get; set; }
        public DbSet<FuelType> fuelTypes { get; set; }
        public DbSet<TransmissionType> transmissionTypes { get; set; }
        public DbSet<Vehicle> vehicles { get; set; }
        public DbSet<VehicleCategory> vehicleCategories { get; set; }
        public DbSet<VehicleMake> vehicleMakes { get; set; }
        public DbSet<VehicleModel> vehicleModels { get; set; }
        public DbSet<Role> roles { get; set; }
        public DbSet<UserRole> userRoles { get; set; }
        public DbSet<VehiclePermission> vehiclePermissions { get; set; }
        public DbSet<OtherCost> otherCosts { get; set; }
        public DbSet<CrashEvent> crashEvents { get; set; }
        public DbSet<VehicleOwnerChange> vehicleOwnerChanges { get; set; }
        public DbSet<Refuel> refuels { get; set; }
        public DbSet<ServiceEvent> serviceEvents { get; set; }
        public DbSet<Service> services { get; set; }
        public DbSet<Factory> factories { get; set; }
        public DbSet<Dealer> dealers { get; set; }
        public DbSet<Review> reviews { get; set; }
        public DbSet<MileageStand> mileageStands { get; set; }
        #endregion
    }
}
