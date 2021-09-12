#pragma warning disable 1591

using System;
using System.IO;
using System.Reflection;
using AnimeApi.Models;
using AnimeApi.Models.ModelValidators;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace AnimeApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0.0",
                    new OpenApiInfo
                    {
                        Title = "AnimeApi",
                        Version = "v1.0.0",
                        Contact = new OpenApiContact
                        {
                            Name = "StanlsSlav",
                            Email = "",
                            Url = new Uri("https://github.com/StanlsSlav"),
                        },
                        License = new OpenApiLicense
                        {
                            Name = "Licensed under MIT",
                            Url = new Uri("https://mit-license.org/")
                        }
                    });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddTransient<IValidator<Anime>, AnimeValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1.0.0/swagger.json", "AnimeApi v1.0.0"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}