using Contracts;
using MassTransit;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Serilog;
using System.Diagnostics;
using System.Text.Json;

namespace WebApp2;

public class ConsumerService : IConsumer<MessageWrapper>
{
    public async Task Consume(ConsumeContext<MessageWrapper> context)
    {
        var serialized = JsonSerializer.Serialize(context.Message);
        Log.Information("Received message that looks like this: " + serialized);
        Log.Information("Doing something with the message....");
        await Task.CompletedTask;
    }
}
