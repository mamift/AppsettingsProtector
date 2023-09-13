using AppsettingsProtector.Extensions;
using ReferenceBlazorApp.Data;

namespace ReferenceBlazorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddPersistentEncryptor(out var startupEncryptor);
            builder.Configuration.AddEncryptedJsonFile(source => {
                source.Path = Path.Combine(Environment.CurrentDirectory, "protectedSettings.json");
                source.Encryptor = startupEncryptor;
                source.TryEncryptOnDecryptFailure = true; // this is true anyway, but code is here to demonstrate the api exists
            });

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();

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

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}