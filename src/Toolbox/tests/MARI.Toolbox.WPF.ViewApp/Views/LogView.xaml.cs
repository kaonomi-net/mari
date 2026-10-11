namespace MARI.Toolbox.WPF.ViewApp.Views
{
    using System.Windows.Controls;

    public record class DataSampleToGrid(bool IsCheck, IReadOnlyCollection<string> Options);

    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();

            var sampleData = new[]
            {
                new DataSampleToGrid(false, new [] { "Opt1", "opt2", "opt3" }),
                new DataSampleToGrid(false, new [] { "Opt1", "opt2", "opt3" }),
                new DataSampleToGrid(false, new [] { "Opt1", "opt2", "opt3" }),
                new DataSampleToGrid(false, new [] { "Opt1", "opt2", "opt3" }),
                new DataSampleToGrid(false, new [] { "Opt1", "opt2", "opt3" })
            };

            this.DataGrid.ItemsSource = sampleData;
        }
    }
}
