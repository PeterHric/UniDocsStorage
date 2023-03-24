using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using DocumentStorage;
using DocumentStorage.EFCore;
using DocumentStorage.Hdd;
using DocumentStorage.InMemory;
using DocumentStorage.Mongo;
using DocumentStorage.MsSql;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace DocumentStorageApi;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

public class Startup
{
    public void ConfigureServicesInMem(IServiceCollection services)
    {
        services.AddSingleton<IDocumentStorage, InMemoryDocumentStorage>();
        services.AddControllers();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                .AddJsonFile("appsettings.json");
        _configuration = builder.Build();

        string storageType = _configuration.GetValue<string>("StorageType");

        switch (storageType)
        {
            case "InMemory":
                services.AddSingleton<IDocumentStorage, InMemoryDocumentStorage>();
                break;
            case "HDD":
                services.AddSingleton<IDocumentStorage>(sp =>
                {
                    var configuration = sp.GetService<IConfiguration>();
                    return new HddDocumentStorage(_configuration);
                });
                break;
            case "MsSql":
                services.AddSingleton<IDocumentStorage>(sp =>
                {
                    var configuration = sp.GetService<IConfiguration>();
                    return new MssqlDocumentStorage(_configuration);
                });
                break;
            case "Mongo":
                var mongoClient = new MongoClient(_configuration["MongoConnectionString"]);
                services.AddSingleton<IMongoClient>(mongoClient);
                services.AddSingleton<IDocumentStorage, MongoDocumentStorage>();
                break;
            case "EntityFramework":
                services.AddDbContext<DocumentDbContext>(options => options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")));
                services.AddTransient<EfDocumentRepository>();
                break;
            default:
                throw new Exception($"Invalid storage type: {storageType}");
        }

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    /// <summary>
    /// The configuration - used optionally, hence nullable
    /// </summary>
    private IConfiguration? _configuration;
}
