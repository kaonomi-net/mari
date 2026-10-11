// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

using MARI.Framework.Core.Abstractions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using Kaonomi.Sample.Forex.VGrain.Abstractions;

var builder = WebApplication.CreateBuilder(args);

await Task.Delay(3000);

builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "MARI forex sample",
                        Version = "v1"
                    });
                })

                // Allow to generate swagger description file based on minimal api
                .AddEndpointsApiExplorer();

// Add mari client
builder.Host.UseMARIClient(c =>
{
    c.WizardConfig()
     .NoCluster()
     .ConfigureLogging(c => c.AddConsole());
});

var app = builder.Build();

// Enable middleware swagger
app.UseSwagger();

app.MapGet("/forex/currency/{pair}", async ([FromServices] IMARIExecutionHandler handler, [FromRoute] string pair) =>
    {
        return await handler.VGrain<ICurrencyPairVGrain>()
                            .SetConfiguration(pair)
                            .SetInput(10)
                            .Call((a, quantity, ctx) => a.GetLastValues(10, ctx))
                            .RunAsync();
    })
    .WithDescription("Get X last currency pair forex values");

// Enable swagger interface
app.UseSwaggerUI();

app.Run();
