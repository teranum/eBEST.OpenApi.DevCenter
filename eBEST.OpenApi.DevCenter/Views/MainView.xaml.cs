using eBEST.OpenApi.DevCenter.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void DataGrid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && sender is DataGrid dataGrid)
            //{
            //    if (e.Key == System.Windows.Input.Key.V)
            //    {
            //        if (dataGrid.SelectedCells.Count > 0)
            //        {
            //            var firstCell = dataGrid.SelectedCells[0];
            //            var item = firstCell.Item;
            //            var column = firstCell.Column;
            //            var cellContent = column.GetCellContent(item);
            //            if (cellContent is TextBlock textBlock)
            //            {
            //                string text = Clipboard.GetText();
            //                text = text.Trim(['\r', '\n']);




            //                textBlock.Text = text;
            //            }
            //        }
            //    }
            //}
        }

        private void MenuItem_Click_DataGridCopy(object sender, RoutedEventArgs e)
        {

        }
    }
}