// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.VGrains.Controllers
{
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Events;
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Queries;

    using Orleans;

    using System.Threading.Tasks;

    /// <summary>
    /// Controller in charge to managed blackboard eventBook
    /// </summary>
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public interface IBlackboardEventControllerGrain : IBlackboardBaseControllerGrain
    {
        /// <summary>
        /// Reacts to eventBook
        /// </summary>
        Task<IReadOnlyCollection<BlackboardCommand>?> ReactToEventsAsync(BlackboardEventBook eventBook, GrainCancellationToken token);

        /// <summary>
        /// Reacts to signal message
        /// </summary>
        Task<IReadOnlyCollection<BlackboardCommand>?> ManagedSignalMessageAsync(SignalMessage message, GrainCancellationToken token);
        
        /// <summary>
        /// Process a incoming query
        /// </summary>
        /// <returns>
        /// Specific command responses: <br/>
        ///     <see cref="BlackboardCommandRejectAction"/> : If query type is not supported. <br/>
        ///     <see cref="BlackboardCommandDeferredResponse"/> : Define reponse will arrived later through the deferred reponse mari system. <br/>
        ///     <see cref="BlackboardCommandResponse{TResponseRequested}"/> : Provide directly the query response. <br/>
        /// </returns>
        Task<IReadOnlyCollection<BlackboardCommand>?> ProcessRequestAsync<TResponseRequested>(BlackboardBaseQuery request, GrainCancellationToken token);
    }
}
