// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Abstractions.Services
{
    public interface IFolderPickerService
    {
        /// <summary>
        /// Picks the folder path asynchronous.
        /// </summary>
        ValueTask<string?> PickFolderPathAsync(string? root = null);
    }
}
