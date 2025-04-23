using BIA.Entity.Utility;
using BIA.Helper;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Serilog;
using BIA.DAL.Repositories;
using BIA.BLL.BLLServices;
using BIA.BLL.Utility;
using BIA.Common;
using BIA.Controllers;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.DAL.DBManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<OracleDataManagerV2>();
builder.Services.AddScoped<OracleDataManager>();
builder.Services.AddScoped<DALBiometricRepo>();
builder.Services.AddScoped<BLLCommon>();
builder.Services.AddScoped<ApiRequest>();
builder.Services.AddScoped<BLLOrder>();
builder.Services.AddScoped<BLLLog>();
builder.Services.AddScoped<BiometricApiCall>();
builder.Services.AddScoped<BaseController>();
builder.Services.AddScoped<GeoFencingValidation>();
builder.Services.AddScoped<BLLUserAuthenticaion>();
builder.Services.AddScoped<BLLSIMReplacement>();
builder.Services.AddScoped<BLLRetailerUserSync>();
builder.Services.AddScoped<BLLResubmit>();
builder.Services.AddScoped<BLLRAToDBSSParse>();
builder.Services.AddScoped<BLLRaiseComplaint>();
builder.Services.AddScoped<BllOrderBssService>();
builder.Services.AddScoped<BllHandleException>();
builder.Services.AddScoped<BLLGeofencing>();
builder.Services.AddScoped<BLLFTRRestriction>();
builder.Services.AddScoped<BLLFirstRecharge>();
builder.Services.AddScoped<BLLDynamic>();
builder.Services.AddScoped<BLLDivDisThana>();
builder.Services.AddScoped<BLLDBSSToRAParse>();
builder.Services.AddScoped<BLLDBSSNotification>();
builder.Services.AddScoped<BLLCommon>();
builder.Services.AddScoped<BllBiometricBssService>();
builder.Services.AddScoped<BL_Json>();
builder.Services.AddScoped<ApiManager>();
builder.Services.AddScoped<OrderScheduler>();
builder.Services.AddScoped<OrderApiCall>();
builder.Services.AddScoped<eShopAPICall>();
builder.Services.AddScoped<StarTrekCommonController>();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddControllers(options =>
{
    options.InputFormatters.Add(new FormInputFormatter());
    options.InputFormatters.Add(new XmlSerializerInputFormatter(new MvcOptions()));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

var environment = Environment.OSVersion.Platform == PlatformID.Unix ? "Linux" : "Windows";

var configuration = builder.Configuration;
var logPath = string.Empty;

if(environment == "Windows")
{
    logPath = configuration["ApplicationSettings:WindowsLogPath"].ToString();
}
else
{
    logPath = configuration["ApplicationSettings:LinuxLogPath"].ToString();
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

// Add logging with Serilog
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog();
});

// Register LogWriter with the logPath
builder.Services.AddSingleton<LogWriter>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<LogWriter>>();
    return new LogWriter(logger, logPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});

var app = builder.Build();

var logWriter = app.Services.GetRequiredService<LogWriter>();
logWriter.WriteDailyLog2("Application started successfully.");

app.UseResponseCompression();
app.UseCors("AllowAllOrigins");
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information($"Application started on {environment}.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly!");
}
finally
{
    Log.CloseAndFlush();
}
