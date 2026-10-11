// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Ensure Docker is running on you environment
// Create a mongoDB using the folling command "docker run -d -p 27017:27017 mongo:latest"
// You can watch and edit data in you data base using mongo tools like studio 3T https://studio3t.com/

using Common;

using MARI.Framework.Bag.DebugTools;
using MARI.Framework.Builders;
using MARI.Framework.Core.Abstractions.Enums;
using MARI.Framework.Extensions.Mongo.Abstractions;
using MARI.Framework.Extensions.Mongo.Models;

using MongoDB.Driver;

var increaseSequenceUid = new Guid("3E05942C-AB97-4F3E-92CF-2BE0D3E68DC9");
var increaseSequence = Sequence.Build("debug-display", "DebugDisplay", fixUid: increaseSequenceUid)
                               .RequiredInput<string>()
                               .Use<ICounterVGrain>().Call((g, key, ctx) => g.Increase(key, ctx)).Return
                               .Use<IDisplayInfoVGrain>().Call((g, o, ctx) => g.DisplayCallInfoAsync(o, ctx)).Return   
                               .Build();

var timeUid = new Guid("67B799EB-01D8-4475-88F4-95C912AC4611");
var timer = Trigger.Cron("* * * * *", "minut-timer", "MinutTimer", fixUid: timeUid)
                   .SetOutput(t => t.StaticCollection(new[] { "A", "B", "Other" })
                                       .PullMode(PullModeEnum.Random))
                   .AddTargetSequence(increaseSequence)
                   .Build();

MARIMongoDefaultSerializerConfig.SetupSerializationConfiguration();
MARIMongoDefaultSerializerConfig.SetupSerializationConfiguration();

var client = new MongoClient("mongodb://127.0.0.1:27017");
var db = client.GetDatabase("mari");
var collection = db.GetCollection<DefinitionContainer>("Definitions");

collection.ReplaceOne(f => f.Uid == increaseSequenceUid, DefinitionContainer.Create(increaseSequence), new ReplaceOptions() { IsUpsert = true });
collection.ReplaceOne(f => f.Uid == timeUid, DefinitionContainer.Create(timer), new ReplaceOptions() { IsUpsert = true });
