#define DEBUG
#define DEBUG_HANDLER

#if DEBUG
#else
#undef DEBUG_HANDLER
#endif
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using SCCPP1;
using SCCPP1.Session;
using SCCPP1.Database.Requests;

internal class Program
{

    /// <summary>
    /// This will enable the database request system, which is currently unfinished.
    /// Enabling this will switch the system to use the <see cref="DbRequestManager"/> system instead of the <see cref="DatabaseConnector"/>.
    /// 
    /// </summary>
    public const bool DbRequestSystem = false;
    private static void Main(string[] args)
    {
        //before site loads
        if (DbRequestSystem)
        {
        }
        else
        {
            DatabaseConnector.InitiateDatabase();
        }
        //DatabaseConnector.SaveBrittany(new SCCPP1.User.Account(new SessionData("brittl"), false));
        var builder = WebApplication.CreateBuilder(args);


        // Sets up MS web identity using Microsoft's identity packages
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

       

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });
        builder.Services.AddRazorPages().AddMicrosoftIdentityUI();

        //these allow the session classes to be used without conflicting with the MS Identity's packages
        builder.Services.AddSingleton<SessionHandler>();
        builder.Services.AddSingleton<SessionModel>();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        //enable auth
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();

        app.Run();

    }
}