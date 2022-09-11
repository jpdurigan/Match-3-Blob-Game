using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SpecialItem
{
    public static List<Tile> GetDamage(Tile tile)
    {
        List<Tile> tiles = new List<Tile>();
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
        return tiles;
    }
}
