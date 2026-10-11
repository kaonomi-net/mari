// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.ComServer.Client.Test
{
    using MARI.Toolbox.Abstractions.Proxies;
    using MARI.Toolbox.WPF.UI.Services;

    using Microsoft.Extensions.DependencyInjection;

    using System.Windows;

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

            builder.AddSingleton<IDispatcherProxy>(new UIDispatcher(this.Dispatcher));

            //builder.SetupView<AvalonDockViewManager>(b =>
            //{
            //    b.Register<MainWindow, MainViewModel>();
            //    // .Register<DocumentExplorerView, DocumentExplorerViewModel>(new ViewRelationInfo(key: ViewContantsKey.DocumentExplorer, viewPosition: ViewPositionEnum.Left))
            //    // .Register<DocumentDetailView, DocumentDetailViewModel>(new ViewRelationInfo(key: ViewContantsKey.DocumentDetails, viewPosition: ViewPositionEnum.Center))
            //    // .Register<ClusterMonitorView, ClusterMonitorViewModel>(new ViewRelationInfo(key: ViewContantsKey.ClusterMonitoring, viewPosition: ViewPositionEnum.Right));
            //});

            this._host = builder.BuildServiceProvider();

            //var viewManager = this._host.GetRequiredService<IViewManager>();
            //viewManager.Show<MainWindow>();

            var mainWindow = new MainWindow
            {
                DataContext = ActivatorUtilities.CreateInstance<MainViewModel>(this._host)
            };

            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}
