using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Roblox.Sessions.Client;
using Roblox.Users.Client;

namespace Roblox.Web.WebAPI
{
    public class WebApiStartup
    {
        private string commentsPath { get; set; }
        public void RegisterApiConfiguration(string name, string description, string[] versions, string newCommentsPath)
        {
            Pages.Docs.pageTitle = name;
            Pages.Docs.pageDescription = description;
            Pages.Docs.versions = versions;
            commentsPath = newCommentsPath;
        }
        
        public void RegisterServices(IConfiguration config, IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddControllers().AddJsonOptions(options => {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            // services
            services.AddScoped<IUsersV1Client, UsersV1Client>(_ => new (config.GetSection("ApiClients:Users:BaseUrl").Value, config.GetSection("ApiClients:Users:ApiKey").Value));
            services.AddScoped<ISessionsV1Client, SessionsV1Client>(_ => new (config.GetSection("ApiClients:Sessions:BaseUrl").Value, config.GetSection("ApiClients:Sessions:ApiKey").Value));
            // config attributes
            LoggedInAttribute.SetClients(new SessionsV1Client(config.GetSection("ApiClients:Sessions:BaseUrl").Value, config.GetSection("ApiClients:Sessions:ApiKey").Value), new UsersV1Client(config.GetSection("ApiClients:Users:BaseUrl").Value, config.GetSection("ApiClients:Users:ApiKey").Value));
            services.AddSwaggerGen(c =>
            {
                foreach (var item in Pages.Docs.versions)
                {
                    c.SwaggerDoc(item, new OpenApiInfo { Title = Pages.Docs.pageTitle + " " + item.ToUpper() , Version = item});
                }
                c.IncludeXmlComments(commentsPath);
            });
        }

        public void RegisterConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger(c =>
                {
                    c.SerializeAsV2 = true;
                    c.RouteTemplate = "/docs/json/{documentname}";
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "../../Roblox.Web.WebAPI/Roblox.Web.WebAPI/Public/")),
                RequestPath = ""
            });
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}