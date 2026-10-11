// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions
{
    using System.Threading.Tasks;

    /// <summary>
    /// Global provider of <see cref="IBlackboardRef"/>
    /// </summary>
    public interface IBlackboardProvider
    {
        /// <summary>
        /// Gets dedicated <see cref="IBlackboardRef"/> associate to <paramref name="boardName"/> in configuration <paramref name="boardTemplateConfigurationKey"/>
        /// </summary>
        ValueTask<IBlackboardRef> GetBlackboardAsync(Guid uid, CancellationToken token = default, Guid? callContextId = null);

        /// <summary>
        /// Gets dedicated <see cref="IBlackboardRef"/> associate to <paramref name="boardName"/> in configuration <paramref name="boardTemplateConfigurationKey"/>
        /// </summary>
        ValueTask<IBlackboardRef> GetBlackboardAsync(string boardName,
                                                  string boardTemplateConfigurationKey,
                                                  CancellationToken token = default, 
                                                  Guid? callContextId = null);

        /// <summary>
        /// Gets dedicated <see cref="IBlackboardRef"/> associate to <paramref name="boardName"/> in configuration <paramref name="boardTemplateConfigurationKey"/>
        /// </summary>
        ValueTask<IBlackboardRef> GetBlackboardAsync(string boardName,
                                                     Guid boardTemplateConfigurationUid,
                                                     CancellationToken token = default, 
                                                     Guid? callContextId = null);
    }
}
