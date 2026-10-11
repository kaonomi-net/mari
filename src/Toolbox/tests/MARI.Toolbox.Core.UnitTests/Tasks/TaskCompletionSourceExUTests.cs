// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Tasks
{
    using AutoFixture;

    using MARI.Toolbox.Tasks;
    using MARI.Toolbox.UnitTests.ToolKit.Helpers;

    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="ITaskCompletionSourceEx"/>
    /// </summary>
    public sealed class TaskCompletionSourceExUTests
    {
        #region Fields
        
        private readonly Fixture _fixture;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCompletionSourceExUTests"/> class.
        /// </summary>
        public TaskCompletionSourceExUTests()
        {
            this._fixture = ObjectTestHelper.PrepareFixture();
        }

        #endregion

        #region Methods

        [Theory]
        [InlineData(typeof(NoneType),  typeof(TaskCompletionSourceEx))]
        [InlineData(typeof(int), typeof(TaskCompletionSourceEx<int>))]
        public void Generic_TaskCompletion(Type resultType, Type completionSourceType)
        {
            var result = (resultType == NoneType.Trait) ? NoneType.Instance : Activator.CreateInstance(resultType);

            var completion = (ITaskCompletionSourceEx)Activator.CreateInstance(completionSourceType, new object?[] { resultType })!;

            Check.That(completion).IsNotNull();
            Check.That(completion.State).IsNotNull().And.IsEqualTo(resultType);

            Check.That(completion).IsNotNull();
            Check.That(completion.ExpectedResultType).IsEqualTo(resultType);

            var task = completion.GetTask();

            Check.That(task.Status).IsEqualTo(TaskStatus.WaitingForActivation);

            completion.SetResultObject(result);

            Check.That(task.Status).IsEqualTo(TaskStatus.RanToCompletion);

            var taskResult = task.GetResult();

            if (resultType == NoneType.Trait)
                taskResult = NoneType.Instance;

            Check.That(taskResult).IsNotNull().And.IsEqualTo(result);
        }

        [Theory]
        [InlineData(typeof(TaskCompletionSourceEx))]
        [InlineData(typeof(TaskCompletionSourceEx<int>))]
        public void Generic_TaskCompletion_Cancellation(Type completionSourceType)
        {
            var completion = (ITaskCompletionSourceEx)Activator.CreateInstance(completionSourceType, new object?[] { completionSourceType })!;

            Check.That(completion).IsNotNull();
            Check.That(completion.State).IsNotNull().And.IsEqualTo(completionSourceType);

            Check.That(completion).IsNotNull();

            var task = completion.GetTask();
            Check.That(task.Status).IsEqualTo(TaskStatus.WaitingForActivation);

            completion.SetCanceled();
            Check.That(task.Status).IsEqualTo(TaskStatus.Canceled);
        }

        [Theory]
        [InlineData(typeof(TaskCompletionSourceEx))]
        [InlineData(typeof(TaskCompletionSourceEx<int>))]
        public void Generic_TaskCompletion_Exceptions(Type completionSourceType)
        {
            var completion = (ITaskCompletionSourceEx)Activator.CreateInstance(completionSourceType, new object?[] { completionSourceType })!;

            Check.That(completion).IsNotNull();
            Check.That(completion.State).IsNotNull().And.IsEqualTo(completionSourceType);

            var task = completion.GetTask();
            Check.That(task.Status).IsEqualTo(TaskStatus.WaitingForActivation);

            var ex = new InvalidDataException();
            completion.SetException(ex);
            
            Check.That(task.Status).IsEqualTo(TaskStatus.Faulted);

            Check.That(task.Exception).IsNotNull();
            Check.That(task.Exception!.InnerExceptions).IsNotNull().And.CountIs(1);
            Check.That(task.Exception!.InnerExceptions.First()).IsNotNull().And.IsEqualTo(ex);
        }

        #endregion
    }
}
