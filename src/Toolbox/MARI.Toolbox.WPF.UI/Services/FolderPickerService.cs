// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Services
{
    using MARI.Toolbox.WPF.Abstractions.Services;

    using Microsoft.Win32;

    using System.Threading.Tasks;

    public sealed class FolderPickerService : IFolderPickerService
    {
        #region Methods

        /// <inheritdoc />
        public ValueTask<string?> PickFolderPathAsync(string? root = null)
        {
            var openFolderDialog = new OpenFolderDialog()
            {
                InitialDirectory = root,
            };

            if (openFolderDialog.ShowDialog() == true)
                return ValueTask.FromResult((string?)openFolderDialog.FolderName);

            return ValueTask.FromResult((string?)null);
        }

        #endregion
    }
}
