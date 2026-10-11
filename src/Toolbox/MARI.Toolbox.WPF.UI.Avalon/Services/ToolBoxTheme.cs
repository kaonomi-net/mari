// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Avalon.Services
{
    using AvalonDock.Themes;

    using System;

    internal sealed class ToolBoxTheme : Theme
    {
        public override Uri GetResourceUri()
        {
            return new Uri("/MARI.Toolbox.WPF.UI.Avalon;component/Resources/AvalonThemeBrushes.xaml", UriKind.Relative);
        }
    }
}
