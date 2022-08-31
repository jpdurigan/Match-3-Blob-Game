using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item.Types _type = Item.Types.NONE;
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

            Sprite sprite = ItemDatabase.GetItemSprite(_type);
            if (sprite != null) _icon.sprite = sprite;
            else _icon.sprite = emptySprite;

            OnTypeChanged();
            // _icon.sprite = ItemDatabase.GetItemSprite(_type);
        }
    }

    private Image _icon;
    public Image icon
    {
        get
        {
            return _icon;
        }
        
        set
        {
            _icon = value;
            button.targetGraphic = _icon;
        }
    }
    private Button button;
    [SerializeField] private Sprite emptySprite;

    public Tile Left;
    public Tile Right;
    public Tile Top;
    public Tile Bottom;
    public Tile[] Neighbours;
    private List<Tile> ConnectedTiles = null;

    private bool wasInitialized = false;


    private void Start()
    {
        button = GetComponent<Button>();
        _icon = GetComponentsInChildren<Image>()[1];
        button.onClick.AddListener(() => Board.Instance.Select(this));
    }

    public void Initialize()
    {
        Left = Board.Instance.GetTile(x - 1, y);
        Right = Board.Instance.GetTile(x + 1, y);
        Top = Board.Instance.GetTile(x, y - 1);
        Bottom = Board.Instance.GetTile(x, y + 1);
        Neighbours = new[]
        {
            Left,
            Top,
            Right,
            Bottom,
        };
        Type = Item.Types.NONE;

        wasInitialized = true;
    }

    public List<Tile> GetConnectedTiles()
    {
        if (ConnectedTiles == null) UpdateConnectedTilesRecursive();
        return ConnectedTiles;
    }

    private void OnTypeChanged()
    {
        UpdateConnectedTilesRecursive();
        foreach(Tile neighbour in Neighbours)
        {
            if (neighbour == null || ConnectedTiles.Contains(neighbour)) continue;
            neighbour.UpdateConnectedTilesRecursive();
        }
    }

    private void UpdateConnectedTilesRecursive(List<Tile> tiles = null)
    {
        if (tiles == null) tiles = new List<Tile>();

        ConnectedTiles = tiles;
        foreach(Tile tile in Neighbours)
        {
            if (tile == null) continue;
            if (tiles.Contains(tile)) continue;
            if (tile.Type != Type) continue;

            tiles.Add(tile);
            tile.UpdateConnectedTilesRecursive(tiles);
        }
    }

    public bool IsNone()
    {
        return Type == Item.Types.NONE;
    }
}
