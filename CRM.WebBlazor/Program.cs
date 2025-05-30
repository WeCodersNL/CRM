using CRM.WebBlazor.Components;
using CRM.WebBlazor.Service;
using CRM.WebBlazor.Service.Identity;
using Microsoft.AspNetCore.Localization;
using MudBlazor;
using MudBlazor.Services;
using Serilog;
using System.Globalization;

namespace CRM.WebBlazor;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, config) =>
                config.ReadFrom.Configuration(context.Configuration)
                .Enrich.With(new LogsEnricher()));

            var apiBaseAddress = builder.Configuration["ApiBaseAddress"];
            builder.Services.AddHttpClient("DefaultClient", client =>
            {
                client.BaseAddress = new Uri(apiBaseAddress!);
            });

            builder.Services.AddHttpClient("SecureClient", client =>
            {
                client.BaseAddress = new Uri(apiBaseAddress!);
            });

            // Add services to the container.
            builder.Services.AddLocalization();
            var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("nl") };
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddMudServices();
            builder.Services.AddScoped<TokenStore>();
            builder.Services.AddScoped<RefreshTokenHandler>();
            builder.Services.AddScoped<AuthenticatedHttpClientService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.VisibleStateDuration = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly in Blazor Web app");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
