using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;

namespace WebApp1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    private readonly IPublishEndpoint _publishEndpoint;

    public WeatherForecastController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<MessageWrapper> Get()
    {
        string responseBody = await GetEnrichment();

        var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        var messageWrapper = new MessageWrapper(result, responseBody);

        await ExecuteAsync(messageWrapper);

        return messageWrapper;
    }

    private async Task<string> GetEnrichment()
    {
        using HttpClient client = new HttpClient();
        var res = await client.GetAsync("http://localhost:5115/Enrichment");
        Log.Information("Request to http://localhost:5115/Enrichment resulted with status code {}", res.StatusCode);
        res.EnsureSuccessStatusCode();

        return await res.Content.ReadAsStringAsync(); ;
    }

    private async Task ExecuteAsync(MessageWrapper msg, CancellationToken cancellationToken = default)
    {
        Log.Information("Publishing message downstream...");
        await _publishEndpoint.Publish(msg, cancellationToken);
    }
}
