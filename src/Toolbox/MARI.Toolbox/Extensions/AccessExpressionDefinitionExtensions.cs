// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Extensions
{
    using MARI.Toolbox.Abstractions.Expressions;
    using MARI.Toolbox.Helpers;

    using System.Linq.Expressions;

    public static class AccessExpressionDefinitionExtensions
    {
        /// <summary>
        /// Try resolve configuration based on <paramref name="input"/> and <paramref name="lambdaBaseConfiguration"/>
        /// </summary>
        public static object? Resolve(this AccessExpressionDefinition accessConfiguration, object? input)
        {
            if (!string.IsNullOrEmpty(accessConfiguration.ChainCall))
                return DynamicCallHelper.GetValueFrom(input, accessConfiguration.ChainCall, true);

            if (accessConfiguration.MemberInit is not null)
            {
                // Opti: May be cache the lambda function 
                var expr = accessConfiguration.MemberInit.ToMemberInitializationLambdaExpression();

                if (expr.Parameters.Any())
                    return expr.Compile().DynamicInvoke(input);

                return expr.Compile().DynamicInvoke();
            }

            return accessConfiguration.DirectObject?.GetValue();
        }
    }
}
