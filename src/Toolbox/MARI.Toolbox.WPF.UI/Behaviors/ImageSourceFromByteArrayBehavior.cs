// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Behaviors
{
    using Microsoft.Xaml.Behaviors;

    using System.IO;
    using System.Windows;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Behavior used to fill an <see cref="Image"/> from a byte array
    /// </summary>
    public sealed class ImageSourceFromByteArrayBehavior : Behavior<System.Windows.Controls.Image>
    {
        #region Fields

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data),
                                                                                             typeof(IList<byte>),
                                                                                             typeof(ImageSourceFromByteArrayBehavior),
                                                                                             new PropertyMetadata(null, ImageDataSourceChanged));
        private MemoryStream? _dataStream;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public IList<byte>? Data
        {
            get { return (IList<byte>?)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void OnAttached()
        {
            UpdateImageSource();
            base.OnAttached();
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            this.AssociatedObject.Source = null;
            base.OnDetaching();
        }

        /// <summary>
        /// Images the data source changed.
        /// </summary>
        private static void ImageDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inst = d as ImageSourceFromByteArrayBehavior;

            if (inst is not null)
            {
                if (inst._dataStream is not null)
                {
                    if (inst.AssociatedObject is not null)
                        inst.AssociatedObject.Source = null;
                    inst._dataStream.Dispose();
                    inst._dataStream = null;
                }

                if (e.NewValue is not null)
                    inst._dataStream = new MemoryStream(e.NewValue as byte[] ?? ((IEnumerable<byte>)e.NewValue).ToArray());
            }

            (d as ImageSourceFromByteArrayBehavior)?.UpdateImageSource();
        }

        /// <summary>
        /// Updates the image source.
        /// </summary>
        private void UpdateImageSource()
        {
            if (this.AssociatedObject is null)
                return;

            if (this.Data is null || this.Data.Count == 0)
            {
                this.AssociatedObject.Source = null;
                return;
            }

            this.Dispatcher.BeginInvoke(() =>
            {

                var stream = this._dataStream;
                lock (this.AssociatedObject)
                {
                    if (stream != this._dataStream)
                        return;

                    var imageSource = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                    this.AssociatedObject.BeginInit();
                    this.AssociatedObject.Source = imageSource;
                    this.AssociatedObject.EndInit();
                }
            });
        }

        #endregion
    }
}
