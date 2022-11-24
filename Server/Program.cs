global using blazorSBIFS.Server.Data;
global using blazorSBIFS.Shared.Models;
global using blazorSBIFS.Server.Services.UserService;
global using blazorSBIFS.Shared.DataTransferObjects;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Authorization;
using blazorSBIFS.Server.Tools;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Service for fixing possible object cycles
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Database and salt setup ensurance
string conString = "Data Source=" + Environment.MachineName + ";Initial Catalog=dbSBIFS;Integrated Security=True;TrustServerCertificate=True";
FileAdapter fileTxt = new TextFile();
string configPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents\\SBIFS";

if (!Directory.Exists(configPath))
{
    Directory.CreateDirectory(configPath);
}
string saltPath = configPath + "\\salt.txt";
if (!File.Exists(saltPath))
{
    fileTxt.WriteTextToFile(saltPath, SecurityTools.GenerateSalt());
}

// Add DB Context
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(conString);
});

// Add Custom Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();

// Token authentication with bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new TextFile().GetAllTextFromFile(saltPath))),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
