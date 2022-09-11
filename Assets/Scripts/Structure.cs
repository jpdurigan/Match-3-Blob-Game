using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structure : Object
{
    public List<Tile> Tiles = null;
    private List<Tile> horizontalConnection = null;
    private List<Tile> verticalConnection = null;
    private List<Tile> squareConnection = null;

    public void Add(Tile tile)
    {
        if (Has(tile)) return;
        if (Tiles == null) Tiles = new List<Tile>();
        Tiles.Add(tile);

        if (tile.IsSlime()) return;

        if (tile.HasHorizontalConnection())
        {
            List<Tile> tileConnection = tile.GetHorizontalConnection();
            AddList(tileConnection);
            if (
                horizontalConnection == null
                || horizontalConnection.Count < tileConnection.Count
            ) horizontalConnection = tileConnection;
        }
        
        if (tile.HasVerticalConnection())
        {
            List<Tile> tileConnection = tile.GetVerticalConnection();
            AddList(tileConnection);
            if (
                verticalConnection == null
                || verticalConnection.Count < tileConnection.Count
            ) verticalConnection = tileConnection;
        }
        
        if (tile.HasSquareConnection())
        {
            List<Tile> tileConnection = tile.GetSquareConnection();
            AddList(tileConnection);
            if (
                squareConnection == null
                || squareConnection.Count < tileConnection.Count
            ) squareConnection = tileConnection;
        }
    }

    public void AddList(List<Tile> tiles)
    {
        foreach(Tile tile in tiles) Add(tile);
    }

    public bool Has(Tile tile)
    {
        return Tiles != null && Tiles.Contains(tile);
    }

    public Item.Types GetBomb()
    {
        if (HasSquareBomb()) return Item.Types.BOMB_SQUARE;
        if (HasHorizontalBomb()) return Item.Types.BOMB_HORIZONTAL;
        if (HasVerticalBomb()) return Item.Types.BOMB_VERTICAL;
        return Item.Types.NONE;
    }

    private bool HasHorizontalConnection() => horizontalConnection != null && horizontalConnection.Count >= 3;
    private bool HasVerticalConnection() => verticalConnection != null && verticalConnection.Count >= 3;
    private bool HasSquareConnection() => squareConnection != null;

    private bool HasSquareBomb() => HasSquareConnection() || (HasHorizontalConnection() && HasVerticalConnection());
    private bool HasHorizontalBomb() => HasVerticalConnection() && verticalConnection.Count >= 4;
    private bool HasVerticalBomb() => HasHorizontalConnection() && horizontalConnection.Count >= 4;
    public bool HasBomb() => HasSquareBomb() || HasHorizontalBomb() || HasVerticalBomb();
}
