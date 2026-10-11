// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Abstractions.Services
{
    using System.Threading.Tasks;

    /// <summary>
    /// File picker
    /// </summary>
    public interface IFilePickerService
    {
        /// <summary>
        /// Picks the existing file path.
        /// </summary>
        ValueTask<string?> PickExistingFileAsync(params string[] filters);

        /// <summary>
        /// Picks the existing file path.
        /// </summary>
        ValueTask<IReadOnlyCollection<string>?> PickExistingFilesAsync(params string[] filters);
    }
}
