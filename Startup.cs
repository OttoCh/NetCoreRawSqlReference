using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RawSql.Models;

namespace RawSql
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
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
            });

            //services.Add(new ServiceDescriptor(typeof(Context), new Context(Configuration.GetConnectionString("DefaultConnection"))));
            //services.Add(new ServiceDescriptor(typeof(MemberContext), new MemberContext(Configuration.GetConnectionString("DefaultConnection"))));
            
            //services.AddTransient<Database>(_ => new Database(Configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<MemberContext>( _ => new MemberContext(Configuration.GetConnectionString("DefaultConnection")) );
            services.AddTransient<Context>(_ => new Context(Configuration.GetConnectionString("DefaultConnection")) );

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Member}/{action=Index}/{id?}");
            });
        }
    }
}
