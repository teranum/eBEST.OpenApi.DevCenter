using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace eBEST.OpenApi.DevCenter.Models
{
    internal partial class TabListData : ObservableObject
    {
        [ObservableProperty]
        int _Id;

        public string Name { get; set; }
        public IList<string> Items { get; set; }
        public TabListData(string name)
        {
            this.Name = name;
            Items = new ObservableCollection<string>();
        }
    }
}
