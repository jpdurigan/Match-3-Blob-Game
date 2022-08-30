using System.Linq;
using UnityEngine;

public static class ItemDatabase
{
    public static Item[] Items { get; private set; }

    private static float itemsTotalWeight = 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Items = Resources.LoadAll<Item>(path:"Items/").OrderBy(i => i.weight).ToArray();
        foreach(Item item in Items)
        {
            Debug.Log(item.weight);
            itemsTotalWeight += item.weight;
        }
    }

    public static Item GetRandomItem()
    {
        Item randomItem = null;
        float randomWeight = Random.Range(0f, itemsTotalWeight);
        foreach(Item item in Items)
        {
            randomWeight -= item.weight;
            if (randomWeight <= 0)
            {
                randomItem = item;
                break;
            }
        }
        return randomItem;
    }
}
