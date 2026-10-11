// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Abstractions.Views
{
    using System;

    /// <summary>
    /// View relation between view and view model
    /// </summary>
    public sealed record ViewRelation(Guid Uid, Type View, Type ViewModel, ViewRelationInfo Info);
}
