using CommonEntityModel.EntityModel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace _3dposManagaement.Database
{
    public class RestaurantManagementDBContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public RestaurantManagementDBContext(DbContextOptions<RestaurantManagementDBContext> options) : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<ApplicationRole> ApplicationRole { get; set; }
        public DbSet<RoleUpdateHistory> RoleUpdateHistory { get; set; }
        public DbSet<ErrorLog> ErrorLog { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }
        public DbSet<UserStatus> UserStatus { get; set; }
        public DbSet<UserStatusUpdateLog> UserStatusUpdateLog { get; set; }
        public DbSet<UnitType> UnitType { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<InventoryModel> InventoryModel { get; set; }  
        public DbSet<InventoryTransactionType> InventoryTransactionType { get; set; } ////Unnecessary
        public DbSet<ProductPurchase> ProductPurchase { get; set; }
        public DbSet<ProductPurchaseMaster> ProductPurchaseMaster { get; set; }
        public DbSet<CategoryWiseProductPurchaseSum> CategoryWiseProductPurchaseSum { get; set; }
        public DbSet<Resturant> Resturant { get; set; }
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
    }
}
