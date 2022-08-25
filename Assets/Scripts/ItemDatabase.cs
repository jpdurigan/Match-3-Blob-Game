using UnityEngine;

public static class ItemDatabase
{
    public static Item[] Items { get; private set; }

    private static void Initialize()
    {
        Items = Resources.LoadAll<Item>(path:"Items/");
    }
}
