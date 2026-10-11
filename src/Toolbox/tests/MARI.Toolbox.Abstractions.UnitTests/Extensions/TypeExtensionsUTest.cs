// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.UnitTests.Extensions
{
    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;

    using Xunit;

    /// <summary>
    /// Test for type extensions
    /// </summary>
    public sealed class TypeExtensionsUTest
    {
        #region Test

        private TResult MapFrom<TArg, TResult>(Tuple<int, TArg> p, IReadOnlyDictionary<string, Tuple<double, TResult>> args)
        {
            throw new NotImplementedException();
        }

        private TArg MapMultiFrom<TArg>(TArg param1, TArg param2)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        [Fact]
        public void Method_Abstract_Partial()
        {
            Expression<Func<TypeExtensionsUTest, string>> expr = e => e.MapFrom<Uri, string>(null!, null!);
            var genMethod = ((MethodCallExpression)expr.Body).Method.GetGenericMethodDefinition();

            var abstractMthd = genMethod.GetAbstractMethod();

            Check.That(abstractMthd).IsNotNull();
            Check.That(abstractMthd.IsIncomplet).IsTrue();
            Check.That(abstractMthd.GenericArguments).IsNotNull().And.CountIs(2);

            var restoredMethod = abstractMthd.ToMethod(typeof(TypeExtensionsUTest));

            Check.That(restoredMethod).IsNotNull().And.IsEqualTo(genMethod);
        }

        [Fact]
        public void Method_Generic_Solved_By_Parameter()
        {
            Expression<Func<TypeExtensionsUTest, string>> expr = e => e.MapFrom<Uri, string>(null!, null!);
            var genMethod = ((MethodCallExpression)expr.Body).Method.GetGenericMethodDefinition();

            var implMethod = genMethod.MakeGenericMethodFromParameters(new[] { typeof(Tuple<int, Guid>), typeof(Dictionary<string, Tuple<double, ushort>>) });

            Check.That(implMethod).IsNotNull();
            Check.That(implMethod.IsGenericMethodDefinition).IsFalse();

            var genericArgs = implMethod.GetGenericArguments();
            Check.That(genericArgs).IsNotNull().And.CountIs(2);

            Check.That(genericArgs.First()).IsNotNull().And.IsEqualTo(typeof(Guid));
            Check.That(genericArgs.Last()).IsNotNull().And.IsEqualTo(typeof(ushort));
        }

        [Theory]
        [InlineData(typeof(List<Guid>), typeof(Guid[]), typeof(IReadOnlyCollection<Guid>))]
        [InlineData(typeof(List<Guid>), typeof(HashSet<Guid>), typeof(IReadOnlyCollection<Guid>))]
        [InlineData(typeof(string), typeof(ReadOnlyObservableCollection<char>), typeof(IEnumerable<char>))]
        public void Method_Generic_Solved_By_Parameter_Multiple_Influance(Type firstArg, Type secondArg, Type Common)
        {
            Expression<Func<TypeExtensionsUTest, string>> expr = e => e.MapMultiFrom<string>(null!, null!);
            var genMethod = ((MethodCallExpression)expr.Body).Method.GetGenericMethodDefinition();

            var implMethod = genMethod.MakeGenericMethodFromParameters(new[] { firstArg, secondArg });

            Check.That(implMethod).IsNotNull();
            Check.That(implMethod.IsGenericMethodDefinition).IsFalse();

            var genericArgs = implMethod.GetGenericArguments();
            Check.That(genericArgs).IsNotNull().And.CountIs(1);

            Check.That(genericArgs.First()).IsNotNull().And.IsEqualTo(Common);
        }

        #endregion
    }
}
