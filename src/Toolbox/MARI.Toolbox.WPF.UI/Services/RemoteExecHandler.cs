// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Services
{
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.WPF.Abstractions.Models;
    using MARI.Toolbox.WPF.Abstractions.Services;

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IRemoteExecHandler" />
    public sealed class RemoteExecHandler : IRemoteExecHandler
    {
        #region Fields

        private readonly IFileSystemHandler _fileSystemHandler;
        private readonly string _executableName;
        private readonly string _toolName;
        private readonly Uri _uri;

        private readonly string _installPath;
        private readonly Uri _execPathUri;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteExecHandler"/> class.
        /// </summary>
        public RemoteExecHandler(Uri uri,
                                 string toolName,
                                 string executableName,
                                 IFileSystemHandler fileSystemHandler)
        {
            this._uri = uri;
            this._toolName = toolName;
            this._executableName = executableName;
            this._fileSystemHandler = fileSystemHandler;

            var installPath = Path.Combine(Directory.GetCurrentDirectory(), ".tools", this._toolName);
            var execPathUri = new Uri(Path.Combine(installPath, this._executableName));

            this._installPath = installPath;
            this._execPathUri = execPathUri;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<RemoteExecResult> ExecuteAsync(CancellationToken token, params string[] arguments)
        {
            await InstallPkgAsync(token);

            var execPath = Path.Combine(this._installPath, this._executableName);

            using var proc = new Process();
            string logs = string.Empty;
            string errors = string.Empty;
            Exception? exception = null;
            int? exitCode = null;
            bool started = false;

            try
            {
                proc.StartInfo.FileName = execPath;

                foreach (var arg in arguments ?? Array.Empty<string>())
                    proc.StartInfo.ArgumentList.Add(arg);

                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;

                token.ThrowIfCancellationRequested();

                if (proc.Start())
                {
                    started = true;

                    // Read both redirected streams concurrently before waiting the exit to prevent pipe buffer dead lock
                    var logsTask = proc.StandardOutput.ReadToEndAsync(token);
                    var errorsTask = proc.StandardError.ReadToEndAsync(token);

                    await proc.WaitForExitAsync(token);

                    logs = await logsTask;
                    errors = await errorsTask;

                    exitCode = proc.ExitCode;
                }

                token.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                exception = ex;

                // Do not leave an orphan process running on cancellation or failure
                if (started)
                {
                    try
                    {
                        if (!proc.HasExited)
                            proc.Kill(true);
                    }
                    catch
                    {
                        // Process may have exited meanwhile
                    }
                }
            }

            // -1 : process not started or not exited normally
            var finalExitCode = exitCode ?? -1;

            return new RemoteExecResult(exception is null && finalExitCode == 0,
                                        logs,
                                        errors,
                                        finalExitCode,
                                        exception?.GetFullString() ?? string.Empty);
        }

        /// <summary>
        ///  Check if tool is installed otherwise install it.
        /// </summary>
        private async Task InstallPkgAsync(CancellationToken token)
        {
            if (this._fileSystemHandler.Exists(this._execPathUri))
                return;

            //TO-DO from Opus 5.5 Downloaded executable run without integrity check : The binary is downloaded (possibly over HTTP) and executed with no hash or signature check. Remaining: verify a checksum/signature, this requires an expected hash source (new constructor parameter or companion file) to be decided.
            var bytes = await GetBytesFromUriAsync(this._uri, token);

            if (bytes.Length == 0)
                throw new InvalidDataException("Empty content downloaded for tool '" + this._toolName + "' from " + this._uri);

            token.ThrowIfCancellationRequested();

            if (ExtractZipFileIfCan(bytes, this._installPath))
                return;

            bool written = false;
            try
            {
                written = await this._fileSystemHandler.WriteToFileAsync(bytes, this._execPathUri, @override: true, token: token);
            }
            finally
            {
                // Never keep a partial executable, otherwise it will never be downloaded again
                if (!written)
                {
                    try
                    {
                        await this._fileSystemHandler.Delete(this._execPathUri);
                    }
                    catch
                    {
                        // Best effort cleanup, keep the original error
                    }
                }
            }

            if (!written)
                throw new IOException("Failed to write tool '" + this._toolName + "' executable to " + this._execPathUri);
        }

        /// <summary>
        /// Extracts the zip file if can.
        /// </summary>
        /// <returns><c>true</c> if <paramref name="bytes"/> is a zip archive that have been extracted; <c>false</c> if it is not a zip archive</returns>
        private static bool ExtractZipFileIfCan(byte[] bytes, string installPath)
        {
            using (var stream = new MemoryStream(bytes))
            {
                ZipArchive zip;
                try
                {
                    zip = new ZipArchive(stream, ZipArchiveMode.Read);
                }
                catch (InvalidDataException)
                {
                    // Not a zip archive
                    return false;
                }

                using (zip)
                {
                    try
                    {
                        zip.ExtractToDirectory(installPath);
                    }
                    catch
                    {
                        // Do not keep a partial extraction
                        try
                        {
                            if (Directory.Exists(installPath))
                                Directory.Delete(installPath, true);
                        }
                        catch
                        {
                            // Best effort cleanup
                        }

                        throw;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the bytes from URI asynchronous.
        /// </summary>
        private async Task<byte[]> GetBytesFromUriAsync(Uri uri, CancellationToken token)
        {
            if (uri.IsFile)
            {
                using (var readStream = this._fileSystemHandler.OpenRead(uri)!)
                {
                    return readStream.ReadAll();
                }
            }

            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(uri, token))
                {
                    result.EnsureSuccessStatusCode();
                    return await result.Content.ReadAsByteArrayAsync(token);
                }
            }
        }

        #endregion
    }
}
