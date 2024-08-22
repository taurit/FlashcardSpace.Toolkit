namespace CoreLibrary.Services.ObjectGenerativeFill;

internal class ArrayOfItemsWithIds<T>(List<T> items) where T : ObjectWithId
{
    public List<T> Items { get; set; } = items;
}
