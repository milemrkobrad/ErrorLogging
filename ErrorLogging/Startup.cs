using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ErrorLogging.Common.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ErrorLogging
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseBrowserLink();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //}

            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "text/html";

                    var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        ErrorModel errorModel = new ErrorModel
                        {
                            ServerVariables = context.Request?.Headers?.Keys.Select(k => new Item(k, context.Request.Headers[k])).ToList(),
                            Cookies = context.Request?.Cookies?.Keys.Select(k => new Item(k, context.Request.Cookies[k])).ToList(),
                            Form = context.Request.HasFormContentType ? context.Request?.Form?.Keys.Select(k => new Item(k, context.Request.Form[k])).ToList() : null
                    };

                        //var xml = SerializeObject(errorModel);

                        XmlSerializer xsSubmit = new XmlSerializer(typeof(ErrorModel));
                        var subReq = errorModel;
                        var xml = "";

                        using (var sww = new StringWriter())
                        {
                            using (XmlWriter writer = XmlWriter.Create(sww))
                            {
                                xsSubmit.Serialize(writer, subReq);
                                xml = sww.ToString(); // Your XML
                            }
                        }

                        //LogException(error.Error, context);
                        await context.Response.WriteAsync("<h2>Greška na stranici.</h2> <br><p>" + xml + "</p>").ConfigureAwait(false);
                    }
                });
            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public string SerializeObject(object obj)
        {
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                serializer.Serialize(ms, obj);
                ms.Position = 0;
                xmlDoc.Load(ms);
                return xmlDoc.InnerXml;
            }
        }
    }
}
