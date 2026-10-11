// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.ComServer.Abstraction.Test
{
    using MARI.Toolbox.Abstractions.Attributes;

    [RefRoute("chat")]
    public sealed record class ChatMessage(string From, string Message, DateTime CreateTime);

    public sealed record class OtherClientsRequest();

    public sealed record class OtherClientsResponse(IReadOnlyCollection<string> Clients);
}
