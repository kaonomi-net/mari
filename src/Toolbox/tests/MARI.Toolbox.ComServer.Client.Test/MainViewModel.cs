// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.ComServer.Client.Test
{
    using MARI.Toolbox.Abstractions.Commands;
    using MARI.Toolbox.Abstractions.Proxies;
    using MARI.Toolbox.Communications;
    using MARI.Toolbox.ComServer.Abstraction.Test;
    using MARI.Toolbox.WPF;
    using MARI.Toolbox.WPF.Commands;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public sealed class MainViewModel : BaseViewModel
    {
        #region Fields

        private readonly ObservableCollection<string> _logs;
        private readonly ComApiDescriptor _apiDesc;

        private string _send;
        private string _query;
        private bool _isConnected;
        private ComApiHandler _handler;
        private IReadOnlyCollection<string> _otherClients;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel(IDispatcherProxy dispatcherProxy)
            : base(dispatcherProxy)
        {
            this.ConnectToServerCommand = new AsyncDelegateCommand(dispatcherProxy, ConnectToServerAsync);

            var apiDesc = new ComApiDescriptor();

            apiDesc.AddApiCommandHanlder<ChatMessage>(ChatMessageReceivedAsync)
                   .AddApiRequestDescription<OtherClientsRequest, OtherClientsResponse>();

            this._apiDesc = apiDesc;

            this._logs = new ObservableCollection<string>();
            this.Logs = new ReadOnlyObservableCollection<string>(this._logs);

            this.SendCommand = new AsyncDelegateCommand(dispatcherProxy, SendAsync, _ => this._handler is not null && !string.IsNullOrEmpty(this.Send));
            RegisterCommand(this.SendCommand);

            this.Title = Guid.NewGuid().ToString().Replace("-", "").Substring(10);

            this.RefreshServerListCommand = new AsyncDelegateCommand(dispatcherProxy, RefreshServerListAsync);
        }

        #endregion

        #region Properties

        #region Commands

        public ICommand QueryCommand { get; }

        public ICommandExt SendCommand { get; }

        public ICommand ConnectToServerCommand { get; }

        public ICommand RefreshServerListCommand { get; }

        #endregion

        public string Send
        {
            get { return this._send; }
            set
            {
                if (SetProperty(ref this._send, value))
                    RefreshCommandStatus();
            }
        }

        public string Query
        {
            get { return this._query; }
            set { SetProperty(ref this._query, value); }
        }

        public bool IsConnected
        {
            get { return this._isConnected; }
            private set { SetProperty(ref this._isConnected, value); }
        }

        public IReadOnlyCollection<string> Logs { get; }

        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyCollection<string> Clients
        {
            get { return this._otherClients; }
            private set { SetProperty(ref this._otherClients, value); }
        }

        public string Title { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Chats the message received asynchronous.
        /// </summary>
        private ValueTask ChatMessageReceivedAsync(ChatMessage message, Guid sourceClientId)
        {
            return this.DispatcherProxy.SendAsync(() =>
            {
                if (this.Title == message.From)
                    this._logs.Add(string.Format($"Self - {message.CreateTime:dd/MM/yyyy - HH:mm} - {message.Message}"));
                else
                    this._logs.Add(string.Format($"[{message.From}] - {message.CreateTime:dd/MM/yyyy - HH:mm} - {message.Message}"));
            });
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        private async ValueTask SendAsync()
        {
            var msg = this.Send;
            this.Send = string.Empty;

            var chatMsg = new ChatMessage(this.Title, msg, DateTime.Now);

            await this._handler.SendCommand(chatMsg);

            await ChatMessageReceivedAsync(chatMsg, Guid.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        private async ValueTask RefreshServerListAsync()
        {
            using (StartWorking())
            {
                try
                {
                    var result = await this._handler.SendRequest<OtherClientsRequest, OtherClientsResponse>(new OtherClientsRequest());

                    this.Clients = result.Clients;
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// Connects to server asynchronous.
        /// </summary>
        private async ValueTask ConnectToServerAsync()
        {
            this._handler = await ComApiHandler.ConnectAsync(IPAddress.Loopback, 4242, this._apiDesc, "Client " + this.Title);
            this.IsConnected = true;

            RefreshCommandStatus();
        }

        #endregion
    }
}
