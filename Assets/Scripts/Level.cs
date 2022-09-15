using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Match-3/Level")]
public class Level : ScriptableObject
{
    public Vector2Int gridSize = Vector2Int.zero;
    public Item.Types[] initialCondition = new Item.Types[0];
    public int initialLives => initialCondition.Where((type) => type == Item.Types.SLIME).Count();

    public Item.Types goalType = Item.Types.NONE;
    public int goalAmount = -1;

    public int turnsAmount = -1;

    public int Width => gridSize.x;
    public int Height => gridSize.y;

    public bool isAvailable = false;
    public bool isCompleted = false;

    public Item.Types GetTile(int x, int y) => initialCondition[GetTileIndex(x, y)];
    public Item.Types GetTile(int x, int y, Vector2Int size) => initialCondition[GetTileIndex(x, y, size)];
    public int GetTileIndex(int x, int y) => x + gridSize.x * y;
    public int GetTileIndex(int x, int y, Vector2Int size) => x + size.x * y;

    public void Reset()
    {
        isAvailable = false;
        isCompleted = false;
    }
}