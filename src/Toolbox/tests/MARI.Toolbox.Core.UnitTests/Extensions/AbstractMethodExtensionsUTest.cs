// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Extensions
{
    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="AbstractMethod"/>
    /// </summary>
    public sealed class AbstractMethodExtensionsUTest
    {
        #region Nested

        internal interface IContraintA { }

        internal class MthdClass
        {
            public MthdClass() { }

            public void MethodClassic() { }

            public void GenericMethodStruct<TContraints>(TContraints contraints)
                where TContraints : struct
            {
            }

            public void GenericMethod<TContraints>(TContraints contraints)
                where TContraints : IContraintA
            {
            }
        }

        #endregion

        #region Methods

        [Fact]
        public void AccessMethodDefinition()
        {
            var mthClass = typeof(MthdClass);

            foreach (var mthd in mthClass.GetMethods())
            {
                var def = mthd.GetAbstractMethod();

                Check.That(def).IsNotNull();
                Check.That(def.IsConstructor).IsFalse();
                Check.That(def.MethodName).IsEqualTo(mthd.Name);
            }
        }

        [Fact]
        public void AccessMethodDefinition_IsEqualTo()
        {
            var mthClass = typeof(MthdClass);

            var mthds = mthClass.GetMethods()
                                .Select(m => m.GetAbstractMethod())
                                .ToArray();

            foreach (var otherMthd in mthds)
            {
                foreach (var mthd in mthClass.GetMethods())
                {
                    var equals = otherMthd.MethodName == mthd.Name;
                    Check.ThatCode(() => otherMthd.IsEqualTo(mthd)).WhichResult().IsEqualTo(equals);
                }
            }
        }

        [Fact]
        public void AccessMethodDefinition_IsEqualTo_Spe()
        {
            var mthClass = typeof(MthdClass);

            var mthd = mthClass.GetMethods()
                               .Where(m => m.Name == nameof(MthdClass.GenericMethod))
                               .First()
                               .GetAbstractMethod();

            var spe = mthClass.GetMethod(nameof(MthdClass.GenericMethodStruct))!;

            Check.ThatCode(() => mthd.IsEqualTo(spe, true)).DoesNotThrow().And.WhichResult().IsFalse();
        }

        #endregion
    }
}
