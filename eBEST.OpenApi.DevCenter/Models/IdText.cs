namespace eBEST.OpenApi.DevCenter.Models
{
    internal class IdText
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IdText(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
