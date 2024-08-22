namespace CoreLibrary.Services.GenerativeFill;

internal class ArrayOfItemsWithIds<T>(List<T> items) where T : ObjectWithId
{
    public List<T> Items { get; set; } = items;
}
