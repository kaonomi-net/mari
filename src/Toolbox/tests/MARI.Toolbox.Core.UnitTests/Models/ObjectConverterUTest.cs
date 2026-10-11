// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Models
{
    using AutoFixture;
    using AutoFixture.Kernel;

    using MARI.Toolbox.Abstractions.Models;
    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Models;
    using MARI.Toolbox.Models.Converters;
    using MARI.Toolbox.UnitTests.ToolKit.Xunits;

    using NFluent;

    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Test for <see cref="ObjectConverter"/>
    /// </summary>
    public sealed class ObjectConverterUTest
    {
        #region Nested

        internal abstract record class Parent(string Id);

        internal sealed record class Child(string Id, string ChildId) : Parent(Id);

        internal sealed record class Source<TArg>(TArg item) : ISupportConvert
        {
            /// <inheritdoc />
            public bool TryConvert<TTarget>(out TTarget? target)
            {
                if (TryConvert(out var objTarget, typeof(TTarget)))
                {
                    target = (TTarget)objTarget;
                    return true;
                }

                target = default;
                return false;
            }

            /// <inheritdoc />
            public bool TryConvert(out object target, Type targetType)
            {
                var genericLocal = GetType().GetGenericTypeDefinition();
                if (targetType.IsGenericType &&
                    targetType.GetGenericTypeDefinition() == genericLocal &&
                    typeof(TArg).IsAssignableTo(targetType.GetGenericArguments().Single()))
                {
                    target = Activator.CreateInstance(targetType, new object?[] { this.item })!;
                    return true;
                }

                target = null;
                return false;
            }
        }

        #endregion

        #region Methods

        [Fact]
        public void Support_Convert()
        {
            var fixture = new Fixture();
            var item = new Source<Child>(new Child(fixture.Create<string>(), fixture.Create<string>()));

            var objConverter = new ObjectConverter(EnumerableHelper<IDedicatedObjectConverter>.Enumerable);

            var cnvGeneric = objConverter.TryConvert<Source<Parent>>(item, out var resultWithGeneric);

            Check.That(cnvGeneric).IsTrue();
            Check.That(resultWithGeneric).IsNotNull();
            Check.That(resultWithGeneric!.item).IsSameReferenceAs(item.item);

            var cnvDirect = objConverter.TryConvert(item, typeof(Source<Parent>), out var resultWithDirect);

            Check.That(cnvDirect).IsTrue();
            Check.That(resultWithDirect).IsNotNull().And.IsInstanceOf<Source<Parent>>();
            Check.That(((Source<Parent>)resultWithDirect!).item).IsSameReferenceAs(item.item);
        }

        [Theory]
        [MemberData(nameof(GetConvertTypes))]
        public void Scalar_Convert(Type _, Type destination, object value, object expected)
        {
            using (var obj = new ObjectConverter(new[] { new ScalarDedicatedConverter() }))
            {
                var cnvt = obj.TryConvert(value, destination, out var result);

                Check.That(cnvt).IsTrue();
                Check.That(result).IsEqualTo(expected);
            }
        }

        [Fact]
        public void Guid_Convert()
        {
            using (var obj = new ObjectConverter(new[] { new GuidDedicatedConverter() }))
            {
                var id = Guid.NewGuid();

                var cnvt = obj.TryConvert(id, typeof(string), out var result);

                Check.That(cnvt).IsTrue();
                Check.That(result).IsInstanceOf<string>().And.IsEqualTo(id.ToString());

                var cnvtBack = obj.TryConvert(result, typeof(Guid), out var resultBack);

                Check.That(cnvt).IsTrue();
                Check.That(resultBack).IsInstanceOf<Guid>().And.IsEqualTo(id);
            }
        }

        #region Tools

        public static IEnumerable<object[]> GetConvertTypes()
        {
            var fixture = new Fixture();
            var convertMethod = typeof(Convert).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                               .Where(m => m.ReturnType != typeof(object) &&
                                                           CSharpTypeInfo.ScalarTypes.Contains(m.ReturnType) &&
                                                           m.GetParameters().Length == 1 &&
                                                           m.Name == "To" + m.ReturnType.Name &&
                                                           m.GetParameters().First().ParameterType != typeof(object) &&
                                                           CSharpTypeInfo.ScalarTypes.Contains(m.GetParameters().First().ParameterType))
                                               .GroupBy(m => (Param: m.GetParameters().First(), Return: m.ReturnType))
                                               .ToFrozenDictionary(k => Tuple.Create(k.Key.Param.ParameterType, k.Key.Return), k => k.OrderByDescending(m => m.Name.Length).First());

            return convertMethod.Select(cnv => GenerateValidTestValues(cnv.Key.Item1, cnv.Key.Item2, fixture))
                                .NotNull()
                                .ToArray();
        }

        private static object[]? GenerateValidTestValues(Type source, Type target, Fixture fixture)
        {
            var data = fixture.Create(target, new SpecimenContext(fixture));

            if (source == target)
                return null;

            try
            {
                if (source == typeof(DateTime))
                {
                    data = fixture.Create<short>();
                    data = Convert.ChangeType(data, target);
                }

                if (source == typeof(bool))
                {
                    data = fixture.Create<bool>() ? 1 : 1;
                }

                object? convertibleData = null;
                try
                {
                    convertibleData = Convert.ChangeType(data, source);
                }
                catch (InvalidCastException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    var reverse = GenerateValidTestValues(target, source, fixture);
                    data = reverse[2];
                    convertibleData = reverse[3];
                }

                return new object[] { source, target, convertibleData, data };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #endregion
    }
}
