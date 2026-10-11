// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Extensions
{
    using MARI.Toolbox.Abstractions.Models;
    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.UnitTests.ToolKit.Tests;

    using NFluent;

    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Test for <see cref="ExpressionExtensions.CreateAccess"/>
    /// </summary>
    public sealed class CreateAccessExtensionUTest
    {
        #region Nested

        public record struct Container(string strId);

        public record class Level1(string StrId);

        public record class Root(Level1 Lvl);

        public record class EmptyContainer();

        #endregion

        #region Methods

        [Fact]
        public void CreateAccess_Direct()
        {
            var obj = Guid.NewGuid();

            var access = obj.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.ChainCall).IsNull();
            Check.That(access.MemberInit).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(Guid).GetAbstractType());
            Check.That(access.DirectObject).IsNotNull().And.IsInstanceOf<TypedArgument<Guid>>();

            Check.That(((TypedArgument<Guid>)access.DirectObject!).Value).IsEqualTo(obj);   
        }

        [Fact]
        public void CreateAccess_Direct_Serialization()
        {
            var obj = Guid.NewGuid();

            var access = obj.CreateAccess();

            SerializationTester.SerializeTester(access);

            Check.That(access).IsNotNull();
            Check.That(access.ChainCall).IsNull();
            Check.That(access.MemberInit).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(Guid).GetAbstractType());
            Check.That(access.DirectObject).IsNotNull().And.IsInstanceOf<TypedArgument<Guid>>();

            Check.That(((TypedArgument<Guid>)access.DirectObject!).Value).IsEqualTo(obj);
        }

        [Fact]
        public void CreateAccess_ChainCall()
        {
            var obj = Guid.NewGuid().ToString();
            Expression<Func<string, int>> func = (string o) => o.Length;

            var access = func.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.MemberInit).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(int).GetAbstractType());

            Check.That(access.ChainCall).IsNotNull().And.IsEqualTo("o." + nameof(string.Length));
        }

        [Fact]
        public void CreateAccess_ChainCall_Multiple()
        {
            Expression<Func<Root, string>> func = (Root o) => o.Lvl.StrId;

            var access = func.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.MemberInit).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(string).GetAbstractType());

            Check.That(access.ChainCall).IsNotNull().And.IsEqualTo("o." + nameof(Root.Lvl) + "." + nameof(Level1.StrId));
        }

        [Fact]
        public void CreateAccess_ChainCall_Multiple_Serialization()
        {
            Expression<Func<Root, string>> func = (Root o) => o.Lvl.StrId;

            var access = func.CreateAccess();

            SerializationTester.SerializeTester(access);

            Check.That(access).IsNotNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.MemberInit).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(string).GetAbstractType());

            Check.That(access.ChainCall).IsNotNull().And.IsEqualTo("o." + nameof(Root.Lvl) + "." + nameof(Level1.StrId));
        }

        [Fact]
        public void CreateAccess_ChainCall_Serialization()
        {
            var obj = Guid.NewGuid().ToString();
            Expression<Func<string, int>> func = (string o) => o.Length;

            var access = func.CreateAccess();

            SerializationTester.SerializeTester(access);

            Check.That(access).IsNotNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.MemberInit).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(int).GetAbstractType());

            Check.That(access.ChainCall).IsNotNull().And.IsEqualTo("o." + nameof(string.Length));
        }

        [Fact]
        public void CreateAccess_MemberInit()
        {
            var obj = Guid.NewGuid().ToString();
            Expression<Func<string, Container>> func = (string o) => new Container() { strId = obj };

            var access = func.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.ChainCall).IsNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(Container).GetAbstractType());

            Check.That(access.MemberInit).IsNotNull();
        }

        [Fact]
        public void CreateAccess_MemberInit_Serialization()
        {
            var obj = Guid.NewGuid().ToString();
            Expression<Func<string, Container>> func = (string o) => new Container() { strId = obj };

            var access = func.CreateAccess();
            
            SerializationTester.SerializeTester(access);

            Check.That(access).IsNotNull();
            Check.That(access.ChainCall).IsNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(Container).GetAbstractType());

            Check.That(access.MemberInit).IsNotNull();
        }


        [Fact]
        public void CreateAccess_MemberInit_WithoutInput()
        {
            Expression<Func<Container>> func = () => new Container() { strId = "obj" };

            var access = func.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.ChainCall).IsNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(Container).GetAbstractType());

            Check.That(access.MemberInit).IsNotNull();
        }

        [Fact]
        public void CreateAccess_MemberInit_WithoutInput_DirectNew()
        {
            Expression<Func<EmptyContainer>> func = () => new EmptyContainer();

            var access = func.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.ChainCall).IsNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(EmptyContainer).GetAbstractType());

            Check.That(access.MemberInit).IsNotNull();
        }

        [Fact]
        public void CreateAccess_Direct_WithResolution()
        {
            var obj = Guid.NewGuid();

            var access = obj.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.ChainCall).IsNull();
            Check.That(access.MemberInit).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(Guid).GetAbstractType());
            Check.That(access.DirectObject).IsNotNull().And.IsInstanceOf<TypedArgument<Guid>>();

            Check.That(((TypedArgument<Guid>)access.DirectObject!).Value).IsEqualTo(obj);

            var result = access.Resolve(null);

            Check.That(result).IsNotNull().And.IsEqualTo(obj);
        }

        [Fact]
        public void CreateAccess_ChainCall_WithResolution()
        {
            var obj = Guid.NewGuid().ToString();
            Expression<Func<string, int>> func = (string o) => o.Length;

            var access = func.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.MemberInit).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(int).GetAbstractType());

            Check.That(access.ChainCall).IsNotNull().And.IsEqualTo("o." + nameof(string.Length));

            var newObj = Guid.NewGuid().ToString().Substring(0, Random.Shared.Next(10, obj.Length));
            var result = access.Resolve(newObj);
            Check.That(result).IsNotNull().And.IsEqualTo(newObj.Length);
        }

        [Fact]
        public void CreateAccess_MemberInit_WithResolution()
        {
            var obj = Guid.NewGuid().ToString();
            Expression<Func<string, Container>> func = (string o) => new Container() { strId = obj };

            var access = func.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.ChainCall).IsNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(Container).GetAbstractType());

            Check.That(access.MemberInit).IsNotNull();

            var newObj = Guid.NewGuid().ToString().Substring(0, Random.Shared.Next(10, obj.Length));
            var result = access.Resolve(newObj);
            Check.That(result).IsNotNull().And.IsInstanceOf<Container>();

