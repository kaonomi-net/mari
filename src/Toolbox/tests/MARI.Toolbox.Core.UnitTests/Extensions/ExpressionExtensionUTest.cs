// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.UnitTests.Extensions
{
    using AutoFixture;
    using AutoFixture.Kernel;

    using MARI.Toolbox.Abstractions.Conditions;
    using MARI.Toolbox.Abstractions.Enums;
    using MARI.Toolbox.Abstractions.Expressions;
    using MARI.Toolbox.Models;
    using MARI.Toolbox.UnitTests.ToolKit.Helpers;
    using MARI.Toolbox.UnitTests.Xunits;

    using Microsoft.VisualStudio.TestPlatform.Utilities;

    using Newtonsoft.Json;

    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.Serialization;

    /// <summary>
    /// Test extension around <see cref="Expression"/>
    /// </summary>
    public sealed class ExpressionExtensionUTest
    {
        #region Fields

        private const string SAMPLE_VALUE = "SAMPLE_VALUE";

        private static readonly JsonSerializerSettings s_serializationSetting;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ExpressionExtensionUTest"/> class.
        /// </summary>
        static ExpressionExtensionUTest()
        {
            SampleValueStaticProp = SAMPLE_VALUE;

            s_serializationSetting = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };
        }

        #endregion

        #region Nested

        public record class ParentClass();
        public sealed record class ChildClassA(string Name) : ParentClass();
        public sealed record class ChildClassB(string Name) : ParentClass();

        public record class HierachyRootClass(ParentClass? Child);

        private static string SampleValueStaticProp { get; }

        public record class SimpleObj();

        public record class SimpleObjWithString(string arg, string b);

        public class ComplexObj : IEquatable<ComplexObj>
        {
            public ComplexObj(Guid uid)
            {
                this.Uid = uid;
            }

            public Guid Uid { get; }

            public string Name { get; set; }

            public bool Equals(ComplexObj? other)
            {
                if (other is null)
                    return false;

                if (object.ReferenceEquals(other, this))
                    return true;

                return this.Uid == other.Uid &&
                       this.Name == other.Name;
            }

            public override bool Equals(object? obj)
            {
                if (obj is ComplexObj complex)
                    return Equals(complex);
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.Uid, this.Name);
            }
        }

        public class ContainerComplex
        {
            private readonly Dictionary<Guid, ComplexObj> _complexObjs;

            public ContainerComplex(IEnumerable<ComplexObj> complexObjs)
            {
                this._complexObjs = complexObjs.GroupBy(c => c.Uid)
                                               .ToDictionary(k => k.Key, v => v.First());
            }

            public string Name { get; set; }

            public ComplexObj this[Guid uid]
            {
                get { return this._complexObjs[uid]; }
            }
        }

        public class Root
        {
            public object Complex { get; set; }
        }

        public record struct ExpressioSimpleSampleObject(Guid uid, string name, bool value, StringComparison? StringComparison);
        public record struct ExpressionComplexSampleObject(Guid uid, string name, bool value, ExpressioSimpleSampleObject nested);

        #endregion

        #region Methods

        /// <summary>
        /// Test <see cref="ExpressionExtensions.Serialize{TInput}(this Expression{Func{TInput, bool}} expression)"/>
        /// one condition simple
        /// </summary>
        [Fact]
        public void ConditionSerialization_Simple()
        {
            var sucessStr = "sucess";
            var failStr = "fail";

            ConditionSerializationTest((string? item) => item == "sucess", new[] { sucessStr }, new[] { failStr, string.Empty, null });
            ConditionSerializationTest((string? item) => item == sucessStr, new[] { sucessStr }, new[] { failStr, string.Empty, null });
            ConditionSerializationTest((string? item) => !string.IsNullOrEmpty(item), new[] { sucessStr, failStr }, new[] { string.Empty, null });
        }

        /// <summary>
        /// Test <see cref="ExpressionExtensions.Serialize{TInput}(this Expression{Func{TInput, bool}} expression)"/>
        /// one condition simple
        /// </summary>
        [Fact]
        public void ConditionSerialization_Cast()
        {
            var sucessStr = "sucess";
            var failStr = "fail";

            ConditionSerializationTest((ParentClass? item) => item as ParentClass != null &&
                                                              ((ChildClassA)item).Name == "sucess",

                                       new ParentClass[] { new ChildClassA(sucessStr) },
                                       new ParentClass[] { new ChildClassA(failStr), new ChildClassB(string.Empty) });

            ConditionSerializationTest((HierachyRootClass? item) => item != null &&
                                                                    item.Child as ParentClass != null &&
                                                                    ((ChildClassA)item.Child).Name == "sucess",

                                       new[] { new HierachyRootClass(new ChildClassA(sucessStr)) },
                                       new[] { new HierachyRootClass(new ChildClassA(failStr)), new HierachyRootClass(null) });
        }

        /// <summary>
        /// Test <see cref="ExpressionExtensions.Serialize{TInput}(this Expression{Func{TInput, bool}} expression)"/>
        /// multiple simple condition and
        /// </summary>
        [Fact]
        public void ConditionSerialization_Simple_And()
        {
            var sucessStr = "succeed";
            var failStartStr = "success";
            var failEndStr = "feed";

            ConditionSerializationTest((string? item) => item != null && item.StartsWith("s") && item.EndsWith("d"), new[] { sucessStr }, new[] { failStartStr, failEndStr, string.Empty, null });
            ConditionSerializationTest((string? item) => item != null && (item.StartsWith("s") || item.EndsWith("d")), new[] { sucessStr, failStartStr, failEndStr }, new[] { string.Empty, null });
        }

        /// <summary>
        /// one condition simple that contains math operator
        /// </summary>
        [Theory]
        [EnumData<MathOperatorEnum>()]
        public void ConditionSerialization_Simple_MathOperator(MathOperatorEnum mathOperator)
        {
            Expression<Func<int, bool>> expression;

            switch (mathOperator)
            {
                case MathOperatorEnum.Sum:
                    expression = (i) => i + 2 < 10;
                    break;

                case MathOperatorEnum.Sub:
                    expression = (i) => i - 2 < 10;
                    break;

                case MathOperatorEnum.Multiply:
                    expression = (i) => i * 2 < 10;
                    break;

                case MathOperatorEnum.Modulo:
                    expression = (i) => i % 2 == 0;
                    break;

                case MathOperatorEnum.Divide:
                    expression = (i) => i / 2 < 10;
                    break;

                case MathOperatorEnum.None:
                    // All
#pragma warning disable IDE0047 // Remove unnecessary parentheses
                    expression = (i) => (((i + 2 - 4) * 2) / 4) % 2 == 0;
#pragma warning restore IDE0047 // Remove unnecessary parentheses
                    break;

                default:
                    throw new NotSupportedException("Math operator not managed " + mathOperator);
            }

            var func = expression.Compile();

            var results = Enumerable.Range(0, 1000)
                                    .GroupBy(i => func(i))
                                    .ToDictionary(k => k.Key, v => v.ToList());

            Check.That(results).CountIs(2);
            Check.That(results[true]).Not.IsEmpty();
            Check.That(results[false]).Not.IsEmpty();

            ConditionSerializationTest<int>(expression, results[true], results[false]);
        }

        [Fact]
        public void Conditional_With_Index_Access()
        {
            var id = Guid.NewGuid();

            var containers = new ContainerComplex[]
            {
                new ContainerComplex(new []
                {
                    new ComplexObj(id) { Name = "Success" }
                }),

                new ContainerComplex(new []
                {
                    new ComplexObj(id) { Name = "Failed" }
                })
            };

            Expression<Func<ContainerComplex, bool>> filter = c => c[id].Name.StartsWith("S");

            ConditionSerializationTest<ContainerComplex>(filter, containers.Take(0).ToArray(), containers.Skip(1).ToArray());
        }

        [Fact]
        public void Conditional_With_Index_Convert_Access()
        {
            var id = Guid.NewGuid();

            var containers = new Root[]
            {
                new Root()
                {
                    Complex = new ContainerComplex(new []
                    {
                        new ComplexObj(id) { Name = "Success" }
                    }),
                },

                new Root()
                {
                    Complex = new ContainerComplex(new []
                    {
                        new ComplexObj(id) { Name = "Failed" }
                    })
                }
            };

            Expression<Func<Root, bool>> filter = r => (((ContainerComplex)r.Complex)[id]).Name == "Success";

            ConditionSerializationTest<Root>(filter, containers.Take(0).ToArray(), containers.Skip(1).ToArray());
        }

        /// <summary>
        /// Conditions the serialization each serialize part.
        /// </summary>
        [Theory]
        [InlineData(typeof(ConditionParameterDefinition))]
        [InlineData(typeof(ConditionCallDefinition))]
        [InlineData(typeof(ConditionExpressionDefinition))]
        [InlineData(typeof(ConditionGroupDefinition))]
        [InlineData(typeof(ConditionMathOperationDefinition))]
        [InlineData(typeof(ConditionMemberAccessDefinition))]
        [InlineData(typeof(ConditionOperandDefinition))]
        [InlineData(typeof(ConditionValueDefinition))]

        //[InlineData(typeof(MemberBindingDefinition))]
        [InlineData(typeof(MemberInputConstantBindingDefinition<string>))]
        [InlineData(typeof(MemberInputConstantBindingDefinition<bool>))]

        [InlineData(typeof(MemberInputParameterBindingDefinition))]
        [InlineData(typeof(MemberInputCallChainBindingDefinition))]
        [InlineData(typeof(MemberInputNestedInitBindingDefinition))]
        [InlineData(typeof(MemberInputAccessBindingDefinition))]

        public void Expression_serialization_part(Type partType)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            // TODO: Allow more type combinaison to improve test efficiency
            fixture.Register<AbstractType>(() => typeof(int).GetAbstractType());
            fixture.Register<ConcretType>(() => (ConcretType)typeof(int).GetAbstractType());
            fixture.Register<ConcretBaseType>(() => (ConcretBaseType)typeof(int).GetAbstractType());
            fixture.Register<ConditionValueDefinition>(() => new ConditionValueDefinition(typeof(int).GetAbstractType(), Random.Shared.Next(0, 152)));
            fixture.Register<ConditionBaseDefinition>(() => fixture.Create<ConditionValueDefinition>());
            fixture.Register<MemberBindingDefinition>(() => new MemberInputConstantBindingDefinition<string>(false, SAMPLE_VALUE, SAMPLE_VALUE));

            var inst = fixture.Create(partType, new SpecimenContext(fixture));
            Check.That(inst).IsNotNull();

            var serializationJson = JsonConvert.SerializeObject(inst, s_serializationSetting);

            Check.That(serializationJson).IsNotNull();

            var newInst = JsonConvert.DeserializeObject(serializationJson, partType, s_serializationSetting);

            Check.That(newInst).IsNotNull().And.IsInstanceOfType(partType);
            Check.That(newInst).IsEqualTo(inst);
        }

        [Fact]
        public void MemberInit_Complex_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();
            var uidValue = Guid.NewGuid();

            Expression<Func<string, ComplexObj>> expr = input => new ComplexObj(uidValue)
            {
                Name = nameValue
            };

            MemberInit_Tester(expr, "Poner Rose");
        }

        /// <summary>
        /// Members the initialize constant binding.
        /// </summary>
        [Fact]
        public void MemberInit_Constant_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();
            var uidValue = Guid.NewGuid();

            Expression<Func<string, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                uid = uidValue,
                value = true,
                name = nameValue
            };
            MemberInit_Tester(expr, "Poner Rose");
        }

        /// <summary>
        /// Members the initialize constant binding.
        /// </summary>
        [Fact]
        public void MemberInit_Constant_Binding_Using_ConstValue()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();
            var uidValue = Guid.NewGuid();

            Expression<Func<string, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                uid = uidValue,
                value = true,
                name = SAMPLE_VALUE
            };
            MemberInit_Tester(expr, "Poner Rose");
        }

        [Fact]
        public void MemberInit_Ctor_Class_Binding_Using_ConstValue()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();
            var uidValue = Guid.NewGuid();

            Expression<Func<string, SimpleObjWithString>> expr = input => new SimpleObjWithString(SAMPLE_VALUE, input);
            MemberInit_Tester(expr, "Poner Rose");
        }

        [Fact]
        public void MemberInit_Ctor_Class_Binding_Using_StaticReadOnlyProp()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();
            var uidValue = Guid.NewGuid();

            Expression<Func<string, SimpleObjWithString>> expr = input => new SimpleObjWithString(SampleValueStaticProp, input);
            MemberInit_Tester(expr, "Poner Rose");
        }

        /// <summary>
        /// Members the initialize lambda new.
        /// </summary>
        [Fact]
        public void MemberInit_New_Lambda_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            Expression<Func<SimpleObj>> expr = () => new SimpleObj();

            var bindingModel = expr.SerializeMemberInitialization();
            Check.That(bindingModel).IsNotNull();

            var serializationJson = JsonConvert.SerializeObject(bindingModel, s_serializationSetting);
            Check.That(serializationJson).IsNotNull();

            var newBindingModel = (MemberInitializationDefinition)JsonConvert.DeserializeObject(serializationJson, bindingModel.GetType(), s_serializationSetting)!;
            Check.That(serializationJson).IsNotNull();

            var newInitExpr = newBindingModel.ToMemberInitializationExpression<SimpleObj>();
            Check.That(newInitExpr).IsNotNull();

            var sourceGenerated = expr.Compile().Invoke();
            var generationAfter = newInitExpr.Compile().Invoke();

            Check.That(sourceGenerated).IsEqualTo(generationAfter);
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Constant_And_Input_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();
            var uidValue = Guid.NewGuid();

            Expression<Func<string, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                uid = uidValue,
                value = true,
                name = input
            };

            MemberInit_Tester(expr, "Poner Rose");
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Constant_And_InputChainCall_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            Expression<Func<ExpressioSimpleSampleObject, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                uid = input.uid,
                value = true,
                name = nameValue
            };

            MemberInit_Tester(expr, new ExpressioSimpleSampleObject() { uid = Guid.NewGuid() });
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Constant_And_Extern_ChainCall_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            var externInput = new ExpressioSimpleSampleObject()
            {
                uid = Guid.NewGuid(),
            };

            Expression<Func<string, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                uid = externInput.uid,
                value = true,
                name = nameValue
            };

            MemberInit_Tester(expr, "Poeny Rose");
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Convert_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            Expression<Func<string, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                value = true,
                StringComparison = StringComparison.OrdinalIgnoreCase
            };

            MemberInit_Tester(expr, "Poeny Rose");
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Nested_Param_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            Expression<Func<ExpressioSimpleSampleObject, ExpressionComplexSampleObject>> expr = input => new ExpressionComplexSampleObject()
            {
                uid = input.uid,
                value = true,
                name = nameValue,
                nested = input
            };

            MemberInit_Tester(expr, new ExpressioSimpleSampleObject() { uid = Guid.NewGuid() });
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Nested_Build_Binding_Full()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            var externInput = new ExpressioSimpleSampleObject()
            {
                uid = Guid.NewGuid(),
            };

            Expression<Func<ExpressioSimpleSampleObject, ExpressionComplexSampleObject>> expr = input => new ExpressionComplexSampleObject()
            {
                uid = input.uid,
                value = true,
                name = nameValue,
                nested = new ExpressioSimpleSampleObject()
                {
                    uid = externInput.uid,
                    value = true,
                    name = "Poney rose"
                }
            };

            MemberInit_Tester(expr, new ExpressioSimpleSampleObject() { uid = Guid.NewGuid() });
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Nested_Build_Binding_Full_With_Ctor()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            var externInput = new ExpressioSimpleSampleObject()
            {
                uid = Guid.NewGuid(),
            };

            Expression<Func<ExpressioSimpleSampleObject, ExpressionComplexSampleObject>> expr = input => new ExpressionComplexSampleObject(input.uid,
                                                                                                                                           nameValue,
                                                                                                                                           true,
                                                                                                                                           new ExpressioSimpleSampleObject()
                                                                                                                                           {
                                                                                                                                               uid = externInput.uid,
                                                                                                                                               value = true,
                                                                                                                                               name = "Poney rose"
                                                                                                                                           });

            MemberInit_Tester(expr, new ExpressioSimpleSampleObject() { uid = Guid.NewGuid() });
        }

        [Fact]
        public void Replace_Paramerter()
        {
            Expression<Func<ParentClass, bool>> source = p => p is ChildClassA && string.IsNullOrEmpty(((ChildClassA)p).Name);

            var origin = source.Compile();
            Check.ThatCode(() => origin(new ChildClassA(string.Empty))).WhichResult().IsTrue();
            Check.ThatCode(() => origin(new ChildClassA("test"))).WhichResult().IsFalse();
            Check.ThatCode(() => origin(new ChildClassB(string.Empty))).WhichResult().IsFalse();

            var reroot = source.ReplaceParameter<ParentClass, HierachyRootClass, bool>(h => h.Child!);

            var rerootFunc = reroot.Compile();
            Check.ThatCode(() => rerootFunc(new HierachyRootClass(new ChildClassA(string.Empty)))).WhichResult().IsTrue();
            Check.ThatCode(() => rerootFunc(new HierachyRootClass(new ChildClassA("test")))).WhichResult().IsFalse();
            Check.ThatCode(() => rerootFunc(new HierachyRootClass(new ChildClassB(string.Empty)))).WhichResult().IsFalse();
        }

        #region Tools

        /// <summary>
        /// Members the initialize constant binding.
        /// </summary>
        private void MemberInit_Tester<TInput, TOutput>(Expression<Func<TInput, TOutput>> expr, TInput test)
        {
            var bindingModel = expr.SerializeMemberInitialization();
            Check.That(bindingModel).IsNotNull();

            var serializationJson = JsonConvert.SerializeObject(bindingModel, s_serializationSetting);
            Check.That(serializationJson).IsNotNull();

            var newBindingModel = (MemberInitializationDefinition)JsonConvert.DeserializeObject(serializationJson, bindingModel.GetType(), s_serializationSetting)!;
            Check.That(serializationJson).IsNotNull();

            var newInitExpr = newBindingModel.ToMemberInitializationExpression<TInput, TOutput>();
            Check.That(newInitExpr).IsNotNull();

            var sourceGenerated = expr.Compile().Invoke(test);
            var generationAfter = newInitExpr.Compile().Invoke(test);

            Check.That(sourceGenerated).IsEqualTo(generationAfter);
        }

        /// <summary>
        /// Generic condition serialization tester
        /// </summary>
        private void ConditionSerializationTest<TItem>(Expression<Func<TItem, bool>> condition,
                                                       IReadOnlyCollection<TItem> working,
                                                       IReadOnlyCollection<TItem> failing)
        {
            var serializeCond = condition.Serialize();
            Check.That(serializeCond).IsNotNull();

            var jsonSerializeCond = JsonConvert.SerializeObject(serializeCond, s_serializationSetting);
            Check.That(jsonSerializeCond).Not.IsNullOrEmpty();

            var deseralizedCond = JsonConvert.DeserializeObject<ConditionExpressionDefinition>(jsonSerializeCond, s_serializationSetting);
            Check.That(deseralizedCond).IsNotNull();

            Check.That(serializeCond).IsEqualTo(deseralizedCond);

            var originFuncCond = condition.Compile();
            var deserializedExpressionCond = deseralizedCond?.ToExpression<TItem, bool>();

            var deserializedFuncCond = deserializedExpressionCond?.Compile();

            Check.That(originFuncCond).IsNotNull();
            Check.That(deserializedFuncCond).IsNotNull();
            Check.That(deserializedExpressionCond).IsNotNull();

            foreach (var work in working)
            {
                Check.ThatCode(() => originFuncCond(work)).WhichResult().IsTrue();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                Check.ThatCode(() => deserializedFuncCond(work)).WhichResult().IsTrue();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

            foreach (var fail in failing)
            {
                Check.ThatCode(() => originFuncCond(fail)).WhichResult().IsFalse();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                Check.ThatCode(() => deserializedFuncCond(fail)).WhichResult().IsFalse();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }

        #endregion

        #endregion
    }
}
