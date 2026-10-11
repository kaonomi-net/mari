// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.UnitTests.ToolKit.Models
{
    using AutoFixture.Kernel;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Multiple TypeRelay
    /// </summary>
    /// <seealso cref="AutoFixture.Kernel.ISpecimenBuilder" />
    public class MultiTypeRelay : ISpecimenBuilder
    {
        #region Fields

        private readonly IReadOnlyList<TypeRelay> _relays;
        private readonly Type _from;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiTypeRelay"/> class.
        /// </summary>
        public MultiTypeRelay(Type from, IReadOnlyCollection<Type> types)
        {
            this._from = from;
            this._relays = types.Select(t => new TypeRelay(from, t)).ToArray();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type typeRequest && typeRequest == this._from)
            {
                try
                {
                    var specimen = this._relays[Random.Shared.Next(0, this._relays.Count)].Create(request, context);
                    return specimen;
                } 
                catch (Exception)
                {
                    return null!;
                }
            }
            return new NoSpecimen();
        }

        /// <summary>
        /// Creates the specified to.
        /// </summary>
        public static MultiTypeRelay Create<TFrom>(params Type[] to)
        {
            return new MultiTypeRelay(typeof(TFrom), to);
        }

        #endregion

    }
}
