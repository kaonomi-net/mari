// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Models
{
    using System.Threading.Tasks;

    /// <summary>
    /// Result returned by stage execution
    /// </summary>
    public readonly record struct StageStepResult(Type? expectedResultType, Task result);
}
