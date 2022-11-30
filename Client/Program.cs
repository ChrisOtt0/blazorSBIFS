using blazorSBIFS.Client;
using blazorSBIFS.Client.Services;
using blazorSBIFS.Client.ViewModels;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add DI Services
builder.Services.AddSingleton<IHttpService, HttpService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddTransient<ILoginViewModel, LoginViewModel>();
builder.Services.AddTransient<IRegisterViewModel, RegisterViewModel>();
builder.Services.AddSingleton<IProfileViewModel, ProfileViewModel>();
builder.Services.AddTransient<IGroupsViewModel, GroupsViewModel>();
builder.Services.AddTransient<IGroupViewModel, GroupViewModel>();
builder.Services.AddTransient<IDeleteAccountViewModel, DeleteAccountViewModel>();


await builder.Build().RunAsync();
