// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Services
{
    using MARI.Toolbox.WPF.Abstractions.Services;

    using Microsoft.Win32;

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Default file windows file system picker
    /// </summary>
    /// <seealso cref="IFilePickerService" />
    public sealed class FileSystemPickerService : IFilePickerService
    {
        #region Methods

        /// <inheritdoc />
        public ValueTask<string?> PickExistingFileAsync(params string[] filters)
        {
            var filePicker = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false,
            };

            if (filters is not null && filters.Any())
                filePicker.Filter = string.Join("|", filters);

            if (filePicker.ShowDialog() == true && !string.IsNullOrEmpty(filePicker.FileName))
                return ValueTask.FromResult<string?>(filePicker.FileName);

            return ValueTask.FromResult<string?>(null);
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyCollection<string>?> PickExistingFilesAsync(params string[] filters)
        {
            var filePicker = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = true,
            };

            if (filters is not null && filters.Any())
                filePicker.Filter = string.Join("|", filters);

            if (filePicker.ShowDialog() == true && filePicker.FileNames is not null && filePicker.FileNames.Any(f => !string.IsNullOrEmpty(f)))
                return ValueTask.FromResult<IReadOnlyCollection<string>?>(filePicker.FileNames);

            return ValueTask.FromResult<IReadOnlyCollection<string>?>(null);
        }

        #endregion
    }
}
