using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Match-3/Item")]
public class Item : ScriptableObject
{
    public enum Types
    {
        NONE,
        SLIME,
        BLUE,
        PURPLE,
        YELLOW,
        GREEN,
        ORANGE,
        WHITE,
    }

    public Types type;
    public int value;
    public Sprite sprite;
    [Range(0f, 2f)] public float weight;
}
