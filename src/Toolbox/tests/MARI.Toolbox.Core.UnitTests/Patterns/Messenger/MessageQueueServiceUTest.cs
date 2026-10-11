// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Patterns.Messenger
{
    using AutoFixture;

    using MARI.Toolbox.Abstractions.Patterns.Messenger;
    using MARI.Toolbox.Helpers;
    using MARI.Toolbox.Patterns.Messenger;
    using MARI.Toolbox.UnitTests.ToolKit.Helpers;

    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="MessageQueueService"/>
    /// </summary>
    public sealed class MessageQueueServiceUTest
    {
        #region Fields
        
        private static readonly TimeSpan s_timeout;
        private static readonly TimeSpan s_unwantedDeliveryDelay = TimeSpan.FromMilliseconds(200);

        #endregion

        #region Nested

        private interface IMessageWithCategory
        {
            string? Category { get; }
        }

        private record class SimpleMessage(Guid Uid, string Text) : IMessage;

        private record class MessageWithCategory(Guid Uid, string Text, string? Category) : IMessage, IMessageWithCategory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueServiceUTest"/> class.
        /// </summary>
        static MessageQueueServiceUTest()
        {
            s_timeout = TimeSpan.FromSeconds(Debugger.IsAttached ? 1000 : 20);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Simples test, received eveythink sended
        /// </summary>
        [Fact]
        public async Task Simple()
        {
            using (var timeout = CancellationHelper.DisposableTimeout(s_timeout))
            {
                var fixture = ObjectTestHelper.PrepareFixture();

                var msgs = fixture.CreateMany<SimpleMessage>().ToReadOnly();

                await MessengerService_Tester_Impl(null, msgs, msgs, timeout.Content);
            }
        }

        /// <summary>
        /// Simples test, received eveythink sended
        /// </summary>
        [Fact]
        public async Task Simple_With_Category()
        {
            using (var timeout = CancellationHelper.DisposableTimeout(s_timeout))
            {
                var fixture = ObjectTestHelper.PrepareFixture();

                var msgs = fixture.CreateMany<SimpleMessage>().ToReadOnly();

                await MessengerService_Tester_Impl("Poney", msgs, msgs, timeout.Content);
            }
        }

        /// <summary>
        /// Simples test, received eveythink sended
        /// </summary>
        [Fact]
        public async Task Simple_With_Filter()
        {
            using (var timeout = CancellationHelper.DisposableTimeout(s_timeout))
            {
                var msgs = new[]
                {
                    new SimpleMessage(Guid.NewGuid(), "Poney Rose"),
                    new SimpleMessage(Guid.NewGuid(), "Mth"),
                    new SimpleMessage(Guid.NewGuid(), "Poney Rose"),
                };

                Func<SimpleMessage, string?, bool> predicate = (m, c) => m.Text.Contains("PONEY", StringComparison.OrdinalIgnoreCase);

                var expected = msgs.Where(m => predicate(m, "")).ToArray();

                Check.That(expected.Length).IsEqualTo(2);

                await MessengerService_Tester_Impl(null,
                                                   msgs,
                                                   expected,
                                                   timeout.Content,
                                                   predicate);
            }
        }


        /// <summary>
        /// Simples test, received eveythink sended
        /// </summary>
        [Fact]
        public async Task Categories()
        {
            using (var timeout = CancellationHelper.DisposableTimeout(s_timeout))
            {
                var fixture = ObjectTestHelper.PrepareFixture();

                var expectedCategory = "PONEY";

                var msgs = new[]
                {
                    new MessageWithCategory(Guid.NewGuid(), fixture.Create<string>(), fixture.Create<string>()),
                    new MessageWithCategory(Guid.NewGuid(), fixture.Create<string>(), expectedCategory),
                    new MessageWithCategory(Guid.NewGuid(), fixture.Create<string>(), fixture.Create<string>()),
                    new MessageWithCategory(Guid.NewGuid(), fixture.Create<string>(), expectedCategory),
                };

                var expected = msgs.Where(m => m.Category == expectedCategory).ToArray();

                Check.That(expected.Length).IsEqualTo(2);

                await MessengerService_Tester_Impl(expectedCategory, msgs, expected, timeout.Content);
            }
        }


        /// <summary>
        /// Messengers the service tester implementation.
        /// </summary>
        private async Task MessengerService_Tester_Impl<TMesssage>(string? category,
                                                                   IReadOnlyCollection<TMesssage> messageToSend,
                                                                   IReadOnlyCollection<TMesssage> expectedMessageToBeReceived,
                                                                   CancellationToken token,
                                                                   Func<TMesssage, string?, bool>? predicate = null)
            where TMesssage : IMessage, IEquatable<TMesssage>
        {
            using (var service = new MessageQueueService())
            {
                var messageReceived = new List<TMesssage>();
                var allExpectedReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

                if (expectedMessageToBeReceived.Count == 0)
                    allExpectedReceived.TrySetResult();

                var subToken = service.Subscribe<TMesssage>(category).Message(this, (msg, category) =>
                {
                    lock (messageReceived)
                    {
                        messageReceived.Add(msg);

                        if (messageReceived.Count >= expectedMessageToBeReceived.Count)
                            allExpectedReceived.TrySetResult();

                        token.ThrowIfCancellationRequested();
                    }
                    return ValueTask.CompletedTask;
                }, predicate);

                foreach (var msg in messageToSend)
                {
                    var pushCategory = category;

                    if (msg is IMessageWithCategory ctg)
                        pushCategory = ctg.Category;

                    service.Push<TMesssage>(msg, pushCategory);
                }

                // Signaled by the handler when the expected count is reached (bounded by the test timeout token)
                await allExpectedReceived.Task.WaitAsync(token);

                // Short, bounded wait to let unwanted deliveries (duplicates, filtered messages) of the fire-and-forget Push arrive
                await Task.Delay(s_unwantedDeliveryDelay, token);

                // unsubscribe
                subToken.Dispose();

                token.ThrowIfCancellationRequested();

                // Send again to ensure subscription have been correctly removed
                // SendAsync completes only once every remaining subscriber processed the message : no delay needed
                foreach (var msg in messageToSend)
                    await service.SendAsync<TMesssage>(msg, category, token);

                TMesssage[] receivedSnapshot;
                lock (messageReceived)
                {
                    receivedSnapshot = messageReceived.ToArray();
                }

                var results = receivedSnapshot.GroupBy(m => m.Uid)
                                              .ToDictionary(k => k.Key, v => v.ToArray());

                Check.WithCustomMessage("Same message received multiple times")
                     .ThatCode(() => results.Any(r => r.Value.Length > 1))
                     .WhichResult().IsFalse();

                var indexedExpectedResult = expectedMessageToBeReceived.ToDictionary(k => k.Uid);

                Check.WithCustomMessage("Message number received differ from expected")
                     .That(results.Count).IsEqualTo(indexedExpectedResult.Count);

                foreach (var msg in indexedExpectedResult)
                {
                    if (results.TryGetValue(msg.Key, out var result))
                    {
                        token.ThrowIfCancellationRequested();
                        var r = result.First();
                        Check.That(r).IsEqualTo(msg.Value);

                        continue;
                    }

                    throw new InvalidDataException("Expected msg not found in the results");
                }
            }
        }

        #endregion
    }
}
