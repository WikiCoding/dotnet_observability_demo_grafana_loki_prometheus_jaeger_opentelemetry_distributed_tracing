using MassTransit;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using WebApp2;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.GrafanaLoki("http://localhost:3100", 
        labels: [new LokiLabel { Key = "app", Value = "dotnet-app-2" }],
        propertiesAsLabels: ["level"])
    .CreateLogger();

// Add services to the container.
builder.Services.AddSerilog();

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("DOTNET-APP-2"))
    .WithLogging(logging =>
    {
        logging.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            otlpOptions.Endpoint = new Uri("http://localhost:4317/v1/logs");
        });
    }).WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddSource("MassTransit");
        tracing.AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri("http://localhost:4317");
        });
    }).WithMetrics(metrics =>
    {
        metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation();
        //.AddPrometheusExporter();
    });

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddOpenTelemetry();

    x.AddConsumer<ConsumerService>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("admin");
            h.Password("admin");
        });

        cfg.ConfigureEndpoints(ctx);
    });
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
