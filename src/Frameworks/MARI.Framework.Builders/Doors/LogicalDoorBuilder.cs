// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Doors
{
    using MARI.Framework.Builders;
    using MARI.Framework.Builders.Exceptions;
    using MARI.Framework.Builders.Resources;
    using MARI.Framework.Builders.Signals;
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Toolbox.Statements;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Get a <see cref="BooleanLogicalDoorDefinition"/>
    /// </summary>
    /// <seealso cref="ILogicalDoorBuilder" />
    internal sealed class LogicalDoorBuilder : ILogicalDoorBuilder, IDefinitionBaseBuilder<DoorDefinition>
    {
        #region Fields

        private static readonly Regex s_formulaCharAllowed = new Regex(@"^[a-zA-Z!&|^\(\)\s]+$");
        private static readonly Regex s_variableCharAllowed = new Regex(@"^[a-zA-Z]+$");
        private static readonly Regex s_formulaVariableToken = new Regex(@"[a-zA-Z]+");

        private readonly IDoorWithListenerBuilder _rootDoorBuilder;
        private readonly Dictionary<Guid, string> _variableNames;

        private string? _logicalFormula;
        private TimeSpan? _activeWindowInterval;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalDoorBuilder"/> class.
        /// </summary>
        public LogicalDoorBuilder(IDoorWithListenerBuilder doorBuilder)
        {
            this._rootDoorBuilder = doorBuilder;
            this._variableNames = new Dictionary<Guid, string>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ILogicalDoorBuilder ListenWindowMode(ListenWindowModeEnum windowModeEnum)
        {
            throw new NotSupportedException("Feature to yet implement and supported");
        }

        /// <inheritdoc />
        public ILogicalDoorBuilder AssignVariableName(string variableName, SignalId signalId)
        {
            ArgumentException.ThrowIfNullOrEmpty(variableName);

            if (!s_variableCharAllowed.IsMatch(variableName))
            {
                throw new InvalidParameterException(nameof(BooleanLogicalDoorDefinition.VariableNames),
                                                    BuildErrorSR.VariableOnlyCharAllowed);
            }

            CheckVariableNameNotAlreadyUsed(variableName);
            this._variableNames.Add(signalId.Uid, variableName);
            return this;
        }

        /// <inheritdoc />
        public ILogicalDoorBuilder AssignVariableName(string variableName, DoorId doorId)
        {
            ArgumentException.ThrowIfNullOrEmpty(variableName);

            if (!s_variableCharAllowed.IsMatch(variableName))
            {
                throw new InvalidParameterException(nameof(BooleanLogicalDoorDefinition.VariableNames),
                                                    BuildErrorSR.VariableOnlyCharAllowed);
            }

            CheckVariableNameNotAlreadyUsed(variableName);
            this._variableNames.Add(doorId.Uid, variableName);
            return this;
        }

        /// <inheritdoc />
        public ILogicalDoorBuilder AssignVariableName(string variableName, SignalDefinition signalId)
        {
            return AssignVariableName(variableName, signalId.SignalId);
        }

        /// <inheritdoc />
        public ILogicalDoorBuilder AssignVariableName(string variableName, DoorDefinition doorId)
        {
            return AssignVariableName(variableName, doorId.DoorId);
        }

        /// <inheritdoc />
        public ILogicalDoorBuilder UseVariableThis()
        {
            CheckVariableNameNotAlreadyUsed("this");
            this._variableNames.Add(this._rootDoorBuilder.Uid, "this");
            return this;
        }

        /// <inheritdoc />
        public IDefinitionBaseBuilder<DoorDefinition> Formula(string formula)
        {
            ArgumentException.ThrowIfNullOrEmpty(formula);
            this._logicalFormula = formula;
            return this;
        }

        /// <inheritdoc />
        public ILogicalDoorBuilder ActiveWindowInterval(TimeSpan activeWindowInterval)
        {
            this._activeWindowInterval = activeWindowInterval;
            return this;
        }

        /// <inheritdoc />
        public DoorDefinition Build()
        {
            if (this._activeWindowInterval == null)
                throw new InvalidParameterException(nameof(BooleanLogicalDoorDefinition.ActiveWindowInterval), BuildErrorSR.MandatoryValueMissing);

            if (string.IsNullOrEmpty(this._logicalFormula))
                throw new InvalidParameterException(nameof(BooleanLogicalDoorDefinition.LogicalFormula), BuildErrorSR.MandatoryValueMissing);

            var allVariablesIds = this._rootDoorBuilder.SignalIds
                                                      .Select(p => (p.Uid, p.Name))
                                                      .Concat(this._rootDoorBuilder.DoorIds.Select(s => (s.Uid, s.Name)))
                                                      .Distinct()
                                                      .ToArray();

            if (allVariablesIds.Length == 0)
            {
                throw new InvalidParameterException(nameof(this._rootDoorBuilder.SignalIds) + "-" + nameof(this._rootDoorBuilder.DoorIds),
                                                    BuildErrorSR.MandatoryValueMissing);
            }

            // All listened sources (and only them) must have a variable name
            var sourceIds = allVariablesIds.Select(v => v.Uid).ToHashSet();
            var assignedIds = this._variableNames.Keys.Where(k => k != this._rootDoorBuilder.Uid).ToHashSet();

            if (!sourceIds.SetEquals(assignedIds))
            {
                throw new InvalidParameterException(nameof(BooleanLogicalDoorDefinition.VariableNames),
                                                    BuildErrorSR.VariableNamesIsNotAlignWIthSources);
            }

            if (string.IsNullOrEmpty(this._logicalFormula))
            {
                throw new InvalidParameterException(nameof(BooleanLogicalDoorDefinition.LogicalFormula),
                                                    BuildErrorSR.MandatoryValueMissing);
            }

            // Match whole variable tokens ('a' must not be considered present in 'ab')
            var formulaTokens = s_formulaVariableToken.Matches(this._logicalFormula!)
                                                      .Select(m => m.Value)
                                                      .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingVariable = this._variableNames.Values
                                                     .Where(vr => formulaTokens.Contains(vr) == false)
                                                     .ToArray();

            if (missingVariable.Length > 0)
            {
                throw new InvalidParameterException(nameof(BooleanLogicalDoorDefinition.LogicalFormula),
                                                    BuildErrorSR.VariableNameIsMissingFromFormula.WithArguments(string.Join(", ", missingVariable)));
            }

            if (!ExpressionBuilder.CanBuildBoolLogicStatement(this._logicalFormula, this._variableNames.Values.ToArray(), out var reason))
                throw new InvalidParameterException(nameof(BooleanLogicalDoorDefinition.LogicalFormula), reason);

            return new BooleanLogicalDoorDefinition(this._rootDoorBuilder.Uid,
                                                    RefIdHelper.Generate(RefTypeEnum.Door, this._rootDoorBuilder.SimpleNameIdentifier, this._rootDoorBuilder.DefinitionMetaData?.NamespaceIdentifier),
                                                    this._rootDoorBuilder.DisplayName ?? this._rootDoorBuilder.SimpleNameIdentifier,
                                                    this._logicalFormula,
                                                    typeof(ILogicalDoorVGrain).AssemblyQualifiedName!,
                                                    this._rootDoorBuilder.SignalIds,
                                                    this._rootDoorBuilder.DoorIds,
                                                    this._variableNames.ToDictionary(kv => kv.Value, kv => kv.Key),
                                                    this._variableNames.ContainsKey(this._rootDoorBuilder.Uid),
                                                    this._rootDoorBuilder.DefinitionMetaData,
                                                    this._activeWindowInterval,
                                                    this._rootDoorBuilder.RetentionMaxPeriod,
                                                    this._rootDoorBuilder.HistoryMaxRetention,
                                                    this._rootDoorBuilder.NotConsumedMaxRetention);
        }

        /// <summary>
        /// Ensure a variable name is not assigned twice (formula variables are case insensitive)
        /// </summary>
        private void CheckVariableNameNotAlreadyUsed(string variableName)
        {
            if (this._variableNames.Values.Contains(variableName, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidParameterException(nameof(BooleanLogicalDoorDefinition.VariableNames),
                                                    "Variable name '" + variableName + "' is already assigned");
            }
        }

        #endregion
    }
}
