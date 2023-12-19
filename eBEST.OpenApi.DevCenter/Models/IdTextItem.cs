namespace eBEST.OpenApi.DevCenter.Models
{
    public class IdTextItem : IdText
    {
        public IdTextItem(int id, string name) : base(id, name)
        {
            Items = new List<IdTextItem>();
        }

        public IdTextItem? Parent;
        public IList<IdTextItem> Items { get; }
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
