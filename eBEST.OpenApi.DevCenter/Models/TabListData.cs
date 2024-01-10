using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace eBEST.OpenApi.DevCenter.Models
{
    internal partial class TabListData(string name) : ObservableObject
    {
        [ObservableProperty]
        int _Id;

        public string Name { get; set; } = name;
        public IList<string> Items { get; set; } = new ObservableCollection<string>();
    }
}
