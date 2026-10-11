// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace System.Linq.Expressions
{
    using MARI.Toolbox.Abstractions.Conditions;
    using MARI.Toolbox.Abstractions.Enums;
    using MARI.Toolbox.Abstractions.Expressions;
    using MARI.Toolbox.Abstractions.Models;
    using MARI.Toolbox.Helpers;
    using MARI.Toolbox.Models;

    using System;
    using System.Collections.Frozen;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extension around expression
    /// </summary>
    public static class ExpressionExtensions
    {
        #region Fields

        private static readonly IReadOnlyDictionary<LogicEnum, Func<Expression, Expression, Expression>> s_logicalExpressionBuild;
        private static readonly IReadOnlyDictionary<OperandEnum, Func<Expression?, Expression, Expression>> s_operandExpressionBuild;
        private static readonly IReadOnlyDictionary<MathOperatorEnum, Func<Expression?, Expression, Expression>> s_mathOperatorExpressionBuild;
        private static readonly IReadOnlyDictionary<int, Type> s_functionTemplateTypes;
        private static readonly IReadOnlyDictionary<int, Type> s_actionTemplateTypes;

        private static readonly Type s_memberInputConstantBindingDefinition = typeof(MemberInputConstantBindingDefinition<>);

        private static readonly IDictionary<ConditionExpressionDefinition, LambdaExpression> s_conditionalExperssionDefCache;
        private static readonly ReaderWriterLockSlim s_conditionalExperssionDefCacheLocker;

        private static readonly IDictionary<MemberInitializationDefinition, LambdaExpression> s_memberInitExperssionDefCache;
        private static readonly ReaderWriterLockSlim s_memberInitExperssionDefCacheLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize class <see cref="ExpressionExtensions"/>
        /// </summary>
        static ExpressionExtensions()
        {
            s_conditionalExperssionDefCacheLocker = new ReaderWriterLockSlim();
            s_conditionalExperssionDefCache = new Dictionary<ConditionExpressionDefinition, LambdaExpression>();

            s_memberInitExperssionDefCache = new Dictionary<MemberInitializationDefinition, LambdaExpression>();
            s_memberInitExperssionDefCacheLocker = new ReaderWriterLockSlim();

            s_logicalExpressionBuild = new Dictionary<LogicEnum, Func<Expression, Expression, Expression>>()
            {
                [LogicEnum.Or] = (left, right) => Expression.OrElse(left, right),
                [LogicEnum.And] = (left, right) => Expression.AndAlso(left, right),
                [LogicEnum.ExclusiveOr] = (left, right) => Expression.ExclusiveOr(left, right),
            }.ToFrozenDictionary(kv => kv.Key, kv => kv.Value);

            s_operandExpressionBuild = new Dictionary<OperandEnum, Func<Expression?, Expression, Expression>>()
            {
                [OperandEnum.GreaterThan] = (left, right) => Expression.GreaterThan(left!, right),
                [OperandEnum.GreaterOrEqualThan] = (left, right) => Expression.GreaterThanOrEqual(left!, right),
                [OperandEnum.LesserThan] = (left, right) => Expression.LessThan(left!, right),
                [OperandEnum.LesserOrEqualThan] = (left, right) => Expression.LessThanOrEqual(left!, right),
                [OperandEnum.Equal] = (left, right) => Expression.Equal(left!, right),
                [OperandEnum.NotEqual] = (left, right) => Expression.NotEqual(left!, right),
                [OperandEnum.Different] = (left, right) => Expression.NotEqual(left!, right),
                [OperandEnum.Not] = (_, right) => Expression.Not(right),
            }.ToFrozenDictionary(kv => kv.Key, kv => kv.Value);

            s_mathOperatorExpressionBuild = new Dictionary<MathOperatorEnum, Func<Expression?, Expression, Expression>>()
            {
                [MathOperatorEnum.Multiply] = (left, right) => Expression.Multiply(left!, right),
                [MathOperatorEnum.Modulo] = (left, right) => Expression.Modulo(left!, right),
                [MathOperatorEnum.Sum] = (left, right) => Expression.Add(left!, right),
                [MathOperatorEnum.Sub] = (left, right) => Expression.Subtract(left!, right),
                [MathOperatorEnum.Divide] = (left, right) => Expression.Divide(left!, right),
            }.ToFrozenDictionary(kv => kv.Key, kv => kv.Value);

            var delegateType = typeof(Delegate);

            s_actionTemplateTypes = typeof(Action).Assembly.GetTypes()
                                                           .Where(t => t.IsAssignableTo(delegateType) &&
                                                                       (t.Name.StartsWith(nameof(Action)) ||
                                                                        t.Name.StartsWith(nameof(Action) + "`")))
                                                           .GroupBy(t => t.GetGenericArguments().Length)
                                                           .ToFrozenDictionary(k => k.Key, v => v.First());

            s_functionTemplateTypes = typeof(Action).Assembly.GetTypes()
                                                             .Where(t => t.IsAssignableTo(delegateType) && t.Name.StartsWith(nameof(Func<int>) + "`"))
                                                             .GroupBy(t => t.GetGenericArguments().Length - 1)
                                                             .ToFrozenDictionary(k => k.Key, v => v.First());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the access to direct object
        /// </summary>
        public static AccessExpressionDefinition CreateAccess<TTargetEntity>(this TTargetEntity? directObject)
        {
            if (directObject is not null && typeof(TTargetEntity).IsAssignableTo(typeof(LambdaExpression)))
                return CreateAccess((LambdaExpression)(object)directObject);

            return new AccessExpressionDefinition((ConcretType)typeof(TTargetEntity).GetAbstractType(), new TypedArgument<TTargetEntity?>(directObject, null), null, null);
        }

        /// <summary>
        /// Creates the access from an expression
        /// </summary>
        public static AccessExpressionDefinition CreateAccess<TTargetEntity, TFrom>(this Expression<Func<TFrom, TTargetEntity>> lambdaAccessExpression)
        {
            return CreateAccess((LambdaExpression)lambdaAccessExpression);
        }

        /// <summary>
        /// Creates the access from an expression
        /// </summary>
        public static AccessExpressionDefinition CreateAccess<TTargetEntity>(this Expression<Func<TTargetEntity>> lambdaAccessExpression)
        {
            return CreateAccess((LambdaExpression)lambdaAccessExpression);
        }

        /// <summary>
        /// Creates the access from an expression
        /// </summary>
        public static AccessExpressionDefinition CreateAccess(this LambdaExpression lambdaAccessExpression)
        {
            ArgumentNullException.ThrowIfNull(lambdaAccessExpression);

            MemberInitializationDefinition? initMember = null;
            string? callChain = null;

            if (lambdaAccessExpression.NodeType != ExpressionType.Lambda)
                throw new InvalidOperationException("Only lambda type could be used as configuration provider");

            var nodeType = lambdaAccessExpression.Body.NodeType;
            if (nodeType == ExpressionType.Constant)
            {
                var cstExpression = (ConstantExpression)lambdaAccessExpression.Body;
                var cstValue = cstExpression.Value;
                return new AccessExpressionDefinition((ConcretType)cstExpression.Type.GetAbstractType(), TypedArgument.From(cstValue, cstExpression.Type), null, null);
            }

            if (nodeType == ExpressionType.MemberInit || nodeType == ExpressionType.New)
            {
                initMember = lambdaAccessExpression.SerializeMemberInitialization();
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression)lambdaAccessExpression.Body;
                if (IsParameterSimpleChainCall(memberExpr))
                {
                    callChain = DynamicCallHelper.GetCallChain(lambdaAccessExpression.Body)!;
                }
                else
                {
                    var cstValue = memberExpr.ExtractConstantValue(out var objType);
                    return new AccessExpressionDefinition((ConcretBaseType)objType!.GetAbstractType(), TypedArgument.From(cstValue, objType!), null, null);
                }
            }
            else if (nodeType == ExpressionType.Parameter)
            {
                callChain = DynamicCallHelper.GetCallChain(lambdaAccessExpression.Body)!;
            }
            else
            {
                throw new NotSupportedException("Expression access is not supported " + lambdaAccessExpression);
            }

            return new AccessExpressionDefinition((ConcretBaseType)lambdaAccessExpression.ReturnType.GetAbstractType(), null, callChain, initMember);
        }

        /// <summary>
        /// Creates the expression to access the information
        /// </summary>
        public static Expression CreateExpression(this AccessExpressionDefinition access, IReadOnlyDictionary<int, ParameterExpression>? parameters = null)
        {
#pragma warning disable IDE0031 // Use null propagation
            if (access.MemberInit is not null)
                access.MemberInit.ToMemberInitializationLambdaExpression(parameters ?? new Dictionary<int, ParameterExpression>());
#pragma warning restore IDE0031 // Use null propagation

            if (!string.IsNullOrEmpty(access.ChainCall))
                return DynamicCallHelper.CompileCallChainAccess(parameters!.Single().Value, access.ChainCall, containRoot: true).Body;

            return Expression.Constant(access.DirectObject?.GetValue(), access.TargetType.ToType());
        }

        /// <summary>
        /// Extracts the constant value from a <see cref="MemberExpression"/>
        /// </summary>
        public static object? ExtractConstantValue(this MemberExpression member, out Type? objectType)
        {
            objectType = null;

            if (member.Expression is ConstantExpression context)
            {
                var cstValue = context.Type.GetValueFromPropertyOrField(context.Value,
                                                                        member.Member,
                                                                        out objectType);
                return cstValue;
            }

            if (member.Expression is MemberExpression nestedMember)
            {
                var contextInst = ExtractConstantValue(nestedMember, out objectType);

                ArgumentNullException.ThrowIfNull(objectType);

                var cstValue = objectType!.GetValueFromPropertyOrField(contextInst,
                                                                       member.Member,
                                                                       out objectType);
                return cstValue;
            }

            if (member.NodeType == ExpressionType.MemberAccess)
            {
                Debug.Assert(member.Expression is null);

                var cstValue = member.Member.DeclaringType!.GetValueFromPropertyOrField(null,
                                                                                        member.Member,
                                                                                        out objectType);
                return cstValue;
            }

            throw new NotSupportedException("Could not resolved the expression to extract the value : " + member?.NodeType);
        }

        /// <summary>
        /// Serialize a member initialization from <paramref name="expression"/>
        /// </summary>
        public static MemberInitializationDefinition SerializeMemberInitialization<TInput, TOutput>(this Expression<Func<TInput, TOutput>> expression)
        {
            return SerializeMemberImplInitialization(expression, typeof(TOutput), expression.Parameters.ToArray());
        }

        /// <summary>
        /// Serialize a member initialization from <paramref name="expression"/>
        /// </summary>
        public static MemberInitializationDefinition SerializeMemberInitialization(this LambdaExpression expression)
        {
            return SerializeMemberImplInitialization(expression, expression.ReturnType, expression.Parameters.ToArray());
        }

        /// <summary>
        /// Serializes the member initialization.
        /// </summary>
        private static MemberInitializationDefinition SerializeMemberImplInitialization(this LambdaExpression expression, Type output, params ParameterExpression[] inputs)

        {
            var indexedInputs = inputs.Select((input, indx) => (input, indx))
                                      .ToDictionary(kv => kv.input, kv => kv.indx);

            var concretInputs = inputs.Select(i => (ConcretType)i.Type.GetAbstractType()).ToArray();

            return SerializeMemberImplInitializationFromExpression(expression.Body, indexedInputs, concretInputs);
        }

        private static MemberInitializationDefinition SerializeMemberImplInitializationFromExpression(this Expression expression, Dictionary<ParameterExpression, int> indexedInputs, IReadOnlyCollection<ConcretType> concretInputs)
        {
            AbstractMethod? ctor = null;
            List<MemberBindingDefinition>? bindings = null;

            NewExpression newBody;

            if (expression.NodeType == ExpressionType.MemberInit)
            {
                var memberInit = (MemberInitExpression)expression;
                newBody = memberInit.NewExpression;

                bindings = SerializeMemberInitializationImpl(indexedInputs, memberInit);
            }
            else if (expression.NodeType == ExpressionType.New)
            {
                newBody = (NewExpression)expression;
            }
            else
            {
                throw new InvalidCastException("Lambda expression must be a member type initialization");
            }

            bindings ??= new List<MemberBindingDefinition>();
            ctor = newBody!.Constructor?.GetAbstractMethod();

            if (ctor is not null)
            {
                var ctorBindings = ctor.Arguments.Select((arg, indx) =>
                {
                    var expr = newBody.Arguments[indx];
                    var parameters = newBody.Constructor!.GetParameters();

                    return BuildMemberBinding(indexedInputs, parameters[indx].Name!, true, expr);
                });

                bindings.AddRange(ctorBindings);
            }

            return new MemberInitializationDefinition((ConcretType)newBody.Type.GetAbstractType(),
                                                      concretInputs,
                                                      ctor,
                                                      bindings);
        }

        private static List<MemberBindingDefinition> SerializeMemberInitializationImpl(Dictionary<ParameterExpression, int> indexedInputs,
                                                                                       MemberInitExpression memberInit)
        {
            var memberBindings = new List<MemberBindingDefinition>(memberInit.Bindings.Count + 1);

            foreach (var binding in memberInit.Bindings)
            {
                if (binding.BindingType == MemberBindingType.Assignment && binding is MemberAssignment assign)
                {
                    var bind = BuildMemberBinding(indexedInputs, assign.Member.Name, false, assign.Expression);
                    memberBindings.Add(bind);
                }
                //else if (binding.BindingType == MemberBindingType.MemberBinding)
                //{
                //}
                else
                {
                    throw new NotSupportedException("NotSupported yet " + binding.BindingType);
                }
            }

            return memberBindings;
        }

        /// <summary>
        /// Create a <see cref="MemberBindingDefinition"/> based on binding from ctor or expression info
        /// </summary>
        private static MemberBindingDefinition BuildMemberBinding(Dictionary<ParameterExpression, int> indexedInputs,
                                                                  string memberName,
                                                                  bool isCtor,
                                                                  Expression expression)
        {
            if (expression is ConstantExpression constant)
            {
                return (MemberBindingDefinition)Activator.CreateInstance(s_memberInputConstantBindingDefinition.MakeGenericType(constant.Type),
                                                                         new object?[] { isCtor, memberName, constant.Value })!;
            }

            if (expression is MemberExpression member)
            {
                var access = CreateAccess(Expression.Lambda(member, indexedInputs.OrderBy(kv => kv.Value).Select(kv => kv.Key))!);
                return new MemberInputAccessBindingDefinition(isCtor, memberName, access);
            }

            if (expression is ParameterExpression parameter)
            {
                var paramIndex = indexedInputs[parameter];
                return new MemberInputParameterBindingDefinition(isCtor, memberName, paramIndex);
            }

            if (expression is MemberInitExpression initNested)
            {
                var nestedExp = SerializeMemberImplInitializationFromExpression(initNested,
                                                                                indexedInputs,
                                                                                indexedInputs.Select(i => (ConcretType)i.Key.Type.GetAbstractType()).ToArray());
                return new MemberInputNestedInitBindingDefinition(isCtor,
                                                                  memberName,
                                                                  initNested.NewExpression.Type.GetAbstractType(),
                                                                  nestedExp.Ctor,
                                                                  nestedExp.Bindings);
            }

            if (expression.NodeType == ExpressionType.Convert && expression is UnaryExpression convertExpression)
            {
                var access = CreateAccess(Expression.Lambda(convertExpression.Operand, indexedInputs.OrderBy(kv => kv.Value).Select(kv => kv.Key))!);
                return new MemberInputConvertBindingDefinition(isCtor,
                                                               memberName,
                                                               (ConcretType)convertExpression.Type.GetAbstractType(),
                                                               access);
            }

            throw new NotSupportedException("Expression could not be serialized " + expression);
        }

        /// <summary>
        /// Converts back a <see cref="MemberInitializationDefinition"/> to an expression 
        /// </summary>
        public static Expression<Func<TInput, TOutput>> ToMemberInitializationExpression<TInput, TOutput>(this MemberInitializationDefinition def, bool useCache = true)
        {
            return (Expression<Func<TInput, TOutput>>)ToMemberInitializationLambdaExpression(def, useCache);
        }

        /// <summary>
        /// Converts back a <see cref="MemberInitializationDefinition"/> to an expression 
        /// </summary>
        public static Expression<Func<TOutput>> ToMemberInitializationExpression<TOutput>(this MemberInitializationDefinition def, bool useCache = true)
        {
            return (Expression<Func<TOutput>>)ToMemberInitializationLambdaExpression(def, useCache);
        }

        /// <summary>
        /// Converts back a <see cref="MemberInitializationDefinition"/> to an expression 
        /// </summary>
        public static LambdaExpression ToMemberInitializationExpression(this MemberInitializationDefinition def, bool useCache = true)
        {
            return ToMemberInitializationLambdaExpression(def, useCache);
        }

        /// <summary>
        /// Serializes predicate function
        /// </summary>
        /// <remarks>
        ///     Tolerate only some action to be able to execute the predicte on all condition and different computeur
        /// </remarks>
        public static ConditionExpressionDefinition Serialize<TInputA, TInputB>(this Expression<Func<TInputA, TInputB, bool>> expression)
        {
            return SerializeConditionalLambda(expression);
        }

        /// <summary>
        /// Serializes predicate function
        /// </summary>
        /// <remarks>
        ///     Tolerate only some action to be able to execute the predicte on all condition and different computeur
        /// </remarks>
        public static ConditionExpressionDefinition Serialize<TInput>(this Expression<Func<TInput, bool>> expression)
        {
            return SerializeConditionalLambda(expression);
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="LambdaExpression"/>.
        /// </summary>
        public static LambdaExpression ToExpressionDelegate(this ConditionExpressionDefinition expressionDefinition, bool useCache = true)
        {
            return ToExpressionDelegate(expressionDefinition, null, useCache);
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="LambdaExpression"/>.
        /// </summary>
        public static LambdaExpression ToExpressionDelegateWithResult(this ConditionExpressionDefinition expressionDefinition, bool useCache = true)
        {
            return ToExpressionDelegate(expressionDefinition, typeof(bool), useCache);
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="LambdaExpression"/>.
        /// </summary>
        public static LambdaExpression ToExpressionDelegate(this ConditionExpressionDefinition expressionDefinition,
                                                            Type? returnType = null,
                                                            bool useCache = true)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            if (useCache)
            {
                s_conditionalExperssionDefCacheLocker.EnterReadLock();
                try
                {
                    if (s_conditionalExperssionDefCache.TryGetValue(expressionDefinition, out var lambda))
                        return lambda;
                }
                finally
                {
                    s_conditionalExperssionDefCacheLocker.ExitReadLock();
                }
            }

            var inputs = expressionDefinition.Parameters
                                             .Select(p => (Expression.Parameter(p.Type.ToType(), p.Name), definition: p))
                                             .ToDictionary(k => k.Item1, v => v.definition);

            var body = SerializeConditionsToExpression(expressionDefinition.Condition, inputs);

            var orderedInputs = inputs.OrderBy(kv => kv.Value.Order).Select(kv => kv.Key).ToArray();

            Type? delegateType = null;

            var delegateModels = s_actionTemplateTypes;
            var delegateGenericTypes = orderedInputs.Select(s => s.Type).ToArray();

            var hasReturnType = returnType != null && returnType != typeof(void);

            if (hasReturnType)
            {
                delegateModels = s_functionTemplateTypes;
                delegateGenericTypes = delegateGenericTypes.Append(returnType!).ToArray();
            }

            if (delegateModels.TryGetValue(inputs.Count, out var delegateModelType))
                delegateType = delegateModelType.MakeGenericType(delegateGenericTypes);

            if (delegateType == null)
                throw new InvalidDataException("Couldn't found a delegate template model in action or func. May be due to arguments numbers");

            var result = LambdaExpression.Lambda(delegateType, body, orderedInputs);

            if (useCache)
            {
                s_conditionalExperssionDefCacheLocker.EnterWriteLock();
                try
                {
                    if (!s_conditionalExperssionDefCache.ContainsKey(expressionDefinition))
                        s_conditionalExperssionDefCache.Add(expressionDefinition, result);
                }
                finally
                {
                    s_conditionalExperssionDefCacheLocker.ExitWriteLock();
                }
            }

            return result;
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="Expression{Func{TInput, TReturn}}"/>.
        /// </summary>
        public static Expression<Func<TInput, TReturn>> ToExpression<TInput, TReturn>(this ConditionExpressionDefinition expressionDefinition, bool useCache = true)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            return ToExpressionDelegate<Func<TInput, TReturn>>(expressionDefinition, typeof(TReturn), useCache);
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="Expression{Func{TInput, TInputB, TReturn}}"/>.
        /// </summary>
        public static Expression<Func<TInputA, TInputB, TReturn>> ToExpression<TInputA, TInputB, TReturn>(this ConditionExpressionDefinition expressionDefinition, bool useCache = true)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            return ToExpressionDelegate<Func<TInputA, TInputB, TReturn>>(expressionDefinition, typeof(TReturn), useCache);
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="Expression{Func{TInput, TInputB, TInputC, TReturn}}"/>.
        /// </summary>
        public static Expression<Func<TInputA, TInputB, TInputC, TReturn>> ToExpression<TInputA, TInputB, TInputC, TReturn>(this ConditionExpressionDefinition expressionDefinition, bool useCache = true)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            return ToExpressionDelegate<Func<TInputA, TInputB, TInputC, TReturn>>(expressionDefinition, typeof(TReturn), useCache);
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="Expression{Func{TInput, TInputB, TInputC, TInputD, TReturn}}"/>.
        /// </summary>
        public static Expression<Func<TInputA, TInputB, TInputC, TInputD, TReturn>> ToExpression<TInputA, TInputB, TInputC, TInputD, TReturn>(this ConditionExpressionDefinition expressionDefinition, bool useCache = true)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            return ToExpressionDelegate<Func<TInputA, TInputB, TInputC, TInputD, TReturn>>(expressionDefinition, typeof(TReturn), useCache);
        }

        /// <summary>
        /// Converts <see cref="ExpressionType"/> to operand <see cref="OperandEnum"/>
        /// </summary>
        public static OperandEnum ToOperand(this ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    return OperandEnum.Equal;

                case ExpressionType.NotEqual:
                    return OperandEnum.NotEqual;

                case ExpressionType.GreaterThan:
                    return OperandEnum.GreaterThan;

                case ExpressionType.GreaterThanOrEqual:
                    return OperandEnum.GreaterOrEqualThan;

                case ExpressionType.LessThan:
                    return OperandEnum.LesserThan;

                case ExpressionType.LessThanOrEqual:
                    return OperandEnum.LesserOrEqualThan;

                case ExpressionType.Not:
                    return OperandEnum.Not;
            }

            throw new NotImplementedException("Operand not managed " + expressionType);
        }

        /// <summary>
        /// Converts <see cref="ExpressionType"/> to math operator <see cref="MathOperatorEnum"/>
        /// </summary>
        public static MathOperatorEnum ToMathOperand(this ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Subtract:
                    return MathOperatorEnum.Sub;

                case ExpressionType.Multiply:
                    return MathOperatorEnum.Multiply;

                case ExpressionType.Modulo:
                    return MathOperatorEnum.Modulo;

                case ExpressionType.Divide:
                    return MathOperatorEnum.Divide;

                case ExpressionType.Add:
                    return MathOperatorEnum.Sum;
            }

            throw new NotImplementedException("Math operator not managed " + expressionType);
        }

        /// <summary>
        /// Replaces the parameter of the lambda expression.
        /// </summary>
        /// <remarks>
        ///     Usefull to keep a filter lambda but to apply on a parent level
        /// </remarks>
        public static Expression<Func<TNewParamter, TOutput>> ReplaceParameter<TOldParameter, TNewParamter, TOutput>(this Expression<Func<TOldParameter, TOutput>> source, Expression<Func<TNewParamter, TOldParameter>> replace)
        {
            var newLambdaBody = ReplaceParameter(source.Body, source.Parameters.First(), replace.Body);
            return Expression.Lambda<Func<TNewParamter, TOutput>>(newLambdaBody, replace.Parameters.First());
        }

        /// <summary>
        /// Replaces the parameter of the lambda expression.
        /// </summary>
        /// <remarks>
        ///     Usefull to keep a filter lambda but to apply on a parent level
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expression ReplaceParameter(this Expression source, ParameterExpression sourceParameter, Expression replace)
        {
            return ReplaceParameter(source, new Dictionary<ParameterExpression, Expression>()
            {
                { sourceParameter, replace }
            });
        }

        /// <summary>
        /// Replaces the parameter of the lambda expression.
        /// </summary>
        /// <remarks>
        ///     Usefull to keep a filter lambda but to apply on a parent level
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expression ReplaceParameter(this Expression source, Dictionary<ParameterExpression, Expression> map)
        {
            return ExpressionParameterRebinder.ReplaceParameters(map, source);
        }

        #region Tools

        /// <summary>
        /// Determines whether [is parameter simple chain call] [the specified member expr].
        /// </summary>
        private static bool IsParameterSimpleChainCall(Expression? memberExpr)
        {
            if (memberExpr is null)
                return false;

            if (memberExpr is MemberExpression member)
                return IsParameterSimpleChainCall(member.Expression);
            return memberExpr is ParameterExpression;
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <typeparamref name="TDelegate"/>
        /// </summary>
        private static Expression<TDelegate> ToExpressionDelegate<TDelegate>(this ConditionExpressionDefinition expressionDefinition, Type returnType, bool useCache = true)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            var lambdaExpression = ToExpressionDelegate(expressionDefinition, returnType, useCache);
            var funcExpression = (Expression<TDelegate>)lambdaExpression;

            return funcExpression;
        }

        /// <summary>
        /// Serializes predicate function
        /// </summary>
        /// <remarks>
        ///     Tolerate only some action to be able to execute the predicte on all condition and different computeur
        /// </remarks>
        private static ConditionExpressionDefinition SerializeConditionalLambda(this LambdaExpression expression)
        {
            var inputParameter = expression.Parameters
                                           .Select((p, indx) => (parameter: p, definition: new ConditionParameterDefinition(Guid.NewGuid(), p.Name ?? string.Empty, p.Type.GetAbstractType(), (ushort)indx)))
                                           .ToImmutableDictionary(kv => kv.parameter, kv => kv.definition);

            var condition = SerializeConditions(expression.Body, inputParameter)!;

            return new ConditionExpressionDefinition(inputParameter.Values, condition);
        }

        /// <summary>
        /// Recusive methods that parcour the expression tree and convert it to <see cref="ConditionBaseDefinition"/> tree serializable
        /// </summary>
        private static ConditionBaseDefinition? SerializeConditions(Expression body, IReadOnlyDictionary<ParameterExpression, ConditionParameterDefinition> sourceInputExpressions)
        {
            switch (body.NodeType)
            {
                case ExpressionType.AndAlso:
                    return new ConditionGroupDefinition(LogicEnum.And, new[]
                    {
                        SerializeConditions(((BinaryExpression)body).Left, sourceInputExpressions)!,
                        SerializeConditions(((BinaryExpression)body).Right, sourceInputExpressions)!,
                    });

                case ExpressionType.OrElse:
                    return new ConditionGroupDefinition(LogicEnum.Or, new[]
                    {
                        SerializeConditions(((BinaryExpression)body).Left, sourceInputExpressions)!,
                        SerializeConditions(((BinaryExpression)body).Right, sourceInputExpressions)!,
                    });

                case ExpressionType.ExclusiveOr:
                    return new ConditionGroupDefinition(LogicEnum.ExclusiveOr, new[]
                    {
                        SerializeConditions(((BinaryExpression)body).Left, sourceInputExpressions)!,
                        SerializeConditions(((BinaryExpression)body).Right, sourceInputExpressions)!,
                    });

                case ExpressionType.Not:
                    return new ConditionOperandDefinition(null,
                                                          OperandEnum.Not,
                                                          SerializeConditions(((UnaryExpression)body).Operand, sourceInputExpressions)!);

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return new ConditionOperandDefinition(SerializeConditions(((BinaryExpression)body).Left, sourceInputExpressions),
                                                          body.NodeType.ToOperand(),
                                                          SerializeConditions(((BinaryExpression)body).Right, sourceInputExpressions)!);

                case ExpressionType.Call:
                    return SerializeCallCondition((MethodCallExpression)body, sourceInputExpressions);

                case ExpressionType.MemberAccess:
                    var memberAccess = (MemberExpression)body;

                    if (memberAccess.Expression != null && memberAccess.Expression.NodeType == ExpressionType.Constant)
                    {
                        // with value source is instant must indicate the expression context
                        var context = (ConstantExpression)memberAccess.Expression;
                        var cstValue = context.Type.GetField(memberAccess.Member.Name)?.GetValue(context.Value);
                        return new ConditionValueDefinition(((FieldInfo)memberAccess.Member).FieldType.GetAbstractType(), cstValue);
                    }
                    
                    var instance = SerializeConditions(memberAccess.Expression, sourceInputExpressions);
                    //var instance = SerializeCallInstance(memberAccess.Expression, sourceInputExpressions);
                    return new ConditionMemberAccessDefinition(instance, memberAccess.Member.Name);

                case ExpressionType.Constant:
                    var cst = (ConstantExpression)body;
                    return new ConditionValueDefinition(cst.Type.GetAbstractType(), cst.Value);

                case ExpressionType.Parameter:
                    return SerializeCallInstance(body, sourceInputExpressions);

                case ExpressionType.Add:
                case ExpressionType.Modulo:
                case ExpressionType.Divide:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                    return new ConditionMathOperationDefinition(SerializeConditions(((BinaryExpression)body).Left, sourceInputExpressions),
                                                                body.NodeType.ToMathOperand(),
                                                                SerializeConditions(((BinaryExpression)body).Right, sourceInputExpressions)!);

                case ExpressionType.TypeAs:
                case ExpressionType.Convert:
                    return new ConditionConvertDefinition(SerializeConditions(((UnaryExpression)body).Operand, sourceInputExpressions)!,
                                                          ((UnaryExpression)body).Type.GetAbstractType(),
                                                          body.NodeType == ExpressionType.Convert);

                case ExpressionType.And:
                //throw new NotImplementedException(body.NodeType.ToString());

                case ExpressionType.Or:
                // throw new NotImplementedException(body.NodeType.ToString());

                default:
                    throw new NotImplementedException("Action not tolerate in condition serialization : " + body.NodeType);
            }
        }

        /// <summary>
        /// Serializes the call condition.
        /// </summary>
        private static ConditionBaseDefinition SerializeCallCondition(MethodCallExpression body, IReadOnlyDictionary<ParameterExpression, ConditionParameterDefinition> sourceInputExpressions)
        {
            var instance = SerializeCallInstance(body.Object, sourceInputExpressions);
            var arguments = body.Arguments?.Select(a => SerializeConditions(a, sourceInputExpressions)!).ToArray() ?? EnumerableHelper<ConditionBaseDefinition>.ReadOnlyArray;

            return new ConditionCallDefinition(instance,
                                               body.Method.DeclaringType?.FullName + "." + body.Method.Name,
                                               arguments);
        }

        /// <summary>
        /// Serializes the call instance.
        /// </summary>
        private static ConditionBaseDefinition? SerializeCallInstance(Expression? instance, IReadOnlyDictionary<ParameterExpression, ConditionParameterDefinition> sourceInputExpressions)
        {
            if (instance == null)
                return null;

            if (instance is ParameterExpression paramExpression && sourceInputExpressions.TryGetValue(paramExpression, out var paramExpressionDefinition))
                return paramExpressionDefinition;

            // Recursion throught members
            if (instance.NodeType == ExpressionType.MemberAccess || instance.NodeType == ExpressionType.TypeAs || instance.NodeType == ExpressionType.Convert)
                return SerializeConditions(instance, sourceInputExpressions);

            throw new NotSupportedException(instance + " is not allowed as Call host.");
        }

        /// <summary>
        /// Convert back <see cref="ConditionBaseDefinition"/> to <see cref="Expression"/>
        /// </summary>
        private static Expression SerializeConditionsToExpression(ConditionBaseDefinition body, IReadOnlyDictionary<ParameterExpression, ConditionParameterDefinition> sourceInputExpressions)
        {
            if (body is ConditionParameterDefinition paramDef)
                return sourceInputExpressions.First(kv => kv.Value == paramDef).Key;

            if (body is ConditionValueDefinition value)
                return Expression.Constant(value.Value, value.Type.ToType());

            if (body is ConditionMemberAccessDefinition member)
            {
                var instance = member.Instance is null ? null : SerializeConditionsToExpression(member.Instance, sourceInputExpressions);

                var memberName = instance?.Type.GetMembers().FirstOrDefault(m => m.Name == member.MemberName);
                ArgumentNullException.ThrowIfNull(memberName);
                return Expression.MakeMemberAccess(instance, memberName);
            }

            if (body is ConditionCallDefinition call)
            {
                var instance = call.Instance is null ? null : SerializeConditionsToExpression(call.Instance, sourceInputExpressions);

                var arguments = call.Arguments?.Select(a => SerializeConditionsToExpression(a, sourceInputExpressions)).ToArray() ?? EnumerableHelper<Expression>.ReadOnlyArray;
                var argumentTypes = arguments.Select(a => a.Type).ToArray();

                var callPart = call.MethodName.Split('.');
                var typeName = string.Join('.', callPart.SkipLast(1));
                var methodDeclarationType = Type.GetType(typeName) ?? instance?.Type ?? throw new InvalidOperationException("Could not found type with following information '" + typeName + "'");

                string methodName = callPart.Last();

                var mthd = methodDeclarationType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                                                .Where(m => m.Name == methodName &&
                                                            m.GetParameters().Length == arguments.Length &&
                                                            m.GetParameters()
                                                             .Select((p, indx) => p.ParameterType.IsAssignableFrom(argumentTypes[indx]))
                                                             .All(t => t))
                                                .First();

                return Expression.Call(instance, mthd, arguments);
            }

            if (body is ConditionGroupDefinition grp)
            {
                var expressions = grp.Conditions.Select(c => SerializeConditionsToExpression(c, sourceInputExpressions)).ToArray();

                var action = s_logicalExpressionBuild[grp.Logic];

                return expressions.Aggregate((left, right) => left == null ? right : action(left, right));
            }

            if (body is ConditionOperandDefinition op)
            {
                var leftExpressions = op.Left is null ? null : SerializeConditionsToExpression(op.Left, sourceInputExpressions);

                Debug.Assert(op.Right is not null);
                var rightExpressions = SerializeConditionsToExpression(op.Right, sourceInputExpressions);

                var action = s_operandExpressionBuild[op.Operand];

                return action(leftExpressions, rightExpressions);
            }

            if (body is ConditionMathOperationDefinition mathOp)
            {
                var leftExpressions = mathOp.Left is null ? null : SerializeConditionsToExpression(mathOp.Left, sourceInputExpressions);

                Debug.Assert(mathOp.Right is not null);
                var rightExpressions = SerializeConditionsToExpression(mathOp.Right, sourceInputExpressions);

                var action = s_mathOperatorExpressionBuild[mathOp.MathOperator];

                return action(leftExpressions, rightExpressions);
            }

            if (body is ConditionConvertDefinition cnv)
            {
                var fromExpression = SerializeConditionsToExpression(cnv.From, sourceInputExpressions);

                if (cnv.StrictCast)
                    return Expression.Convert(fromExpression, cnv.To.ToType());
                return Expression.TypeAs(fromExpression, cnv.To.ToType());
            }

            throw new NotSupportedException("Serialize expression restoration not restorable");
        }

        /// <summary>
        /// Converts back a <see cref="MemberInitializationDefinition"/> to an expression 
        /// </summary>
        public static LambdaExpression ToMemberInitializationLambdaExpression(this MemberInitializationDefinition def, bool useCache = true)
        {
            if (useCache)
            {
                s_memberInitExperssionDefCacheLocker.EnterReadLock();
                try
                {
                    if (s_memberInitExperssionDefCache.TryGetValue(def, out var lambdaExpression))
                        return lambdaExpression;
                }
                finally
                {
                    s_memberInitExperssionDefCacheLocker.ExitReadLock();
                }
            }

            var indexedArgs = def.Inputs.Select((input, index) => (input, index))
                                        .ToDictionary(k => k.index, kv => Expression.Parameter(kv.input.ToType(), "Arg" + kv.index));

            var lambda = ToMemberInitializationLambdaExpression(def, indexedArgs, useCache);

            if (useCache)
            {
                s_memberInitExperssionDefCacheLocker.EnterWriteLock();
                try
                {
                    if (!s_memberInitExperssionDefCache.ContainsKey(def))
                        s_memberInitExperssionDefCache.Add(def, lambda);
                }
                finally
                {
                    s_memberInitExperssionDefCacheLocker.ExitWriteLock();
                }
            }

            return lambda;
        }

        /// <summary>
        /// Converts back a <see cref="MemberInitializationDefinition"/> to an expression 
        /// </summary>
        public static LambdaExpression ToMemberInitializationLambdaExpression(this MemberInitializationDefinition def, IReadOnlyDictionary<int, ParameterExpression> indexedArgs, bool useCache = true)
        {
            NewExpression ctorExp;

            var initType = def.NewType.ToType();
            var bindings = def.Bindings;

            if (def.Ctor is not null)
            {
                var type = def.NewType!.ToType();
                var ctor = def.Ctor.ToMethod(type) as ConstructorInfo;

                ArgumentNullException.ThrowIfNull(ctor);

                var ctorArgs = bindings.Where(b => b.IsCtorParameter)
                                       .ToDictionary(k => k.MemberName);

                if (ctorArgs.Any())
                    bindings = bindings.Except(ctorArgs.Values).ToArray();

                var ctorParameters = ctor.GetParameters()
                                         .Select(p =>
                                         {
                                             if (!string.IsNullOrEmpty(p.Name) && ctorArgs.TryGetValue(p.Name, out var param))
                                                 return GetBindingExpression(indexedArgs, param);

                                             return Expression.Constant(p.DefaultValue);
                                         }).ToArray();

                ctorExp = Expression.New(ctor, ctorParameters);
            }
            else
            {
                ctorExp = Expression.New(def.NewType!);
            }

            var init = Expression.MemberInit(ctorExp, bindings.Select(bind => CreateBindingExpression(initType, indexedArgs, bind)));
            return Expression.Lambda(init, indexedArgs.OrderBy(kv => kv.Key).Select(kv => kv.Value));
        }

        /// <summary>
        /// Creates back binding expression from <see cref="MemberBindingDefinition"/>
        /// </summary>
        private static MemberAssignment CreateBindingExpression(Type outputType, IReadOnlyDictionary<int, ParameterExpression> indexedArgs, MemberBindingDefinition bind)
        {
            var prop = outputType.GetMember(bind.MemberName).First();
            return Expression.Bind(prop, GetBindingExpression(indexedArgs, bind));
        }

        /// <summary>
        /// Creates back binding expression from <see cref="MemberBindingDefinition"/>
        /// </summary>
        private static Expression GetBindingExpression(IReadOnlyDictionary<int, ParameterExpression> indexedArgs, MemberBindingDefinition bind)
        {
            if (bind is MemberInputConstantBindingDefinition constBind)
                return Expression.Constant(constBind.GetValue());

            if (bind is MemberInputParameterBindingDefinition param)
                return indexedArgs[param.ParameterIndex];

            if (bind is MemberInputCallChainBindingDefinition inputCallChain)
            {
                var expr = DynamicCallHelper.CompileCallChainAccess(indexedArgs[inputCallChain.ParameterIndex], inputCallChain.CallChain, containRoot: true);
                return expr.Body;
            }

            if (bind is MemberInputNestedInitBindingDefinition nestedInit)
            {
                var lambda = ToMemberInitializationLambdaExpression(new MemberInitializationDefinition((ConcretType)nestedInit.NewType,
                                                                                                 indexedArgs.Select(i => (ConcretType)i.Value.Type.GetAbstractType()),
                                                                                                 nestedInit.Ctor,
                                                                                                 nestedInit.Bindings),
                                                                    indexedArgs);
                return lambda.Body;
            }

            if (bind is MemberInputAccessBindingDefinition accessInit)
            {
                var accessExpression = accessInit.Access.CreateExpression(indexedArgs);
                return accessExpression;
            }

            if (bind is MemberInputConvertBindingDefinition convert)
            {
                var accessExpression = convert.Access.CreateExpression(indexedArgs);
                return Expression.Convert(accessExpression, convert.NewType.ToType());
            }

            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
