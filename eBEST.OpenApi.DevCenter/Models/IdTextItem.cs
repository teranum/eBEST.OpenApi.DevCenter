namespace eBEST.OpenApi.DevCenter.Models
{
    internal class IdTextItem(int id, string name) : IdText(id, name)
    {
        public IdTextItem? Parent;
        public IList<IdTextItem> Items { get; } = new List<IdTextItem>();
        public void AddChild(IdTextItem item)
        {
            item.Parent = this;
            Items.Add(item);
        }

        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }
        public bool IsActived { get; set; }

        public object? Tag { get; set; }
        public string? Key { get; set; }
    }
}
