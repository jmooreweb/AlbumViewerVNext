﻿using AlbumViewerBusiness;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Dnx.Runtime;
using System.Linq;
using Microsoft.AspNet.Cors.Infrastructure;
using Microsoft.Extensions.PlatformAbstractions;

namespace AlbumViewerAspNet5
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public static IConfiguration Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Add MVC services to the services container.
            services.AddMvc();

            // Add EF services to the services container
            services.AddEntityFramework()                
                        .AddSqlServer()
                        .AddDbContext<AlbumViewerContext>(options =>
                        {
                            var val = Configuration["Data:MusicStore:ConnectionString"];
                            options.UseSqlServer(val);
                        });

            // Inject DbContext as per Request context
            services.AddScoped<AlbumViewerContext>();

            //services.AddTransient<User>();
            //services.AddTransient<UserRole>();

            //services.AddIdentity<User,UserRole>();
            services.AddAuthentication();

            services.Configure<CorsOptions>(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                   builder
                      .WithOrigins("*")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
                });
            });

        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            // Configure the HTTP request pipeline.

            
            // Add the console logger.
            loggerfactory.AddConsole();

            // Add the following to the request pipeline only in development environment.
            if (env.IsEnvironment("Development"))
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage( new ErrorPageOptions() );
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // send the request to the following path or controller action.
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIISPlatformHandler();

            //app.UseIdentity();

            // Enable Cookie Auth with automatic user policy
            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthenticate = true;
                //options.AccessDeniedPath = "/api/Login";
                options.LoginPath = "/api/login";
            });

            // 
            //app.Run(async context =>
            //{
            //    if (!context.User.Identities.Any(identity => identity.IsAuthenticated))
            //    {
            //        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "bob") }, CookieAuthenticationDefaults.AuthenticationScheme));
            //        await context.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user);

            //        context.Response.ContentType = "text/plain";
            //        await context.Response.WriteAsync("Hello First timer");
            //        return;
            //    }

            //    context.Response.ContentType = "text/plain";
            //    await context.Response.WriteAsync("Hello old timer");
            //});

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "api",
                    template: "api/{action}/{id?}",
                    defaults: new {controller = "AlbumViewerApi", action = "Index"});
                routes.MapRoute(
                    name: "version",
                    template: "version/{action}",
                    defaults: new {controller = "VersionApi", action = "Index"});
                routes.MapRoute(
                    name: "mvc",
                    template: "mvc/{action}/{id?}",
                    defaults: new {controller = "AlbumViewerMvc", action = "Index"});


                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }

    public class UserRole
    {

    }
}
