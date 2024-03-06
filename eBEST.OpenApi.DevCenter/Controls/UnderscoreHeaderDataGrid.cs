using System.Windows.Controls;

namespace eBEST.OpenApi.DevCenter.Controls
{
    public class UnderscoreHeaderDataGrid : DataGrid
    {
        public UnderscoreHeaderDataGrid()
        {
            AutoGeneratingColumn += (sender, e) =>
            {
                if (e.PropertyName.Contains("_"))
                {
                    e.Column.Header = e.PropertyName.Replace("_", "__");
                }
            };
        }
    }
}
