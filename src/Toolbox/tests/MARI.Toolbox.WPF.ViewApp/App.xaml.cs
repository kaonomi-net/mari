// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.ViewApp
{
    using MARI.Toolbox.Abstractions.Proxies;
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Services;
    using MARI.Toolbox.WPF.Abstractions.Services;
    using MARI.Toolbox.WPF.Abstractions.Views;
    using MARI.Toolbox.WPF.Abstractions;
    using MARI.Toolbox.WPF.UI.Avalon.Services;
    using MARI.Toolbox.WPF.UI.Services;
    using Microsoft.Extensions.DependencyInjection;

    using System.Configuration;
    using System.Data;
    using System.Windows;
    using Microsoft.Extensions.Configuration;
    using MARI.Toolbox.WPF.ViewApp.ViewModels;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _host;

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ServiceCollection();

            var cfgBuilder = new ConfigurationBuilder();

            builder.AddSingleton<IDispatcherProxy>(new UIDispatcher(this.Dispatcher));

            builder.SetupView<AvalonDockViewManager>(b =>
            {
                b.Register<MainWindow, MainViewModel>();
                // .Register<DocumentExplorerView, DocumentExplorerViewModel>(new ViewRelationInfo(key: ViewContantsKey.DocumentExplorer, viewPosition: ViewPositionEnum.Left))
                // .Register<DocumentDetailView, DocumentDetailViewModel>(new ViewRelationInfo(key: ViewContantsKey.DocumentDetails, viewPosition: ViewPositionEnum.Center))
                // .Register<ClusterMonitorView, ClusterMonitorViewModel>(new ViewRelationInfo(key: ViewContantsKey.ClusterMonitoring, viewPosition: ViewPositionEnum.Right));
            });

            this._host = builder.BuildServiceProvider();

            var viewManager = this._host.GetRequiredService<IViewManager>();
            viewManager.Show<MainWindow>();

            base.OnStartup(e);
        }
    }
}
