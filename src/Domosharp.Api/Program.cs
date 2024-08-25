using Domosharp.Api.Validators;
using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Commands.Hardwares;
using Domosharp.Infrastructure.HostedServices;
using Domosharp.Infrastructure.DBExtensions;
using Domosharp.Infrastructure.Repositories;
using Domosharp.Infrastructure.Validators;

using FluentValidation;

using Microsoft.OpenApi.Models;

using NLog.Web;

using Savorboard.CAP.InMemoryMessageQueue;

using System.Data;
using System.Data.SQLite;
using System.Text.Json.Serialization;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Implementation.HostedServices;

namespace Domosharp.Api;

#pragma warning disable S1118 // Utility classes should not have public constructors
public partial class Program
#pragma warning restore S1118 // Utility classes should not have public constructors
{
  public static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
        .AddEnvironmentVariables()
        .Build();

    builder.Host.UseNLog();

    var services = builder.Services;

    services.AddControllers()
             .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(a =>
    {
      a.SwaggerDoc("v1", new OpenApiInfo { Title = "Domosharp", Version = "v1" });
      a.UseInlineDefinitionsForEnums();
      a.OperationFilter<ReApplyOptionalRouteParameterOperationFilter>();
    });

    services.AddApiVersioning(a =>
    {
      a.DefaultApiVersion = new(1, 0);
      a.AssumeDefaultVersionWhenUnspecified = true;
      a.ReportApiVersions = true;
    }).AddApiExplorer(a =>
    {
      a.GroupNameFormat = "'v'VVV";
      a.SubstituteApiVersionInUrl = true;
    });

    SqlliteConfigExtensions.InitializeMapper();
    services.AddSingleton(configuration);
    var connection = new SQLiteConnection(configuration.GetConnectionString("sql") ?? string.Empty);
    await connection.OpenAsync();
    HardwareRepository.CreateTable(connection);
    DeviceRepository.CreateTable(connection);

    services.AddSingleton<IDbConnection>(connection);

    services.AddTransient<IValidator<Device>, DeviceValidator>();
    services.AddTransient<IValidator<IHardware>, HardwareValidator>();

    services.AddTransient<IDeviceRepository, DeviceRepository>();
    services.AddTransient<IHardwareWorker, HardwareWorker>();
    services.AddHardwareServices();

    services.AddMediatR(a =>
    {
      a.RegisterServicesFromAssemblyContaining<CreateHardwareCommand>();
      a.RegisterServicesFromAssemblyContaining<CreateHardwareCommandHandler>();
    });

    services.AddCap(x =>
    {
      x.UseSqlite(configuration.GetConnectionString("sql"));
      x.UseInMemoryStorage();
      x.UseDashboard();
      x.UseInMemoryMessageQueue();
    });

    services.AddSingleton<MainWorker>();
    services.AddSingleton<IMainWorker>(p => p.GetRequiredService<MainWorker>());

    services.AddHostedService(p => p.GetRequiredService<MainWorker>());

    builder.Logging.ClearProviders();

    var listeningAddresses = new List<string>()
    {
        GetWebUri(configuration)
    };

    var sslUri = GetSSLUri(configuration);
    if (sslUri is not null)
      listeningAddresses.Add(sslUri);

    builder.WebHost.UseUrls([.. listeningAddresses]);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI();
    }

    if (sslUri is not null)
      app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    var worker = app.Services.GetService<MainWorker>();
    if (worker is not null)
      await worker.StartAsync(CancellationToken.None);

    await app.RunAsync();
  }

  private static string GetWebUri(IConfiguration configuration)
  {
    var webBind = configuration.GetValue<string?>("webBind");
    if (string.IsNullOrWhiteSpace(webBind))
      webBind = "*";
    var webPort = configuration.GetValue<int?>("webPort") ?? 8080;
    return $"http://{webBind}:{webPort}";
  }

  private static string? GetSSLUri(IConfiguration configuration)
  {
    var sslWebBind = configuration.GetValue<string?>("sslWebBind");
    if (string.IsNullOrEmpty(sslWebBind))
      return null;
    var sslWebPort = configuration.GetValue<int?>("sslWebPort") ?? 8443;
    return $"https://{sslWebBind}:{sslWebPort}";

  }
}
