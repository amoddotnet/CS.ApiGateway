using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.Stitching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using HotChocolate.Execution;

namespace CS.ApiGateway
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
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
            services.AddHttpContextAccessor();
            services.AddHttpClient("Orders", (sp, client) =>
            {
                // HttpContext context = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
                //if (context.Request.Headers.ContainsKey("Authorization"))
                //{
                //    client.DefaultRequestHeaders.Authorization =
                //        AuthenticationHeaderValue.Parse(
                //            context.Request.Headers["Authorization"]
                //                .ToString());
                //}

                client.BaseAddress = new Uri("https://localhost:44379/api/GraphQL");
            });
            services.AddHttpClient("Products", (sp, client) =>
            {
                //HttpContext context = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
                //if (context.Request.Headers.ContainsKey("Authorization"))
                //{
                //    client.DefaultRequestHeaders.Authorization =
                //        AuthenticationHeaderValue.Parse(
                //            context.Request.Headers["Authorization"]
                //                .ToString());
                //}

                client.BaseAddress = new Uri("https://localhost:44372/api/GraphQL");
            }
           );
            //).AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
            //        {
            //            TimeSpan.FromSeconds(1),
            //            TimeSpan.FromSeconds(5),
            //            TimeSpan.FromSeconds(10)
            //        }));


            services.AddDataLoaderRegistry();

            services.AddGraphQLSubscriptions();

            services.AddStitchedSchema(builder => builder
              .AddSchemaFromHttp("Orders")
              .AddSchemaFromHttp("Products")
            .AddExtensionsFromFile("./QueryExtension/extensions.graphql")
            .AddSchemaConfiguration(c =>
            {
                c.RegisterExtendedScalarTypes();
            })
            ).AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
            //services.AddStitchedSchema(builder => builder);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddOcelot(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors("MyPolicy");
            app.UseGraphQL();
            app.UseHttpsRedirection();
            //app.UseMvc();
            app.UseOcelot();
        }
    }
}
