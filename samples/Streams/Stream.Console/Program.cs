// See https://aka.ms/new-console-template for more information

using MARI.Framework;
using MARI.Framework.Bag.DebugTools;
using MARI.Framework.Bag.Toolbox.Abstractions;
using MARI.Framework.Builders;
using MARI.Framework.Configurations;
using MARI.Framework.Core.Abstractions.Enums;

using MARI.Toolbox.Abstractions.Models;

using Microsoft.Extensions.Logging;

var consumeSeq = Sequence.Build("display-input-with-delay", "Display input with deplay")
                         .RequiredInput<int>()
                         .Use<IDisplayInfoVGrain>().Call((g, m, ctx) => g.DisplayCallInfoAsync(m, ctx)).ReturnNoData
                         .Use<IDelayVGrain>().Configure(TimeSpanRange.Create(500, 700))
                                             .Call((g, ctx) => g.RandomDelayAsync(ctx)).Return
                         .Build();

var fixStreamDefinitionUid = new Guid("BA649F55-09A5-41C9-8C40-D0FCA3833EAE");

var streamDef = StreamQueue.CreateFromDefaultStream("test-flux", fixStreamDefinitionUid);

var fromStreamTrigger = Trigger.Stream(streamDef, "from-stream")
                               .MaxConcurrentProcess(2)
                               .AddTargetSequence(consumeSeq.Uid)
                               .Build();

//// All 5 sec push value to stream
var pushToStreamTrigger = Trigger.Cron("*/25 * * * * *", "push-every-25-sec", "TGR: Push Every 25 sec")
                                 .SetOutput(s => s.StaticCollection(Enumerable.Range(0, 50))
                                                       .PullMode(PullModeEnum.Broadcast))
                                 .AddTargetStream(streamDef)
                                 .Build();

var node = MARINode.Create(b =>
{
    b.WizardConfig()
     .NoCluster()

     .UseCronTriggers(supportSecondCron: true)
     .UseStreamQueues(b =>
     {
         b.SetupDefaultMARIMemoryStream();
     })

     .AddDebugTools(LogLevel.Warning)
     .AddToolBoxTools()

     .ConfigureLogging(c => c.AddConsole())

     .AddInMemoryDefinitionProvider(p =>
     {
         p.SetupSequences(consumeSeq)
          .SetupTriggers(fromStreamTrigger, pushToStreamTrigger)
          .SetupStreamQueues(streamDef);
     });
});

await using (node)
{
    await node.StartUntilEndAsync(async (p, handler, token) =>
    {
        //var streamProvider = p.GetRequiredServiceByName<IStreamProvider>(StreamQueueDefinition.DEFAULT_STREAM_KEY); /*GrainStreamingExtensions.GetStreamProvider(this, targetKV.Key);*/

        //var stream = streamProvider.GetStream<object>(StreamId.Get("TestFlux", fixStreamDefinitionUid));

        //for (int i = 0; i < 200; i++)
        //    await stream.OnNextAsync(i);
    });
}