using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cw5.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cw5
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
           
            services.AddTransient<IStudentDbService, SqlServerStudentDbService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStudentDbService dbService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseMiddleware<>

           // app.Use(async(context,next) =>
          //  {
            //    if (!context.Request.Headers.ContainsKey("Index"))
            //    {
             //       context.Response.StatusCode = StatusCodes.Status401Unauthorized;
             //       await context.Response.WriteAsync("Nie podano indeksu");
             //       return;
             //   }

               // var bodyStream = string.Empty;
               // using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
              //  {
              //      bodyStream = await reader.ReadToEndAsync();
              //  }

                //³¹czenie z baz¹ danych
                // if(!dbService.CheckIndex(index))
                //{

                //}
                //var index = context.Response.WriteAsync("Nie podano");
               // await next();
           // });
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
