// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.ViewApp
{
    using MARI.Toolbox.WPF.UI.Avalon.Services;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow(AvalonDockViewManager? avalonDockViewManager = null)
        {
            InitializeComponent();

            //this.avalonDock.Theme = new ToolBoxTheme();
            avalonDockViewManager?.RegisterDockManager(this.avalonDock);
        }
    }
}