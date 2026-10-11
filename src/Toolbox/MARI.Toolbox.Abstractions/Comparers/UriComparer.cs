// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Comparers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Compare Uri
    /// </summary>
    /// <seealso cref="IEqualityComparer{Uri}" />
    public sealed class UriComparer : IEqualityComparer<Uri>
    {
        #region Field

        private readonly bool _checkFragment;
        private readonly StringComparison _stringComparison;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="UriComparer"/> class.
        /// </summary>
        static UriComparer()
        {
            WithFragment = new UriComparer(true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UriComparer"/> class.
        /// </summary>
        public UriComparer(bool checkFragment, StringComparison stringComparison = StringComparison.Ordinal)
        {
            this._checkFragment = checkFragment;
            this._stringComparison = stringComparison;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a comparer that use the fragment in comparaison.
        /// </summary>
        public static UriComparer WithFragment { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(Uri? x, Uri? y)
        {
            var xNull = x is null;
            var yNull = y is null;

            if (xNull && yNull)
                return true;

            if (xNull || yNull)
                return false;

            if (this._stringComparison == StringComparison.Ordinal && x!.Equals(y) == false)
                return false;

            if (this._stringComparison != StringComparison.Ordinal && x!.OriginalString.Equals(y!.OriginalString, this._stringComparison) == false)
                return false;

            if (this._checkFragment && x!.Fragment.Equals(y!.Fragment, this._stringComparison) == false)
                return false;
            return true;
        }

        /// <inheritdoc />
        public int GetHashCode([DisallowNull] Uri obj)
        {
            if (this._stringComparison == StringComparison.Ordinal)
                return obj.GetHashCode();

            return obj.OriginalString.Length;
        }

        #endregion
    }
}
