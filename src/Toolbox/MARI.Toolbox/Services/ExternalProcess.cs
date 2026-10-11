// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Services
{
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Disposables;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Proxy pilot about a external execution process
    /// </summary>
    /// <seealso cref="IExternalProcess" />
    internal sealed class ExternalProcess : ExternalBaseProcess, IExternalProcess
    {
        #region Fields

        private readonly ProcessStartInfo _info;
        
        private Process? _process;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalProcess"/> class.
        /// </summary>
        public ExternalProcess(ProcessStartInfo info,
                               IReadOnlyCollection<string> arguments,
                               CancellationToken cancellationToken)
            : base(arguments, cancellationToken)
        {
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = false;

            this._info = info;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Executable
        {
            get { return this._info.FileName; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override Task KillAsync(CancellationToken cancellationToken)
        {
            Process? process;
            lock (this._info)
            {
                process = this._process;
            }

            if (process == null && this.ExitCode == null)
                throw new InvalidOperationException("Run process first");

            if (process == null)
                return Task.CompletedTask;

            return Task.Run(() => process.Kill(true), cancellationToken);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return this._info?.ToString() ?? this.Executable;
        }

        /// <inheritdoc />
        protected override void OnDisposeThreadSafeBegin()
        {
            Process? process;
            lock (this._info)
            {
                process = this._process;
            }

            try
            {
                process?.Kill(true);
            }
            catch (InvalidOperationException)
            {
                // Process already exited and disposed
            }
        }

        /// <summary>
        /// Start configured process
        /// </summary>
        protected override Task OnRunAsync(CancellationToken cancellationToken)
        {
            try
            {
                lock (this._info)
                {
                    if (this._process != null)
                        throw new InvalidOperationException("Could not start a running process");

                    var process = new Process()
                    {
                        StartInfo = this._info
                    };

                    //process.Exited += Process_Exited;
                    process.OutputDataReceived += Process_OutputDataReceived;
                    process.ErrorDataReceived += Process_ErrorDataReceived;

                    process.EnableRaisingEvents = true;

                    // Start only once
                    if (!process.Start())
                    {
                        process.Dispose();
                        throw new InvalidOperationException("Process couldn't start");
                    }

                    this._process = process;

                    this._process.BeginOutputReadLine();
                    this._process.BeginErrorReadLine();

                    var processTask = this._process.WaitForExitAsync(cancellationToken);

                    processTask = processTask.ContinueWith(t =>
                    {
                        int? exitCode = null;
                        try
                        {
                            // On cancellation WaitForExitAsync ends while the process may still be running
                            if (!process.HasExited)
                            {
                                process.Kill(true);
                                process.WaitForExit(5000);
                            }

                            exitCode = process.ExitCode;
                        }
                        catch (Exception)
                        {
                            // Process state could not be retrieved, ExitCode is set to -1
                        }
                        finally
                        {
                            lock (this._info)
                            {
                                this.ExitCode = exitCode ?? -1;

                                if (ReferenceEquals(this._process, process))
                                    this._process = null;
                            }

                            process.Dispose();
                        }

                        // Propagate the cancellation or failure of the waiting
                        t.GetAwaiter().GetResult();
                    }, TaskScheduler.Default);

                    base.SetProcessWaitingTask(processTask);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add(nameof(ProcessStartInfo), this._info);
                throw;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the OutputDataReceived event of the Process control.
        /// </summary>
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            base.ProcessOutputDataReceived(e?.Data);
        }

        /// <summary>
        /// Handles the ErrorDataReceived event of the Process control.
        /// </summary>
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            base.ProcessErrorDataReceived(e?.Data);
        }

        #endregion
    }
}
