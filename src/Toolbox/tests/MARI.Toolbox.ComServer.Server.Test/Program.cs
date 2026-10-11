// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

using MARI.Toolbox.Communications;
using MARI.Toolbox.ComServer.Abstraction.Test;
using MARI.Toolbox.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System.Text;

var builder = new ServiceCollection();

builder.AddLogging(c => c.AddConsole());

var serviceProvider = builder.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Server");

await using (var server = new ComServer(4242, int.MaxValue))
{
    var handlers = new Dictionary<Guid, ComApiHandler>();

    var apiDesc = new ComApiDescriptor();
    apiDesc.AddApiCommandHanlder<ChatMessage>((msg, id) => ChatMessageReceivedAsync(msg, id, server, i => handlers[i]))
           .AddApiRequestHanlder<OtherClientsRequest, OtherClientsResponse>((msg, id) => OtherClientsRequestAsync(msg, id, server));

    logger.OptiLog(LogLevel.Information, "-- Server Started : 4242 -- ");

    await server.StartAsync(CancellationToken.None);

    logger.OptiLog(LogLevel.Information, "-- Server Start -- ");

    try
    {
        while (true)
        {
            var client = await server.WaitNextClientAsync(CancellationToken.None);
            logger.OptiLog(LogLevel.Information, "-- New Client -- ");

            var handler = ComApiHandler.Create(client, apiDesc, "Server");
            handlers.Add(client.Uid, handler);
        }
    }
    finally
    {
        foreach (var handler in handlers)
            await (handler.Value?.DisposeAsync() ?? ValueTask.CompletedTask);
    }
}

ValueTask<OtherClientsResponse> OtherClientsRequestAsync(OtherClientsRequest _, Guid sourceClientId, ComServer server)
{
    var otherClients = server.GetClientProxies()
                             .Where(c => c.Uid != sourceClientId)
                             .Select(c => c.Uid.ToString())
                             .ToArray();

    return ValueTask.FromResult(new OtherClientsResponse(otherClients));

    //throw new InvalidDataException("AHAHAHAHAAHAH FAILED");
}

async ValueTask ChatMessageReceivedAsync(ChatMessage message, Guid sourceClientId, ComServer comServer, Func<Guid, ComApiHandler> getApiHandler)
{
    var msg = JsonConvert.SerializeObject(message);
    var msgBytes = Encoding.UTF8.GetBytes(msg);

    foreach (var client in comServer.GetClientProxies().Where(c => c.Uid != sourceClientId))
    {
        var handler = getApiHandler(client.Uid);
        await handler.SendCommand(message);
    }

    //var otherClientTasks = comServer.GetClientProxies()
    //                                .Where(c => c.Uid != sourceClientId)
    //                                .Select(oc => getApiHandler(oc.Uid))
    //                                .Select(handler => handler.SendCommand(message))
    //                                .ToArray();

    //await otherClientTasks.SafeWhenAllAsync();
}