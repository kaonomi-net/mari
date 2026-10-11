// See https://aka.ms/new-console-template for more information

using MARI.Framework.Builders;
using MARI.Framework.Core.Abstractions;
using MARI.Framework.Core.Abstractions.Sequence.Stages;

using Microsoft.AspNetCore.Mvc;

using Redirection.Api.Models;
using Redirection.Api.VGrain;

var builder = WebApplication.CreateBuilder();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
    {
        Description = "Grain Redirection sample",
        Title = "[Sample] Grain redirection"
    });

}).AddEndpointsApiExplorer();

var sentenceBuildSeq = Sequence.Build("sentence-builder", "SentenceBuilder")
                               .RequiredInput<ITextBuilder>()
                               .Use<ISubjectVGrain>().Call((g, i, ctx) => g.PopulateSubject(i, ctx)).Return
                               .Use<IActionVGrain>().Call((g, i, ctx) => g.PopulateAction(i, ctx)).Return
                               .Use<IComplementVGrain>().Call((g, i, ctx) => g.PopulateComplement(i, ctx)).Return
                               .Use<ISeparatorComplementVGrain>().Call((g, i, ctx) => g.PopulateComplement(i, ctx)).Return
                               .Use<IComplementVGrain>().Call((g, i, ctx) => g.PopulateComplement(i, ctx)).Return
                               .Build();

var complementStages = sentenceBuildSeq.Stages
                                       .Where(s => s is SequenceStageCallDefinition call && call.VGrainType.DisplayName == nameof(IComplementVGrain))
                                       .ToArray();

// cast, Select ..
var sentenceBuildSeqSelect = Sequence.Build("sentence-builder-select", "SentenceBuilder")
                                     .NoInput()
                                     .Select(() => new TextBuilder())
                                     .Use<ISubjectVGrain>().Call((g, i, ctx) => g.PopulateSubject(i, ctx)).Return
                                     .Use<IActionVGrain>().Call((g, i, ctx) => g.PopulateAction(i, ctx)).Return
                                     .Use<IComplementVGrain>().Call((g, i, ctx) => g.PopulateComplement(i, ctx)).Return
                                     .Use<ISeparatorComplementVGrain>().Call((g, i, ctx) => g.PopulateComplement(i, ctx)).Return
                                     .Use<IComplementVGrain>().Call((g, i, ctx) => g.PopulateComplement(i, ctx)).Return
                                     .Select(s => s.Text)
                                     .Build();

builder.Host.UseMARINode(b =>
{
    b.WizardConfig()
     .NoCluster()
     .ConfigureLogging(c => c.AddConsole())

     .AddInMemoryDefinitionProvider(m =>
     {
         m.SetupSequences(sentenceBuildSeq, sentenceBuildSeqSelect);
     });
});

var app = builder.Build();

app.UseSwagger();

// Minimal Api request
app.MapGet("/", context =>
{
    context.Response.Redirect("swagger");
    return Task.CompletedTask;
});

// Simply build sentence using 3 grains
app.MapGet("/build", async ([FromServices] IMARIExecutionHandler exec, CancellationToken token) =>
{
    return await exec.Sequence<ITextBuilder>(sentenceBuildSeq.Uid)
                     .SetInput(new TextBuilder())
                     .RunAsync<ITextBuilder>(token);
});

app.MapGet("/build/simplify", async ([FromServices] IMARIExecutionHandler exec, CancellationToken token) =>
{
    return await exec.Sequence(sentenceBuildSeqSelect.Uid)
                     .RunAsync<string>(token);
});

app.MapGet("/build/WithCustomRedirection", async ([FromServices] IMARIExecutionHandler exec, CancellationToken token) =>
{
    return await exec.Sequence<ITextBuilder>(sentenceBuildSeq.Uid, cfg =>
                     {
                         // first stage IComplementVGrain
                         // var uid = sentenceBuildSeq.Stages.OfType<SequenceStageCallDefinition>().First(s => s.VGrainType.AssemblyQualifiedName.Contains("IComplementVGrain")).Uid;

                         // Use contract redirection to target a different implementation
                         cfg.RedirectGrain<IComplementVGrain, IVeryComplementVGrain>();
                     })
                     .SetInput(new TextBuilder())
                     .RunAsync<ITextBuilder>(token);
});

app.MapPut("/global/redirect/complement", async ([FromServices] IMARIExecutionHandler exec, [FromQuery] bool apply, CancellationToken token) =>
{
    if (apply)
    {
        return await exec.VGrain<IAdminOperatorVGrain>()
                         .Call((g, ctx) => g.ApplyGlobalComplementRedirection(ctx))
                         .RunAsync(token);
    }

    return await exec.VGrain<IAdminOperatorVGrain>()
                     .Call((g, ctx) => g.ClearGlobalComplementRedirection(ctx))
                     .RunAsync(token);
});

app.MapPut("/global/redirect/conjunction", async ([FromServices] IMARIExecutionHandler exec, [FromQuery] bool apply, CancellationToken token) =>
{
    if (apply)
    {
        return await exec.VGrain<IAdminOperatorVGrain>()
                         .Call((g, ctx) => g.ApplyGlobalConjuctionRedirection(ctx))
                         .RunAsync(token);
    }

    return await exec.VGrain<IAdminOperatorVGrain>()
                     .Call((g, ctx) => g.ClearGlobalConjuctionRedirection(ctx))
                     .RunAsync(token);
});

app.MapPut("/global/redirect/clear", async ([FromServices] IMARIExecutionHandler exec, CancellationToken token) =>
{
    return await exec.VGrain<IAdminOperatorVGrain>()
                     .Call((g, ctx) => g.ClearAllRedirection(ctx))
                     .RunAsync(token);
});

app.MapGet("/global/redirect/get", async ([FromServices] IMARIExecutionHandler exec, CancellationToken token) =>
{
    return await exec.VGrain<IAdminOperatorVGrain>()
                     .Call((g, ctx) => g.GetAllRedirection(ctx))
                     .RunAsync(token);
});

app.UseSwaggerUI();

app.Run();
