using System;
using UnityEngine;

// [Serializable]
[CreateAssetMenu(menuName = "Match-3/Level")]
public class Level : ScriptableObject
{
    public Vector2Int gridSize = Vector2Int.zero;

    public Item.Types[] initialCondition = new Item.Types[0];
}