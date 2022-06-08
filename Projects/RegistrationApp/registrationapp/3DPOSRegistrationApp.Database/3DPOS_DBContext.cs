using _3DPOSRegistrationApp.Database.Model;
using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Database.ModelClass;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;

namespace _3DPOSRegistrationApp.Database
{
    public class _3DPOS_DBContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        private readonly SuperAdminInfo _superAdminInfo;
        public _3DPOS_DBContext(DbContextOptions<_3DPOS_DBContext> options, IOptions<SuperAdminInfo> superAdminInfo) : base(options)
        {
            this._superAdminInfo = superAdminInfo.Value;
        }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<ApplicationRole> ApplicationRole { get; set; }
        public DbSet<RoleUpdateHistory> RoleUpdateHistory { get; set; }
        public DbSet<ErrorLog> ErrorLog { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }
        public DbSet<UserStatus> UserStatus { get; set; }
        public DbSet<UserStatusUpdateLog> UserStatusUpdateLog { get; set; }
        public DbSet<Resturant> Resturant { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<District> District { get; set; }
        public DbSet<DBCatalog> DBCatalog { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserStatus>().HasData(
                    new UserStatus
                    {
                        UserStatusId = 1,
                        StatusType = "Active",
                        CreatedDate = DateTime.UtcNow
                    },
                    new UserStatus
                    {
                        UserStatusId = 2,
                        StatusType = "Terminated",
                        CreatedDate = DateTime.UtcNow
                    }
                );

            modelBuilder.Entity<ApplicationRole>().HasData(
                     new ApplicationRole
                     {
                         Id = 1,
                         Name = "SuperAdmin",
                         NormalizedName = "SuperAdmin",
                         ConcurrencyStamp = 1.ToString()
                     },
                     new ApplicationRole
                     {
                         Id = 2,
                         Name = "Admin",
                         NormalizedName = "Admin",
                         ConcurrencyStamp = 2.ToString()
                     },
                     new ApplicationRole
                     {
                         Id = 3,
                         Name = "User",
                         NormalizedName = "User",
                         ConcurrencyStamp = 3.ToString()
                     }
                 );

            var appUser = new ApplicationUser
            {
                Id = _superAdminInfo.Id,
                FullName = _superAdminInfo.FullName,
                Email = _superAdminInfo.Email,
                EmailConfirmed = true,
                UserName = _superAdminInfo.UserName,
                NormalizedUserName = _superAdminInfo.NormalizedUserName.ToUpper(),
                CreatedDate = DateTime.UtcNow,
                UserStatus = _superAdminInfo.UserStatus,
                NormalizedEmail = _superAdminInfo.NormalizedEmail.ToUpper(),
                LockoutEnabled = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            PasswordHasher<ApplicationUser> ph = new PasswordHasher<ApplicationUser>();
            appUser.PasswordHash = ph.HashPassword(appUser, _superAdminInfo.Password);

            //seed user
            modelBuilder.Entity<ApplicationUser>().HasData(appUser);

            //set user role to admin
            modelBuilder.Entity<IdentityUserRole<int>>().HasData(new IdentityUserRole<int>
            {
                RoleId = 1,
                UserId = 1
            });

            //modelBuilder.Entity<Resturant>()
            //    .HasIndex(p => new { p.ResturentCode })
            //    .IsUnique(true);
        }
    }
}
