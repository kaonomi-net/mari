// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.UnitTests.Extensions
{
    using MARI.Toolbox.Models;
    using MARI.Toolbox.UnitTests.ToolKit.Xunits;

    using Newtonsoft.Json;

    using NFluent;

    using System;
    using System.Reflection;

    /// <summary>
    /// Test for <see cref="AbstractTypeExtensions"/>
    /// </summary>
    public class AbstractTypeExtensionsUTest
    {
        #region Methods

        /// <summary>
        /// Test <see cref="AbstractTypeExtensions.GetAbstractType"/>
        /// </summary>
        [Theory]
        [InlineData(typeof(int), "Int32", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(Task), "Task", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(ValueTask), "ValueTask", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(string), "String", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(Task<double>), "Task<Double>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(ValueTask<double>), "ValueTask<Double>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(ValueTask<Task<double>>), "ValueTask<Task<Double>>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(IConvertible), "IConvertible", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(ICheck<int>), "ICheck<Int32>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(Tuple<>), "Tuple<>", AbstractTypeCategoryEnum.Incomplet)]
        [InlineData(typeof(Tuple<string>), "Tuple<String>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(Tuple<string, int>), "Tuple<String, Int32>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(List<>), "List<>", AbstractTypeCategoryEnum.Incomplet)]
        [InlineData(typeof(IReadOnlyList<int>), "IReadOnlyList<Int32>", AbstractTypeCategoryEnum.Collection)]
        [InlineData(typeof(List<int>), "List<Int32>", AbstractTypeCategoryEnum.Collection)]
        [InlineData(typeof((string, int)), "ValueTuple<String, Int32>", AbstractTypeCategoryEnum.Concret)]

        public void AbstractType_TypeBuild(Type testType, string expectedDisplayName, AbstractTypeCategoryEnum categoryEnum)
        {
            var abstractType = testType.GetAbstractType();

            Check.That(abstractType).IsNotNull();
            Check.That(abstractType.DisplayName).IsEqualTo(expectedDisplayName);
            Check.That(abstractType.Category).IsEqualTo(categoryEnum);

            Check.ThatCode(() => abstractType.Equals(testType)).DoesNotThrow().And.WhichResult().IsTrue();

            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented
            };

            var typeJson = JsonConvert.SerializeObject(abstractType, settings);

            Check.That(typeJson).Not.IsNullOrEmpty();

            var deserializedAbstractType = JsonConvert.DeserializeObject(typeJson, settings);

            Check.That(deserializedAbstractType).IsNotNull()
                                                .And
                                                .Not.IsSameReferenceAs(abstractType)
                                                .And
                                                .IsEqualTo(abstractType);

            Check.ThatCode(() => abstractType.Equals(testType)).DoesNotThrow().And.WhichResult().IsTrue();
        }

        /// <summary>
        /// Test simple method of string
        /// </summary>
        [Theory]
        [GetMethodsData(types: new Type[]
        {
            typeof(string),
            typeof(IConvertible),
            typeof(ICollection<double>),
            typeof(List<double>),
            typeof(List<>),
            typeof(Dictionary<,>),
        })]
        // Take Type and string to have XUnit serializable information to simplify the test usage
        public void AbstractType_All_Method_Of_Type(string displayName, Type typeHost, string methodGenId)
        {
            var method = typeHost.GetAllMethodInfos(BindingFlags.Instance | BindingFlags.Public)
                                 .Select(m => (Abs: m.GetAbstractMethod(useCache: false), Mth: m))
                                 .FirstOrDefault(v => v.Abs.MethodUniqueId == methodGenId);// || v.Abs.DisplayName == displayName);

            Check.WithCustomMessage(displayName + " Not Founded").That(method.Mth).IsNotNull();

            Check.That(method.Abs.DisplayName).IsEqualTo(displayName);
            Check.That(method.Abs.MethodUniqueId).IsEqualTo(methodGenId);

            AbstractTypeMethod(method.Mth!, displayName);
        }

        [Fact]
        public void AbstractMethod_Generic()
        {
            var type = typeof(List<>);
            var genericMethod = type.GetMethod("Add");

            Check.That(genericMethod).IsNotNull();

            var abstrGenericMethod = genericMethod!.GetAbstractMethod();

            var mtdh = abstrGenericMethod.ToMethod(type);
            Check.That(mtdh).IsNotNull().And.IsEqualTo(abstrGenericMethod);
        }

        /// <summary>
        /// Test to convert back from <see cref="AbstractType"/> to real <see cref="Type"/>
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(Task))]
        [InlineData(typeof(ValueTask))]
        [InlineData(typeof(string))]
        [InlineData(typeof(Task<double>))]
        [InlineData(typeof(ValueTask<double>))]
        [InlineData(typeof(ValueTask<Task<double>>))]
        [InlineData(typeof(IConvertible))]
        [InlineData(typeof(ICheck<int>))]
        [InlineData(typeof(Tuple<>))]
        [InlineData(typeof(Tuple<,>))]
        [InlineData(typeof(Tuple<string>))]
        [InlineData(typeof(Tuple<string, int>))]
        [InlineData(typeof(List<>))]
        [InlineData(typeof(IReadOnlyList<int>))]
        [InlineData(typeof(List<int>))]
        [InlineData(typeof((string, int)))]
        public void AbstractType_ToType(Type type)
        {
            var abstrtype = type.GetAbstractType();

            Check.That(abstrtype).IsNotNull();

            var rollbackType = abstrtype.ToType();
            Check.That(rollbackType).IsNotNull().And.IsEqualTo(type);
        }

        #region Methods

        /// <summary>
        /// Test <see cref="AbstractTypeExtensions.GetAbstractType(Type)"/>
        /// </summary>
        private void AbstractTypeMethod(MethodInfo methodInfo, string expectedDisplayName)
        {
            var abstractMethod = methodInfo.GetAbstractMethod();

            Check.That(abstractMethod).IsNotNull();
            Check.That(abstractMethod.DisplayName).IsEqualTo(expectedDisplayName);

            Check.ThatCode(() => abstractMethod.Equals(methodInfo)).DoesNotThrow().And.WhichResult().IsTrue();

            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented
            };

            var typeJson = JsonConvert.SerializeObject(abstractMethod, settings);

            Check.That(typeJson).Not.IsNullOrEmpty();

            var deserializedAbstractType = JsonConvert.DeserializeObject(typeJson, settings);

            Check.That(deserializedAbstractType).IsNotNull()
                                                .And
                                                .Not.IsSameReferenceAs(abstractMethod)
                                                .And
                                                .IsEqualTo(abstractMethod);

            Check.ThatCode(() => abstractMethod.Equals(methodInfo)).DoesNotThrow().And.WhichResult().IsTrue();
        }

        #endregion

        #endregion
    }
}
