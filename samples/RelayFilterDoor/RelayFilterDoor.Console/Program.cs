// See https://aka.ms/new-console-template for more information

using MARI.Framework;
using MARI.Framework.Builders;
using MARI.Framework.Builders.Signals;
using MARI.Framework.Configurations;
using MARI.Framework.Core.Abstractions.Enums;
using MARI.Framework.Core.Abstractions.Signals;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var signalInit = Signal.Create("loop-init", "loopInit");
var signalEveryMin = Signal.Create("every-min-signal", "everyMinSignal");
var signalAndTriggerPopulate = Signal.Create("and-trigger-populate", "andTriggerPopulate");

var loopDoor = Door.Create("loop-every-10sec", "loopEvery10sec")
                   .Listen(signalInit)
                   .UseLogicalAggregator(b =>
                   {
                       return b.ActiveWindowInterval(TimeSpan.FromSeconds(10))
                               .AssignVariableName("Init", signalInit)
                               .UseVariableThis()
                               .Formula("!Init & !this");
                   })
                   .Build();

var numberDoorSource = Trigger.Door(loopDoor, "number-source", "NumberSource")
                              .SetOutput(s => s.StaticCollection(new[] { 0, 1, 2, 3, 4, 5 })
                                                    .PullMode(PullModeEnum.Circling))
                              .AddTargetSignal(signalAndTriggerPopulate.SignalId)
                              .Build();

var oodDoor = Door.Create("ood-door-filter", "oodDoorFilter")
                  .Listen(signalAndTriggerPopulate)
                  .UseRelayFilter<int>((value, signal) => value % 2 == 1)
                  .Build();

var evenDoor = Door.Create("even-door-filter", "evenDoorFilter")
                   .Listen(signalAndTriggerPopulate)
                   .UseRelayFilter<int>((value, signal) => value % 2 == 0)
                   .Build();

var allSignals = new[]
{
    signalAndTriggerPopulate,
    signalEveryMin,
    signalInit
};

var allDoors = new[]
{
    oodDoor,
    evenDoor,
};

/*
 *    | [loopDoor] /10sec | -
 *                          - |                    | ----- [If % 2 = 1] --> Sequence Ood
 *                            | [numberDoorSource] | ----- [If % 2 = 0] --> Sequence Event
 *                                            > [1, 2, 3, 4, 5, 6]
 */
var node = MARINode.Create(b =>
{
    b.WizardConfig()
     .NoCluster()
     .ConfigureLogging(logging => logging.AddConsole())
     .UseSignals()

     .AddDebugTools(LogLevel.Warning)

     .ShowDoors(allDoors)

     .AddInMemoryDefinitionProvider(b =>
     {
         b.SetupSignals(signalInit,
                        signalEveryMin,
                        signalAndTriggerPopulate)

          .SetupDoors(loopDoor,
                      oodDoor,
                      evenDoor)

          .SetupTriggers(numberDoorSource);
     });
});

await using (node)
{
    await node.StartUntilEndAsync((service, execHandler, token) =>
    {
        var signalService = service.GetRequiredService<ISignalService>();
        signalService.Fire(signalInit.SignalId, token);

        return Task.CompletedTask;
    });
}