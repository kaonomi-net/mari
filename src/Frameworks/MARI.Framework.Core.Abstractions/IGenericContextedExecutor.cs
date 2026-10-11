// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using Orleans.Concurrency;

    using System.Threading.Tasks;

    /// <summary>
    /// Define a generic executor
    /// </summary>
    public interface IGenericContextedExecutor<TContextInfo>
    {
        /// <summary>
        /// Execute and return result
        /// </summary>
        Task<TOutput?> RunAsync<TOutput>(IExecutionContext<TContextInfo> executionContext);

        /// <summary>
        /// Execute with <paramref name="input"/> and return result
        /// </summary>
        Task<TOutput?> RunAsync<TOutput, TInput>(TInput? input, IExecutionContext<TContextInfo> executionContext);

        /// <summary>
        /// Execute and wait the end
        /// </summary>
        Task RunAsync(IExecutionContext<TContextInfo> executionContext);

        /// <summary>
        /// Execute and wait the end
        /// </summary>
        Task RunWithInputAsync<TInput>(TInput? input, IExecutionContext<TContextInfo> executionContext);

        /// <summary>
        /// Fire the specific execution without expecting result
        /// </summary>
        [OneWay]
        Task Fire(IExecutionContext<TContextInfo> executionContext);

        /// <summary>
        /// Fire the specific execution without expecting result
        /// </summary>
        [OneWay]
        Task Fire<TInput>(TInput? input, IExecutionContext<TContextInfo> executionContext);
    }
}
