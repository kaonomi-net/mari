// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using System;
    using System.Collections;
    using System.Text;

    /// <summary>
    /// Base class exception of all system execeptions
    /// </summary>
    /// <seealso cref="System.Exception" />
    public abstract class MARIBaseException : Exception, IMARIException, IEquatable<MARIBaseException>
    {
        #region Fields
        
        private readonly SortedDictionary<string, object> _data;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIBaseException"/> class.
        /// </summary>
        public MARIBaseException(string message, ulong errorCode = 0, Exception? innerException = null)
            : base(message, innerException)
        {
            this._data = new SortedDictionary<string, object>(StringComparer.InvariantCulture);

            this.ErrorCode = errorCode;
            this.Data.Add(MARIErrorCodes.KEY, errorCode);

#if DEBUG
            this.Data.Add(MARIErrorCodes.KEY + "Debug", MARIErrorCodes.ToDecryptErrorCode(errorCode));
#endif
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a collection of key/value pairs that provide additional user-defined information about the exception.
        /// </summary>
        /// <remarks>
        ///     override to have a correct comparaison
        /// </remarks>
        public sealed override IDictionary Data
        {
            get { return this._data; }
        }

        /// <inheritdoc />
        public ulong ErrorCode { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public sealed override bool Equals(object? obj)
        {
            if (obj is MARIBaseException ex)
                return Equals(ex);
            return false;
        }

        /// <inheritdoc/>
        public bool Equals(MARIBaseException? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return string.Equals(this.Message, other.Message) &&
                   (this.InnerException?.Equals(other.InnerException) ?? other.InnerException is null) &&
                   this.ErrorCode == other.ErrorCode && 
                   this._data.All(kv => other.Data.Contains(kv.Key) &&
                                       ((object.Equals(other.Data[kv.Key], null) && object.Equals(kv.Value, null)) || other.Data[kv.Key]!.Equals(kv.Value))) &&
                   OnEquals(other);
        }

        /// <summary>
        /// Called when equality is validated
        /// </summary>
        protected virtual bool OnEquals(MARIBaseException other)
        {
            return true;
        }

        /// <inheritdoc/>
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.Message,
                                    this.InnerException,
                                    this.ErrorCode,
                                    this._data.Aggregate(0, (acc, kv) => HashCode.Combine(acc, kv.Key, kv.Value)),
                                    this.OnGetHashCode());
        }

        /// <summary>
        /// Called when to compute hash code
        /// </summary>
        protected virtual int OnGetHashCode()
        {
            return 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var build = new StringBuilder();

            build.Append(this.GetType().Name);
            build.AppendLine();
            
            build.Append("Message: ");
            build.Append(this.Message);
            build.AppendLine();

            if (this._data.Count > 0)
            {
                build.AppendLine();
                build.AppendLine("Data:");
            }

            foreach (var data in this._data)
            {
                build.Append(data.Key);
                build.Append(": ");
                build.Append(data.Value);
                build.AppendLine();
            }

            if (this.InnerException != null)
            {
                build.AppendLine();
                build.Append("-- Inner --");
                build.AppendLine();
                build.Append(this.InnerException);
                build.AppendLine();
            }

            return build.ToString();
        }

        #endregion
    }

    public abstract class MARIBaseException<TChild> : MARIBaseException, IEquatable<TChild>
    {
        protected MARIBaseException(string message,
                                         ulong errorCode = 0,
                                         Exception? innerException = null) 
            : base(message, errorCode, innerException)
        {
        }

        public bool Equals(TChild? other)
        {
            return base.Equals(other);
        }
    }

    public static class MARIExceptionExtension
    {
        /// <summary>
        /// Create a <see cref="MARIInternalException"/> to carry the exception (client side) through mari enviroment
        /// </summary>
        /// <remarks>
        ///     If exception is arleady a <see cref="MARIInternalException"/> the exception is simply returned
        /// </remarks>
        public static MARIInternalException ToMARIInternal(this MARIBaseException exception)
        {
            if (exception is MARIInternalException internalException)
                return internalException;

            return new MARIInternalException(exception);
        }
    }
}
