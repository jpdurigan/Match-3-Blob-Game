using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SpecialItem
{
    public static List<Tile> GetGrowthAffectedTiles(Structure structure)
    {
        List<Tile> growthTiles = new List<Tile>();
        foreach(Tile tile in structure.Tiles)
        {
            if (tile.IsNeighbouringSlime()) growthTiles.Add(tile);
        }
        return growthTiles;
    }

    public static List<Tile> GetDeathAffectedTiles(Structure structure)
    {
        List<Tile> deathTiles = new List<Tile>();
        foreach(Tile tile in structure.Tiles)
        {
            foreach(Tile neighbour in tile.Neighbours)
            {
                if (neighbour != null && neighbour.IsSlime()) deathTiles.Add(neighbour);
            }
        }
        return deathTiles;
    }

    public static List<Tile> GetDamage(Tile tile, List<Tile> exclude = null)
    {
        List<Tile> tiles = new List<Tile>();
        if (exclude == null) exclude = new List<Tile>();
        Board board = Board.Instance;
        switch (tile.Type)
        {
            case Item.Types.BOMB_HORIZONTAL:
                // todos com mesmo Y
                for (int x=0; x<board.Width; x++) tiles.Add(board.GetTile(x, tile.y));
                break;
            case Item.Types.BOMB_VERTICAL:
                // todos com mesmo X
                for (int y=0; y<board.Height; y++) tiles.Add(board.GetTile(tile.x, y));
                break;
            case Item.Types.BOMB_SQUARE:
                tiles = new List<Tile>{
                    tile.TopLeft, tile.Top, tile.TopRight,
                    tile.Left, tile, tile.Right,
                    tile.BottomLeft, tile.Bottom, tile.BottomRight
                };
                break;
        }
        tiles.RemoveAll((t) => t == null || t.IsBlock() || exclude.Contains(t));
        for(int i = 0; i < tiles.Count(); i++)
        {
            Tile t = tiles[i];
            if (t.IsBomb()) Tile.AddListToList(tiles, GetDamage(t, tiles));
        }
        return tiles;
    }
}
