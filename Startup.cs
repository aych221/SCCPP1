using SCCPP1.Session;

namespace SCCPP1
{
    public class Startup
    {

        
        public void ConfigureServices(IServiceCollection services)
        {
            //this line was from original template code
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //services.AddSingleton<SessionData>();
            services.AddSingleton<SessionHandler>();

            //this line was from original template code
            services.AddRazorPages();

            services.AddDistributedMemoryCache();
            services.AddDataProtection();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }


        //this code was modified/restructured from the original template code
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //block is from original template code
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            //redirect http to https
            app.UseHttpsRedirection();

            //in case we need to use static access to dynamic pages
            app.UseStaticFiles();

            //cookie session
            app.UseSession();
            /*app.Use(async (context, next) =>
            {
                //does session exist?
                if (!context.Session.TryGetValue("Username", out var usernameBytes))
                {
                    context.Response.Redirect("/");
                    return;
                }

                //continue request if session exists
                await next();
            });*/

            app.UseRouting();

            //session auth
            app.UseAuthorization();

            //needed for mapping pages with links
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

    }
}