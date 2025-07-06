using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.GrafanaLoki("http://localhost:3100", 
        labels: [new LokiLabel { Key = "app", Value = "dotnet-app-3" }],
        propertiesAsLabels: ["level"])
    .CreateLogger();

// Add services to the container.
builder.Services.AddSerilog();

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("DOTNET-APP-3"))
    .WithLogging(logging =>
    {
        logging.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            otlpOptions.Endpoint = new Uri("http://localhost:4317/v1/logs");
        });
    }).WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
        tracing.AddOtlpExporter();
    }).WithMetrics(metrics =>
    {
        metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddPrometheusExporter();
    });

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPrometheusScrapingEndpoint();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
