// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.UnitTests.ToolKit.Models
{
    using AutoFixture.Kernel;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Multiple TypeRelay
    /// </summary>
    /// <seealso cref="AutoFixture.Kernel.ISpecimenBuilder" />
    public class KnownTypeRelay : ISpecimenBuilder
    {
        #region Fields

        private readonly MultiTypeRelay _relay;
        private readonly Type _from;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiTypeRelay"/> class.
        /// </summary>
        private KnownTypeRelay(Type from, IReadOnlyCollection<Type> types)
        {
            this._from = from;
            this._relay = new MultiTypeRelay(from, types);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type typeRequest && typeRequest == this._from)
                return this._relay.Create(request, context);

            return new NoSpecimen();
        }

        /// <summary>
        /// Creates the specified to.
        /// </summary>
        public static KnownTypeRelay Create<TFrom>(params Type[] excludeTypes)
        {
            var possibleTypes = typeof(TFrom).GetCustomAttributes<KnownTypeAttribute>()
                                             .Where(t => t.Type is not null && t.Type.IsAbstract == false)
                                             .Select(t => t.Type)
                                             .Except(excludeTypes)
                                             .Where(t => t is not null && t.IsAbstract == false)
                                             .ToArray();

            return new KnownTypeRelay(typeof(TFrom), possibleTypes!);
        }

        #endregion

    }
}
