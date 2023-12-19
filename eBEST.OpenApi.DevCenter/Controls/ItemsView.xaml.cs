using eBEST.OpenApi.DevCenter.Models;
using System.Windows;
using System.Windows.Controls;

namespace eBEST.OpenApi.DevCenter.Controls
{
    /// <summary>
    /// Interaction logic for ItemsView.xaml
    /// </summary>
    public partial class ItemsView : TabControl
    {
        public object? SelectedTreeItem
        {
            get { return (object?)GetValue(SelectedTreeItemProperty); }
            set { SetValue(SelectedTreeItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedTreeItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTreeItemProperty =
            DependencyProperty.Register("SelectedTreeItem", typeof(object), typeof(ItemsView), new PropertyMetadata(propertyChangedCallback: null));

        // Declare a public get accessor.

        public ItemsView()
        {
            InitializeComponent();
        }

        private void FilterModeClicked(object sender, RoutedEventArgs e)
        {
            filterMode.IsChecked = !filterMode.IsChecked;
            Filter();
        }

        private void FilterClicked(object sender, RoutedEventArgs e) => Filter();

        private async void Filter()
        {
            if (root.SelectedItem is TabTreeData seltab)
            {
                string FilterText = seltab.FilterText;
                if (FilterText.Length == 0)
                {
                    seltab.Items = seltab.OrgItems;
                    return;
                }

                var orglistItems = seltab.OrgItems;
                if (orglistItems == null || orglistItems.Count == 0)
                {
                    return;
                }

                bool bOnlyNode = filterMode.IsChecked;

                var task = Task.Run(() =>
                {
                    List<object> newlistItems = [];
                    foreach (var orgItem in orglistItems)
                    {
                        if (orgItem is IdTextItem imagetitle)
                        {
                            IdTextItem? finded;
                            if (bOnlyNode)
                                finded = FindMatchedItemOnlyNode(imagetitle, FilterText);
                            else
                                finded = FindMatchedItem(imagetitle, FilterText);
                            if (finded != null)
                                newlistItems.Add(finded);
                        }
                    }
                    return newlistItems;
                });
                seltab.Items = await task.ConfigureAwait(true);
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetValue(SelectedTreeItemProperty, e.NewValue);
        }

        // sub functions
        static IdTextItem? FindMatchedItemOnlyNode(IdTextItem orgitem, string text)
        {
            IdTextItem? me = null;

            if (orgitem.Items.Count > 0)
            {
                foreach (var childitem in orgitem.Items)
                {
                    if (childitem is IdTextItem imagetitle)
                    {
                        IdTextItem? finded = FindMatchedItemOnlyNode(imagetitle, text);
                        if (finded != null)
                        {
                            me ??= new IdTextItem(orgitem.Id, orgitem.Name)
                            {
                                Tag = orgitem.Tag,
                                Key = orgitem.Key,
                                IsExpanded = true,
                                IsActived = orgitem.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0,
                            };
                            me.AddChild(finded);
                        }
                    }
                }
            }

            if (me == null)
            {
                if (orgitem.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    me = new IdTextItem(orgitem.Id, orgitem.Name)
                    {
                        Tag = orgitem.Tag,
                        Key = orgitem.Key,
                        IsExpanded = true,
                        IsActived = true,
                    };
                }
            }
            return me;
        }

        static IdTextItem? FindMatchedItem(IdTextItem orgitem, string text)
        {
            IdTextItem? me = null;

            if (orgitem.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                me = CopyItem(orgitem);
                me.IsActived = true;
            }

            if (me == null && orgitem.Items.Count > 0)
            {
                foreach (var childitem in orgitem.Items)
                {
                    if (childitem is IdTextItem imagetitle)
                    {
                        IdTextItem? finded = FindMatchedItem(imagetitle, text);
                        if (finded != null)
                        {
                            me ??= new IdTextItem(orgitem.Id, orgitem.Name)
                            {
                                IsExpanded = true,
                                Tag = orgitem.Tag,
                                Key = orgitem.Key,
                            };
                            me.AddChild(finded);
                        }
                    }
                }
            }

            return me;
        }

        static IdTextItem CopyItem(IdTextItem orgitem)
        {
            var newItem = new IdTextItem(orgitem.Id, orgitem.Name) { Tag = orgitem.Tag, Key = orgitem.Key };
            foreach (var childitem in orgitem.Items)
            {
                if (childitem is IdTextItem item)
                {
                    newItem.AddChild(CopyItem(item));
                }
            }
            return newItem;
        }
    }
}
