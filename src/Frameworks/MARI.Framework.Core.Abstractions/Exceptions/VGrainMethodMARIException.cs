// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;
    using MARI.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Raised when method doesn't have been founded on vgrain
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class VGrainMethodMARIException : MARIBaseException<VGrainMethodMARIException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainMethodMARIException"/> class.
        /// </summary>
        public VGrainMethodMARIException(string message, Exception? innerException = null)
            : base(message,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Sequence,
                                             MARIErrorCodes.PartType.Reflection,
                                             MARIErrorCodes.ErrorType.Invalid),
                   innerException)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Format exception about vgrain method not founded
        /// </summary>
        public static VGrainMethodMARIException MethodNotFounded(string signature, AbstractType vgrainType, Exception? innerException = null)
        {
            return new VGrainMethodMARIException(string.Format(MARIExceptionSR.MethodNotFound, signature, vgrainType), innerException);
        }

        /// <summary>
        /// Format exception about vgrain method not founded
        /// </summary>
        public static VGrainMethodMARIException MethodNotFounded(AbstractType vgrainType, AbstractType? output, string methodName, IReadOnlyCollection<AbstractType> argumentTypes)
        {
            var returnSignature = "Task";

            if (output is not null)
                returnSignature = string.Format("Task<{0}>", output);

            var args = "";

            if (argumentTypes != null && argumentTypes.Any())
                args = string.Join(", ", argumentTypes);

            return MethodNotFounded($"{returnSignature} {methodName}({args})", vgrainType);
        }

        #endregion
    }

    [GenerateSerializer]
    public struct VGrainMethodMARIExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class VGrainMethodMARIExceptionConverter : IConverter<VGrainMethodMARIException, VGrainMethodMARIExceptionSurrogate>
    {
        public VGrainMethodMARIException ConvertFromSurrogate(in VGrainMethodMARIExceptionSurrogate surrogate)
        {
           return new VGrainMethodMARIException(surrogate.Message, surrogate.InnerException);
        }

        public VGrainMethodMARIExceptionSurrogate ConvertToSurrogate(in VGrainMethodMARIException value)
        {
            return new VGrainMethodMARIExceptionSurrogate() { Message = value.Message, InnerException = value.InnerException };
        }
    }
}
