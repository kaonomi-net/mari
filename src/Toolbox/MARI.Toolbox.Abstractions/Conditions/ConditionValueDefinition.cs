// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Conditions
{
    using MARI.Toolbox.Models;

    using Newtonsoft.Json;

    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Store simple value
    /// </summary>
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public sealed class ConditionValueDefinition : ConditionBaseDefinition
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    {
        #region Fields

        public const string TypeDiscriminator = "value";

        private static readonly ConcurrentDictionary<Type, IEqualityComparer> s_comparerCache = new ConcurrentDictionary<Type, IEqualityComparer>();

        [NonSerialized]
        private IEqualityComparer? _comparer;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionValueDefinition"/> class.
        /// </summary>
        public ConditionValueDefinition(AbstractType type, object? value)
        {
            this.Type = type;
            this.Value = value;

            if (value is not null && type.IsEqualTo(value.GetType()) == false)
            {
                var serialized = JsonConvert.SerializeObject(value);
                this.Value = JsonConvert.DeserializeObject(serialized, type.ToType());
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type.
        /// </summary>
        [DataMember]
        public AbstractType Type { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        //TO-DO from Opus 5.5 TypeNameHandling.All on object payload : An `object` property deserialized with TypeNameHandling.All (also ItemTypeNameHandling.All on ConditionBaseDefinition and related types) lets a crafted $type instantiate arbitrary gadget types, a known Newtonsoft RCE vector. Restrict it with a SerializationBinder allow-list, or use the existing System.Text.Json polymorphic discriminators.
        [Newtonsoft.Json.JsonProperty(TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All)]
        [DataMember]
        public object? Value { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionValueDefinition x, ConditionValueDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionValueDefinition x, ConditionValueDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            return other is ConditionValueDefinition val &&
                   this.Type.Equals(val.Type) &&
                   GetComparer().Equals(this.Value, val.Value);
        }

        /// <summary>
        /// Gets the comparer lazily (field is not set when deserializers skip the constructor)
        /// </summary>
        private IEqualityComparer GetComparer()
        {
            var comparer = this._comparer;
            if (comparer is null)
            {
                // Generic (non resolved) types cannot be converted to a runtime Type, fallback on the object comparer
                comparer = this.Type is ConcretBaseType
                                ? s_comparerCache.GetOrAdd(this.Type.ToType(), static t => BuildComparer(t))
                                : EqualityComparer<object>.Default;
                this._comparer = comparer;
            }

            return comparer;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.Type, this.Value);
        }

        /// <summary>
        /// Builds the comparer.
        /// </summary>
        private static IEqualityComparer BuildComparer(Type type)
        {
            return (IEqualityComparer)(typeof(EqualityComparer<>).MakeGenericType(type)
                                                                 .GetProperty(nameof(EqualityComparer<int>.Default), BindingFlags.Public | BindingFlags.Static)!
                                                                 .GetValue(null))!;
        }

        #endregion
    }
}
