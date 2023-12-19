namespace eBEST.OpenApi.DevCenter.Models;

internal class BlockRecord(string Name, string DescName, IList<object> BlockDatas)
{
    public double Height { get; set; } = double.NaN;
    public string Name = Name;
    public string DescName { get; } = DescName;
    public IList<object> BlockDatas { get; } = BlockDatas;
}