// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.UnitTests.ToolKit
{
    using Microsoft.Extensions.Logging;

    using System;

    public sealed record TestLog(LogLevel logLevel, EventId eventId, object? state, Exception? exception, string message);
}
