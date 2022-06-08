using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Database.Model;
using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Database.ModelClass;
using _3DPOSRegistrationApp.Repository.IRepository.ICommonRepository;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IResturantRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IUserRepository;
using _3DPOSRegistrationApp.Repository.Repository.CommonRepository;
using _3DPOSRegistrationApp.Repository.Repository.LogRepository;
using _3DPOSRegistrationApp.Repository.Repository.ResturantRepository;
using _3DPOSRegistrationApp.Repository.Repository.UserRepository;
using _3DPOSRegistrationApp.Utility.ServiceModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _3DPOSRegistrationApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        //[Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddControllers();

            services.AddDbContext<_3DPOS_DBContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("3DPOSDBConnection")
                    ));

            services.AddDbContext<RestaurantDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("RestaurantDBConnection")
                    ));

            services.Configure<SuperAdminInfo>(Configuration.GetSection("SuperAdminInfo"));
            //services.Configure<RestaurantSupperAdminInfo>(Configuration.GetSection("RestaurantSupperAdminInfo"));
            //services.Configure<RestaurantAdminInfo>(Configuration.GetSection("RestaurantAdminInfo"));
            //services.Configure<Restaurant3DPOSInfo>(Configuration.GetSection("Restaurant3DPOSInfo"));


            services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<_3DPOS_DBContext>()
            .AddDefaultTokenProviders();

            services.AddIdentityCore<AspNetUsers>()
            .AddEntityFrameworkStores<RestaurantDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
            });
            services.ConfigureApplicationCookie(options => options.LoginPath = "/User/Login");
            services.AddAutoMapper(typeof(ModelMapper));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IRoleLogRepository, RoleLogRepository>();
            services.AddScoped<IStatusLogRepository, StatusLogRepository>();
            services.AddScoped<ICommonRepository, CommonRepository>();
            services.AddScoped<IResturantRepository, ResturantRepository>();
            services.AddScoped<IDBCatalogRepository, DBCatalogRepository>();

            services.AddMvc();


            //Adding Authentication
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //})

            // Adding Jwt Bearer
            //.AddJwtBearer(options =>
            //{
            //    options.SaveToken = true;
            //    options.RequireHttpsMetadata = false;
            //    options.TokenValidationParameters = new TokenValidationParameters()
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidAudience = Configuration["JWT:ValidAudience"],
            //        ValidIssuer = Configuration["JWT:ValidIssuer"],
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
            //    };
            //});

            //services.Configure<AuthMessageSenderOptions>(Configuration);

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials().Build());
            //});
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        options.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidateIssuer = true,
            //            ValidateAudience = true,
            //            ValidateLifetime = true,
            //            ValidateIssuerSigningKey = true,
            //            ValidIssuer = Configuration["Jwt:Issuer"],
            //            ValidAudience = Configuration["Jwt:Issuer"],
            //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
            //        };
            //    });

        }

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

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //    name: "default",
            //    template: "{controller=Home}/{action=Index}/{id?}");
            //});

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseSwagger();
            //    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "_3DPOSRegistrationApp v1"));
            //}

            //app.UseRouting();
            //app.UseCookiePolicy();
            //app.UseAuthorization();
            //app.UseAuthentication();
            //app.UseCookiePolicy();
            //app.UseStaticFiles();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}
