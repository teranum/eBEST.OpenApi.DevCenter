using eBEST.OpenApi.DevCenter.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace eBEST.OpenApi.DevCenter.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void DataGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.EditingElement is TextBox textBox)
                {
                    textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                }
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.OnDataGridCellEditEnding();
                }
            }
        }
    }
}