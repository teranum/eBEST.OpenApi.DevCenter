﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace eBEST.OpenApi.DevCenter.Models
{
    internal partial class TabTreeData : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TabTreeData(int id, string name)
        {
            Id = id;
            Name = name;
            Items = [];
            FilterText = string.Empty;
        }

        public string FilterText { get; set; }

        [ObservableProperty]
        public IList<object>? _Items;

        private IList<object>? _OrgItems;
        public IList<object>? OrgItems
        {
            get => _OrgItems;
            set
            {
                if (value != _OrgItems)
                {
                    _OrgItems = value;
                    Items = _OrgItems;
                }
            }
        }
    }
}
