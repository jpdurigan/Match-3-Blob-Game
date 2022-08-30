using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item.Types _type;
    public Item.Types Type
    {
        get
        {
            return _type;
        }

        set
        {
            if (_type == value) return;
            _type = value;
            icon.sprite = ItemDatabase.GetItemSprite(_type);
        }
    }

    public Image icon;
    public Button button;

    public Tile Left => Board.Instance.GetTile(x - 1, y);
    public Tile Right => Board.Instance.GetTile(x + 1, y);
    public Tile Top => Board.Instance.GetTile(x, y - 1);
    public Tile Bottom => Board.Instance.GetTile(x, y + 1);
    public Tile[] Neighbours => new[]
    {
        Left,
        Top,
        Right,
        Bottom,
    };


    private void Start()
    {
        button.onClick.AddListener(() => Board.Instance.Select(this));
    }

    public List<Tile> GetConnectedTiles(List<Tile> tiles = null)
    {
        if (tiles == null) tiles = new List<Tile>();

        foreach(Tile tile in Neighbours)
        {
            if (tile == null) continue;
            if (tiles.Contains(tile)) continue;
            if (tile.Type != Type) continue;

            tiles.Add(tile);
            tile.GetConnectedTiles(tiles);
        }
        
        return tiles;
    }

    public bool IsNone()
    {
        return Type == Item.Types.NONE;
    }
}
