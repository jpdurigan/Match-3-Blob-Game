using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
#endif
public static class ItemDatabase
{
    public static Item[] Items { get; private set; }

    private static Dictionary<Item.Types, Item> database;
    private static float itemsTotalWeight = 0f;

    static ItemDatabase()
    {
        Initialize();
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Items = Resources.LoadAll<Item>(path:"Items/").OrderBy(i => i.weight).ToArray();
        database = new Dictionary<Item.Types, Item>();
        foreach(Item item in Items)
        {
            itemsTotalWeight += item.weight;
            database.Add(item.type, item);
        }
    }

    public static Item GetRandomItem()
    {
        Item randomItem = null;
        float randomWeight = Random.Range(0f, itemsTotalWeight);
        foreach(Item item in Items)
        {
            if (item.weight <= Mathf.Epsilon) continue;
            
            randomWeight -= item.weight;
            if (randomWeight <= 0)
            {
                randomItem = item;
                break;
            }
        }
        return randomItem;
    }

    public static int GetItemValue(Item.Types type)
    {
        if (!database.ContainsKey(type)) return 0;
        return database[type].value;
    }

    public static Sprite GetItemSprite(Item.Types type)
    {
        if (!database.ContainsKey(type)) return null;
        return database[type].sprite;
    }

    public static Texture GetItemTexture(Item.Types type)
    {
        if (!database.ContainsKey(type)) return null;
        return database[type].texture;
    }

}
