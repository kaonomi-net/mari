Stream
====

## Summary

Use an EBS (Entreprise Bus Service) as storage an diffuseur of job to to.

Orlean can nativaly used different type of **ESB**. <br /> 
With M-A-R-I we create connector through trigger to push and pull.

## How to configure trigger

Send to stream :
```csharp
// PUSH
var trigger = Trigger.Cron("*/25 * * * * *", "push-every-25-sec", "TGR: Push Every 25 sec")

                     // The output must be set before the targets
                     .SetOutput(s => s.StaticCollection(Enumerable.Range(0, 50))
                                      .PullMode(PullModeEnum.Broadcast))

                     .AddTargetStream(streamDef) // <----- Add stream as target
                     .Build();

```

Consume from stream :
```csharp
// PULL
var fromStreamTrigger = Trigger.Stream(streamDef, "from-stream") // <----- Trigger that fire until theire is a message in the stream queue

                               // Limit the number of concurrent execution, Prevent consuming all messages without resources in the cluster to process them (CPU, RAM, ...)
                               .MaxConcurrentProcess(2)

                               .AddTargetSequence(consumeSeq.Uid)
                               .Build();

```