#pragma warning disable CS8605 // Unboxing a possibly null value.
            var inst = (Container)result;
#pragma warning restore CS8605 // Unboxing a possibly null value.

            Check.That(inst.strId).IsNotNull().And.IsEqualTo(obj);
        }

        [Fact]
        public void CreateAccess_MemberInit_WithoutInput_WithResolution()
        {
            Expression<Func<Container>> func = () => new Container() { strId = "obj" };

            var access = func.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.ChainCall).IsNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(Container).GetAbstractType());

            Check.That(access.MemberInit).IsNotNull();

            var newObj = Guid.NewGuid().ToString().Substring(0, Random.Shared.Next(10, 15));
            var result = access.Resolve(newObj); // Force an input to see if it cash
            Check.That(result).IsNotNull().And.IsInstanceOf<Container>();

#pragma warning disable CS8605 // Unboxing a possibly null value.
            var inst = (Container)result;
#pragma warning restore CS8605 // Unboxing a possibly null value.

            Check.That(inst.strId).IsNotNull().And.IsEqualTo("obj");
        }

        [Fact]
        public void CreateAccess_MemberInit_WithoutInput_DirectNew_WithResolution()
        {
            Expression<Func<EmptyContainer>> func = () => new EmptyContainer();

            var access = func.CreateAccess();

            Check.That(access).IsNotNull();
            Check.That(access.ChainCall).IsNull();
            Check.That(access.DirectObject).IsNull();
            Check.That(access.TargetType).IsNotNull().And.IsEqualTo(typeof(EmptyContainer).GetAbstractType());

            Check.That(access.MemberInit).IsNotNull();

            var newObj = Guid.NewGuid().ToString().Substring(0, Random.Shared.Next(10, 15));
            var result = access.Resolve(newObj); // Force an input to see if it cash
            Check.That(result).IsNotNull().And.IsInstanceOf<EmptyContainer>();
        }

        #endregion
    }
}
