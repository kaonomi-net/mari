// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Helpers
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    // https://stackoverflow.com/questions/10090267/use-expressionfunct-x-in-linq-contains-extension/12207396#12207396
    public sealed class ExpressionParameterRebinder : ExpressionVisitor
    {
        #region Fields

        private readonly Dictionary<ParameterExpression, Expression> _map;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionParameterRebinder"/> class.
        /// </summary>
        public ExpressionParameterRebinder(Dictionary<ParameterExpression, Expression> map)
        {
            this._map = map ?? new Dictionary<ParameterExpression, Expression>();
        }

        #endregion

        /// <inheritdoc />
        public static Expression ReplaceParameters(Dictionary<ParameterExpression, Expression> map, Expression exp)
        {
            return new ExpressionParameterRebinder(map).Visit(exp);
        }

        /// <inheritdoc />
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (this._map.TryGetValue(node, out var replacement))
                return Visit(replacement);

            return base.VisitParameter(node);
        }
    }
}
