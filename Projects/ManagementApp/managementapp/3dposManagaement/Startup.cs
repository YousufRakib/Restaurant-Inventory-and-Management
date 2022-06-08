using _3dposManagaement.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using _3dposManagaement.Repository.IRepository.ILogRepository;
using _3dposManagaement.Repository.Repository.LogRepository;
using CommonEntityModel.EntityModel;
using _3dposManagaement.Repository.IRepository.IProductCategoryRepository;
using _3dposManagaement.Repository.Repository.ProductCategoryRepository;
using _3dposManagaement.Repository.IRepository.IProductRepository;
using _3dposManagaement.Repository.Repository.ProductRepository;
using _3dposManagaement.Repository.IRepository.IUnitTypeRepository;
using _3dposManagaement.Repository.Repository.UnitTypeRepository;
using _3dposManagaement.Repository.IRepository.IInventoryRepository;
using _3dposManagaement.Repository.Repository.InventoryRepository;
using _3dposManagaement.Repository.IRepository.IProductPurchaseRepository;
using _3dposManagaement.Repository.Repository.ProductPurchaseRepository;
using _3dposManagaement.Repository.IRepository.IItemCategoryRepository;
using _3dposManagaement.Repository.Repository.ItemCategoryRepository;
using _3dposManagaement.Repository.IRepository.IItemRepository;
using _3dposManagaement.Repository.Repository.ItemRepository;
using _3dposManagaement.Repository.Repository.MenuRepository;
using _3dposManagaement.Repository.IRepository.IMenuRepository;
using _3dposManagaement.Repository.IRepository.IVariantRepository;
using _3dposManagaement.Repository.Repository.VariantRepository;
using _3dposManagaement.Repository.IRepository.IMenuLogRepository;
using _3dposManagaement.Repository.Repository.MenuLogRepository;
using _3dposManagaement.Repository.IRepository.IVarificationCodeRepository;

namespace _3dposManagaement
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddControllers();

            //services.AddDbContext<RestaurantManagementDBContext>(options =>
            //{
            //    options.UseSqlServer(Configuration.GetConnectionString("RestaurantDBConnection"),
            //              sqlServerOptionsAction: sqlOptions =>
            //              {
            //                  sqlOptions.EnableRetryOnFailure();
            //              });
            //});

            services.AddDbContext<RestaurantManagementDBContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("RestaurantDBConnection")
                    ));

            services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<RestaurantManagementDBContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => options.LoginPath = "/User/Login");
            
            services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUnitTypeRepository, UnitTypeRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IProductPurchaseRepository, ProductPurchaseRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IItemCategoryRepository, ItemCategoryRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IVariantRepository, VariantRepository>();
            services.AddScoped<IMenuLogRepository, MenuLogRepository>();
            services.AddScoped<IVarificationCodeRepository, VarificationCodeRepository>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=User}/{action=Login}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
