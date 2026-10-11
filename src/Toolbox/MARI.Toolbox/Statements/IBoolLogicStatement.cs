// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Statements
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public interface IBoolLogicStatement
    {
        /// <summary>
        /// Ask is statement is valid
        /// </summary>
        /// <param name="input">The inputs variables.</param>
        bool Ask(in ReadOnlySpan<bool> input);
    }
}
