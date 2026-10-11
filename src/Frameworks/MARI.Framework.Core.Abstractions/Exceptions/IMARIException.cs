// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    /// <summary>
    /// Define exception raise by the mari system
    /// </summary>
    public interface IMARIException
    {

        /// <summary>
        /// Gets a code used to qualify error.
        /// </summary>
        /// <remarks>
        ///     Look for <see cref="MARIErrorCodes"/> to decrypt the code
        /// </remarks>
        ulong ErrorCode { get; }
    }
}
