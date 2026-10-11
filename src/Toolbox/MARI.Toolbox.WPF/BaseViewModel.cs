// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF
{
    using MARI.Toolbox.Abstractions.Commands;
    using MARI.Toolbox.Abstractions.Proxies;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Memories;

    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows.Input;

    /// <summary>
    /// Base class to every view model with dynamic content between view and view model
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly SafeContainer<BaseViewModel> _linkViewModels;
        private readonly SafeContainer<object> _resources;

        private readonly HashSet<ICommandExt> _commandExts;

        private long _workingCount;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseViewModel"/> class.
        /// </summary>
        public BaseViewModel(IDispatcherProxy dispatcherProxy)
        {
            this._linkViewModels = new SafeContainer<BaseViewModel>();
            this._resources = new SafeContainer<object>();

            this._commandExts = new HashSet<ICommandExt>();

            this.DispatcherProxy = dispatcherProxy;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is working.
        /// </summary>
        public bool IsWorking
        {
            get { return Interlocked.Read(ref this._workingCount) > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is not working.
        /// </summary>
        public bool IsNotWorking
        {
            get { return !this.IsWorking; }
        }

        /// <summary>
        /// Self property that return this. Usefull for some binding situation with converter that need the full object
        /// </summary>
        [IgnoreDataMember]
        public BaseViewModel Self
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the dispatcher proxy.
        /// </summary>
        protected IDispatcherProxy DispatcherProxy { get; }

        #endregion

        #region Events

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Sets the property.
        /// </summary>
        public bool SetProperty<TProp>(ref TProp prop, TProp value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<TProp>.Default.Equals(prop, value))
                return false;

            prop = value;

            OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Registers the command.
        /// </summary>
        protected void RegisterCommand<TCommand>(TCommand command)
            where TCommand : ICommandExt
        {
            this._commandExts.Add(command);
        }

        /// <summary>
        /// Registers the command.
        /// </summary>
        protected void RegisterCommand(ICommand command)
        {
            if (command is ICommandExt commandExt)
                this._commandExts.Add(commandExt);
        }

        /// <summary>
        /// Refreshes the command status.
        /// </summary>
        protected void RefreshCommandStatus()
        {
            this.DispatcherProxy.Send(() =>
            {
                foreach (var cmd in this._commandExts)
                    cmd.RefreshCanExecuteStatus();
            });
        }

        /// <summary>
        /// Disposes all resources.
        /// </summary>
        protected async ValueTask DisposeAllResourcesAsync()
        {
            var resources = this._resources.GetContainerCopy();

            this._resources.Clear();
            this._linkViewModels.Clear();

            List<Exception>? exceptions = null;

            foreach (var resource in resources)
            {
                if (resource is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                        continue;
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= new List<Exception>();
                        exceptions.Add(ex);
                    }
                }

                if (resource is IAsyncDisposable asyncDisposable)
                {
                    try
                    {
                        await asyncDisposable.DisposeAsync();
                        continue;
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= new List<Exception>();
                        exceptions.Add(ex);
                    }
                }
            }

            if (exceptions is not null && exceptions.Count > 0)
                this.DispatcherProxy.Throw(new AggregateException(exceptions));
        }

        /// <summary>
        /// Disposes all resources synchronously, without blocking on asynchronous disposal.
        /// </summary>
        /// <remarks>
        ///     <see cref="IDisposable"/> resources are disposed synchronously on the calling thread.
        ///     Resources only <see cref="IAsyncDisposable"/> are started on the calling thread and NOT awaited
        ///     to prevent dead lock when called from the dispatcher thread; their failures are reported through <see cref="IDispatcherProxy.Throw"/>.
        /// </remarks>
        protected void DisposeAllResources()
        {
            var resources = this._resources.GetContainerCopy();

            this._resources.Clear();
            this._linkViewModels.Clear();

            List<Exception>? exceptions = null;

            foreach (var resource in resources)
            {
                if (resource is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                        continue;
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= new List<Exception>();
                        exceptions.Add(ex);
                    }
                }

                if (resource is IAsyncDisposable asyncDisposable)
                {
                    try
                    {
                        var pending = asyncDisposable.DisposeAsync();
                        if (pending.IsCompletedSuccessfully)
                            continue;

                        var dispatcherProxy = this.DispatcherProxy;
                        pending.AsTask().ContinueWith(t => dispatcherProxy.Throw(new AggregateException(t.Exception!.InnerExceptions)),
                                                      CancellationToken.None,
                                                      TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                                                      TaskScheduler.Default);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= new List<Exception>();
                        exceptions.Add(ex);
                    }
                }
            }

            if (exceptions is not null && exceptions.Count > 0)
                this.DispatcherProxy.Throw(new AggregateException(exceptions));
        }

        /// <summary>
        /// Registers a resource attached to this view model instances.
        /// </summary>
        /// <remarks>
        ///     Resources will be disposed at same time as this instance
        /// </remarks>
        protected Guid RegisterResource<TInst>(TInst resource)
            where TInst : class
        {
            return this._resources.Register(resource);
        }

        /// <summary>
        /// Registers a link view model.
        /// </summary>
        /// <remarks>
        ///     All start work will be diffused to sub view model
        /// </remarks>
        protected Guid RegisterLinkViewModel<TInst>(TInst instance, bool autoDispose = true)
            where TInst : BaseViewModel
        {
            if (autoDispose)
                RegisterResource(instance);

            return this._linkViewModels.Register(instance);
        }

        /// <summary>
        /// Removes the link view model.
        /// </summary>
        protected void RemoveLinkViewModel<TInst>(TInst instance)
            where TInst : BaseViewModel
        {
            this._resources.Remove(instance);
            this._linkViewModels.Remove(instance);
        }

        /// <summary>
        /// To be placed in a using scope to put the view model in working mode
        /// </summary>
        /// <remarks>
        ///     Support recusive scope
        /// </remarks>
        protected IDisposable StartWorking()
        {
            Interlocked.Increment(ref this._workingCount);
            RefreshCommandStatus();

            OnPropertyChanged(nameof(this.IsWorking));
            OnPropertyChanged(nameof(this.IsNotWorking));

            var linkStartWorking = this._linkViewModels.GetContainerCopy()
                                                       .Select(v => v.StartWorking())
                                                       .ToArray();

            return new DisposableAction(() =>
            {
                foreach (var link in linkStartWorking)
                    link.Dispose();

                var result = Interlocked.Decrement(ref this._workingCount);
                OnPropertyChanged(nameof(this.IsWorking));
                OnPropertyChanged(nameof(this.IsNotWorking));

                if (result <= 0)
                    RefreshCommandStatus();
            });
        }

        /// <summary>
        /// Inform the view that a property have changed
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged is null)
                return;

            this.DispatcherProxy.Send(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        #endregion
    }
}
