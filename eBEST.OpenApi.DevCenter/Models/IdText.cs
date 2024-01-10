namespace eBEST.OpenApi.DevCenter.Models
{
    internal class IdText(int id, string name)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
    }
}
