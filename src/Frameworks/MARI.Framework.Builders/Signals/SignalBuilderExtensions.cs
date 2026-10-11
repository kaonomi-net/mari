// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Keep : MARI.Framework.Builders
namespace MARI.Framework.Builders
{
    using MARI.Framework.Builders.Signals;
    using MARI.Framework.Core.Abstractions.Signals;

    using System;

    /// <summary>
    /// Extension about signal building
    /// </summary>
    public static class SignalBuilderExtensions
    {
        /// <summary>
        /// Starts the building a sub signal.
        /// </summary>
        public static ISignalBuilder StartBuildSubSignal(this SignalDefinition parentSignal, string simpleNameIdentifier, string? displayName = null, Guid? uid = null, bool concatToParentName = true)
        {
            ArgumentNullException.ThrowIfNull(parentSignal);
            ArgumentNullException.ThrowIfNullOrEmpty(simpleNameIdentifier);

            return Signal.StartBuilding(simpleNameIdentifier, BuildSubSignalDisplayName(parentSignal, simpleNameIdentifier, displayName, concatToParentName), uid)
                         .Parent(parentSignal);
        }

        /// <summary>
        /// Starts the building a sub signal.
        /// </summary>
        /// <returns>
        ///     Return sub definition created
        /// </returns>
        public static SignalDefinition CreateSubSignal(this SignalDefinition parentSignal, string simpleNameIdentifier, string? displayName = null, Guid? uid = null, bool concatToParentName = true)
        {
            ArgumentNullException.ThrowIfNull(parentSignal);
            ArgumentNullException.ThrowIfNullOrEmpty(simpleNameIdentifier);

            return Signal.StartBuilding(simpleNameIdentifier, BuildSubSignalDisplayName(parentSignal, simpleNameIdentifier, displayName, concatToParentName), uid)
                         .Parent(parentSignal)
                         .Build();
        }

        /// <summary>
        /// Starts the building a sub signal.
        /// </summary>
        /// <returns>
        ///     Root definition
        /// </returns>
        public static SignalDefinition CreateSubSignal(this SignalDefinition parentSignal, string simpleNameIdentifier, string? displayName, out SignalDefinition subSignalDefinition, Guid? uid = null, bool concatToParentName = true)
        {
            subSignalDefinition = CreateSubSignal(parentSignal, simpleNameIdentifier, displayName, uid, concatToParentName);
            return parentSignal;
        }

        #region Tools

        /// <summary>
        /// Builds the sub signal display name; use <paramref name="simpleNameIdentifier"/> when no <paramref name="displayName"/> is provided.
        /// </summary>
        private static string? BuildSubSignalDisplayName(SignalDefinition parentSignal, string simpleNameIdentifier, string? displayName, bool concatToParentName)
        {
            if (!concatToParentName)
                return displayName;

            return parentSignal.Name + ":" + (string.IsNullOrEmpty(displayName) ? simpleNameIdentifier : displayName);
        }

        #endregion
    }
}
