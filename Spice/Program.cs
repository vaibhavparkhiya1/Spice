using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Microsoft.AspNetCore.Identity;
using System.Configuration;
using System;
using Microsoft.CodeAnalysis.Options;
using Spice.Utility;
using Microsoft.AspNetCore.Hosting;
using System.Drawing.Text;
using Stripe;
using Spice.Service;
using Stripe.Terminal;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);




// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(Option => Option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>()
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityCore<ApplicationUser>()
.AddRoles<IdentityRole>()
.AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<ApplicationUser,IdentityRole>>()
 .AddEntityFrameworkStores<ApplicationDbContext>()
 .AddDefaultTokenProviders()
 .AddDefaultUI();


builder.Services.AddScoped<IDbInitilizer,DbInitializer>();
//builder.Services.Configure<StripeSetting>(Configuration.GetSection("Stripe"));
builder.Services.Configure<StripeSetting>(
    builder.Configuration.GetSection("Stripe"));
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.Configure<EmailOptions>(
    builder.Configuration);

builder.Services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);

builder.Services.AddAuthentication().AddFacebook(
 facebookOptions =>
 {
     facebookOptions.AppId = "729509958431221";
     facebookOptions.AppSecret = "4552e3a4f83c91fec3b0e6a1f0f0cdda";
 });





builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.IdleTimeout= TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly= true;
});

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();



var app = builder.Build();

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}
var scope = app.Services.CreateAsyncScope();



var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitilizer>();



// Configure the HTTP request pipeline.



if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();



}
else
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];
//DbInitializer.Initialize();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
dbInitializer.Initialize();

app.UseSession();

app.UseAuthorization();


#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
         {
             endpoints.MapControllerRoute(
             name: "Areas",
             pattern: "{Area=Customers}/{controller=Home}/{action=Index}/{id?}");
             endpoints.MapRazorPages();
         });
#pragma warning restore ASP0014 // Suggest using top level route registrations
app.Run();


