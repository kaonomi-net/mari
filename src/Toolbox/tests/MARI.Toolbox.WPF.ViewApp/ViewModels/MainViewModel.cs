// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.ViewApp.ViewModels
{
    using MARI.Toolbox.Abstractions.Proxies;

    public sealed class MainViewModel : BaseViewModel
    {
        public MainViewModel(IDispatcherProxy dispatcherProxy) : base(dispatcherProxy)
        {
            this.Templates = Enum.GetValues<TemplateSelectorEnum>();
        }

        public IReadOnlyCollection<TemplateSelectorEnum> Templates { get; }
    }
}
