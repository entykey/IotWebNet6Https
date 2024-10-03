using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorAppNet6;
using IOTWeb.Services;
using Microsoft.Fast.Components.FluentUI;
using Blazored.Toast;
// using Microsoft.FluentUI.AspNetCore.Components;  // FluentUI toast, menu,...
// using Microsoft.FluentUI.AspNetCore.Components.DesignTokens;        // FluentUI toast, menu,...

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<WebSocketService>();
builder.Services.AddScoped<HttpControlService>();

builder.Services.AddFluentUIComponents();
// ALL THESE INJECTED SERVICES FOR TOAST of FluentUI not rendering toast !!!
// SO DON'T WASTE ANYMORE HOUR !!!
// builder.Services.AddScoped<IToastService, ToastService>();
// builder.Services.AddScoped<IDialogService, DialogService>();
// builder.Services.AddScoped<IMessageService, MessageService>();
// builder.Services.AddScoped<IKeyCodeService, KeyCodeService>();
// builder.Services.AddScoped<IMenuService, MenuService>(); // IMenuService not found
builder.Services.AddBlazoredToast();


await builder.Build().RunAsync();
