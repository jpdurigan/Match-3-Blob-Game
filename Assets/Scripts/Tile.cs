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

            if (wasInitialized) OnTypeChanged();

            // update visuals
            if (_type == Item.Types.SLIME) return; // slimes are updated every round
            Sprite sprite = ItemDatabase.GetItemSprite(_type);
            if (sprite != null) _icon.sprite = sprite;
            else _icon.sprite = emptySprite;

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
    public Image eyes;

    [HideInInspector] public Tile TopLeft;
    [HideInInspector] public Tile Top;
    [HideInInspector] public Tile TopRight;
    [HideInInspector] public Tile Left;
    [HideInInspector] public Tile Right;
    [HideInInspector] public Tile BottomLeft;
    [HideInInspector] public Tile Bottom;
    [HideInInspector] public Tile BottomRight;
    [HideInInspector] public Tile[] Neighbours;
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
        TopLeft =       Board.Instance.GetTile(x - 1, y - 1);
        Top =           Board.Instance.GetTile(x    , y - 1);
        TopRight =      Board.Instance.GetTile(x + 1, y - 1);
        Left =          Board.Instance.GetTile(x - 1, y);
        Right =         Board.Instance.GetTile(x + 1, y);
        BottomLeft =    Board.Instance.GetTile(x - 1, y + 1);
        Bottom =        Board.Instance.GetTile(x    , y + 1);
        BottomRight =   Board.Instance.GetTile(x + 1, y + 1);

        Neighbours = new[]{ Left, Top, Right, Bottom };

        Type = Item.Types.NONE;

        wasInitialized = true;
    }

    public List<Tile> GetConnectedTiles()
    {
        if (ConnectedTiles == null) UpdateConnectedTilesRecursive();
        return ConnectedTiles;
    }

    public bool ShouldDestroy()
    {
        if (IsSlime()) return false;
        bool shouldDestroy = false;

        // has 3 horizontal
        shouldDestroy = (
            ConnectedTiles.Contains(Left)
            && ConnectedTiles.Contains(Right)
        );
        if (shouldDestroy) return shouldDestroy;

        // has 3 vertical
        shouldDestroy = (
            ConnectedTiles.Contains(Top)
            && ConnectedTiles.Contains(Bottom)
        );
        if (shouldDestroy) return shouldDestroy;

        // has 4 square
        shouldDestroy = (
            ConnectedTiles.Contains(Bottom)
            && ConnectedTiles.Contains(Right)
            && ConnectedTiles.Contains(BottomRight)
        );
        if (shouldDestroy) return shouldDestroy;

        return shouldDestroy;
    }

    public void UpdateVisual()
    {
        eyes.enabled = false;
        button.enabled = true;
        if (!IsSlime()) return;

        _icon.sprite = Slime.Instance.GetSprite(this);
        button.enabled = false;
    }

    public void OnUpdatingGrid()
    {
        if (!IsSlime()) return;

        _icon.sprite = Slime.Instance.GetDefaultSprite();
        eyes.enabled = true;
    }

    public void ShowEyes()
    {
        eyes.enabled = true;
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
        if (!tiles.Contains(this)) tiles.Add(this);
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

    public bool IsSlime()
    {
        return Type == Item.Types.SLIME;
    }

    public bool Is(Item.Types type)
    {
        return Type == type;
    }

    public bool IsNeighbouringSlime()
    {
        bool isNeighbour = false;
        foreach(Tile neighbour in Neighbours)
        {
            if (neighbour != null && neighbour.IsSlime())
            {
                isNeighbour = true;
                break;
            }
        }
        return isNeighbour;
    }

    public Vector2 GetVector2()
    {
        return new Vector2((float)x, (float)y);
    }
}
