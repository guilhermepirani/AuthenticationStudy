using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

namespace AuthenticationStudy.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "GoogleOpenID";
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/denied";
            })
            .AddOpenIdConnect("GoogleOpenID", options =>
            {
                options.Authority = "https://accounts.google.com";
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]; ;
                options.CallbackPath = "/auth";
                options.SaveTokens = true;
                options.Events = new OpenIdConnectEvents()
                {
                    // With openID we can add roles after validating the token
                    OnTokenValidated = async context =>
                    {
                        if (context.Principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value == "105059851373434061162")
                        {
                            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                            claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                        }
                    }
                };
            });
            //.AddGoogle(options =>
            //{
            //    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            //    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]; ;
            //    options.CallbackPath = "/auth";
            //    options.AuthorizationEndpoint += "?prompt=consent";
            //});

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}