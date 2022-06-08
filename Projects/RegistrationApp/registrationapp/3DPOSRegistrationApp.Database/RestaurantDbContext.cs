using _3DPOSRegistrationApp.Database.Model;
using _3DPOSRegistrationApp.Database.ModelClass;
using CommonEntityModel.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace _3DPOSRegistrationApp.Database
{
    public class RestaurantDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<ApplicationRole> ApplicationRole { get; set; }
        public DbSet<RoleUpdateHistory> RoleUpdateHistory { get; set; }
        public DbSet<ErrorLog> ErrorLog { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }
        public DbSet<UserStatus> UserStatus { get; set; }
        public DbSet<Resturant> Resturant { get; set; }
        public DbSet<UserStatusUpdateLog> UserStatusUpdateLog { get; set; }
        public DbSet<UnitType> UnitType { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<InventoryModel> InventoryModel { get; set; } 
        public DbSet<InventoryTransactionType> InventoryTransactionType { get; set; } ////Unnecessary
        public DbSet<ProductPurchase> ProductPurchase { get; set; }
        public DbSet<ProductPurchaseMaster> ProductPurchaseMaster { get; set; }
        public DbSet<CategoryWiseProductPurchaseSum> CategoryWiseProductPurchaseSum { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<ItemCategory> ItemCategory { get; set; }
        public DbSet<CategoryWithMenu> CategoryWithMenu { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<VariantMaster> VariantMaster { get; set; }
        public DbSet<Variant> Variant { get; set; }
        public DbSet<ItemVariant> ItemVariant { get; set; }
        public DbSet<VarificationCode> VarificationCode { get; set; }
        public DbSet<ItemWithMenu> ItemWithMenu { get; set; }
        public DbSet<ItemWithCategory> ItemWithCategory { get; set; }
        public DbSet<MenuLog> MenuLog { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<ProductCategory>().Property(x => x.ProductCategoryName).IsRequired();

            modelBuilder.Entity<ProductCategory>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<Product>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<UnitType>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<ApplicationUser>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<ProductPurchase>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<ProductPurchaseMaster>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<CategoryWiseProductPurchaseSum>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<Menu>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<ItemCategory>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<CategoryWithMenu>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<CategoryWithMenu>().HasIndex(x => new { x.CategoryId });
            modelBuilder.Entity<CategoryWithMenu>().HasIndex(x => new { x.MenuId });
            modelBuilder.Entity<Item>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<VariantMaster>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<Variant>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<ItemVariant>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<VarificationCode>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<ItemWithMenu>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<ItemWithCategory>().HasIndex(x => new { x.RestaurantCode });
            modelBuilder.Entity<MenuLog>().HasIndex(x => new { x.RestaurantCode });

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

            modelBuilder.Entity<InventoryTransactionType>().HasData(
                    new InventoryTransactionType
                    {
                        Id = 1,
                        Type = "In"
                    },
                    new InventoryTransactionType
                    {
                        Id = 2,
                        Type = "Out"
                    }
                );

            modelBuilder.Entity<ApplicationRole>().HasData(
                     new ApplicationRole
                     {
                         Id = 1,
                         Name = "SupperAdmin",
                         NormalizedName = "SupperAdmin",
                         ConcurrencyStamp = 1.ToString()
                     },
                     new ApplicationRole
                     {
                         Id = 2,
                         Name = "RestaurantAdmin",
                         NormalizedName = "RestaurantAdmin",
                         ConcurrencyStamp = 2.ToString()
                     },
                     new ApplicationRole
                     {
                         Id = 3,
                         Name = "_3DPOSAdmin",
                         NormalizedName = "_3DPOSAdmin",
                         ConcurrencyStamp = 3.ToString()
                     }
                 );
        }
    }
}
