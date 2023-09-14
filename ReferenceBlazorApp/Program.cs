using AppsettingsProtector;
using AppsettingsProtector.Extensions;
using ReferenceBlazorApp.Data;

namespace ReferenceBlazorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddPersistedEncryptor<IPersistedBase64Encryptor, PersistedBase64Encryptor>(out var startupEncryptor);
            builder.Configuration.AddEncryptedJsonFile(source => {
                source.Path = "protectedSettings.json";
                source.Encryptor = startupEncryptor;
                source.TryEncryptOnDecryptFailure = true; // this is true anyway, but code is here to demonstrate the api exists
            });

            var secret = builder.Configuration["secret"];
            if (secret == null) {
                throw new ArgumentNullException(nameof(secret));
            }

            var googleConnectionString = builder.Configuration.GetConnectionString("Google");
            ArgumentNullException.ThrowIfNull(googleConnectionString);
            var microsoftConnectionString = builder.Configuration.GetConnectionString("Microsoft");
            ArgumentNullException.ThrowIfNull(microsoftConnectionString);

            var appSettings = builder.Configuration.Get<AppSettings>();
            ArgumentNullException.ThrowIfNull(appSettings);

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