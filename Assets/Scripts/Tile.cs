using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public int Length => allConnections != null ? allConnections.Count : 0;

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
    [HideInInspector] public Tile[] HorizontalNeighbours;
    [HideInInspector] public Tile[] VerticalNeighbours;
    [HideInInspector] public Tile[][] SquareMasks;

    private List<Tile> allConnections = null;
    private List<Tile> horizontalConnection = null;
    private List<Tile> verticalConnection = null;
    private List<Tile> squareConnection = null;

    private bool wasInitialized = false;


    private void Start()
    {
        button = GetComponent<Button>();
        _icon = GetComponentsInChildren<Image>()[1];
        button.onClick.AddListener(() => Board.Instance.Select(this));
        button.enabled = false;
        eyes.enabled = false;
        Type = Item.Types.NONE;
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
        HorizontalNeighbours = new[]{ Left, Right };
        VerticalNeighbours = new[]{ Top, Bottom };
        SquareMasks = new Tile[][]{
            new[]{ TopLeft, Top, Left },
            new[]{ TopRight, Top, Right },
            new[]{ BottomLeft, Bottom, Left },
            new[]{ BottomRight, Bottom, Right },
        };

        Type = Item.Types.NONE;

        wasInitialized = true;
    }

    public Structure GetStructure()
    {
        Structure structure = new Structure();
        structure.Add(this);
        return structure;
    }

    public List<Tile> GetAllConnections()
    {
        return allConnections;
    }

    public bool ShouldDestroy()
    {
        if (IsSlime()) return false;
        bool shouldDestroy = false;

        // has 3 horizontal
        shouldDestroy = HasHorizontalConnection();
        if (shouldDestroy) return shouldDestroy;

        // has 3 vertical
        shouldDestroy = HasVerticalConnection();
        if (shouldDestroy) return shouldDestroy;

        // has square
        shouldDestroy = HasSquareConnection();
        if (shouldDestroy) return shouldDestroy;

        return shouldDestroy;
    }

    public void ResetConnections()
    {
        allConnections = null;
        horizontalConnection = null;
        verticalConnection = null;
        squareConnection = null;
    }

    public void UpdateConnections()
    {
        if( allConnections == null ) UpdateAllConnectionsRecursive();
        if( horizontalConnection == null ) UpdateHorizontalConnectionRecursive();
        if( verticalConnection == null ) UpdateVerticalConnectionRecursive();
        if( squareConnection == null ) UpdateSquareConnection();
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
        ResetConnections();
        foreach(Tile tile in Neighbours) if (tile != null) tile.ResetConnections();
        UpdateConnections();
        foreach(Tile tile in Neighbours) if (tile != null) tile.UpdateConnections();
    }

    private void UpdateAllConnectionsRecursive(List<Tile> tiles = null)
    {
        if (tiles == null) tiles = new List<Tile>();

        allConnections = tiles;
        if (!tiles.Contains(this)) tiles.Add(this);
        foreach(Tile tile in Neighbours)
        {
            if (tile == null || tiles.Contains(tile) || tile.Type != Type) continue;
            tiles.Add(tile);
            tile.UpdateAllConnectionsRecursive(tiles);
        }
    }

    private void UpdateHorizontalConnectionRecursive(List<Tile> tiles = null)
    {
        if (tiles == null) tiles = new List<Tile>();
        horizontalConnection = tiles;
        if (!tiles.Contains(this)) tiles.Add(this);
        foreach(Tile tile in HorizontalNeighbours)
        {
            if (tile == null || tiles.Contains(tile) || tile.Type != Type) continue;
            tiles.Add(tile);
            tile.UpdateHorizontalConnectionRecursive(tiles);
        }
    }

    private void UpdateVerticalConnectionRecursive(List<Tile> tiles = null)
    {
        if (tiles == null) tiles = new List<Tile>();
        verticalConnection = tiles;
        if (!tiles.Contains(this)) tiles.Add(this);
        foreach(Tile tile in VerticalNeighbours)
        {
            if (tile == null || tiles.Contains(tile) || tile.Type != Type) continue;
            tiles.Add(tile);
            tile.UpdateVerticalConnectionRecursive(tiles);
        }
    }

    private void UpdateSquareConnection(List<Tile> tiles = null)
    {
        if (tiles != null)
        {
            squareConnection = tiles;
            return;
        }

        bool isSquare = false;
        foreach(Tile[] squareMask in SquareMasks)
        {
            isSquare = true;
            foreach(Tile tile in squareMask)
            {
                if (tile != null && tile.Is(Type)) continue;
                isSquare = false;
                break;
            }

            if (isSquare)
            {        
                tiles = new List<Tile>();
                tiles.Add(this);
                tiles.AddRange(squareMask);
                squareConnection = tiles;
                foreach(Tile tile in squareMask)
                {
                    UpdateSquareConnection(tiles);
                }
            }
        }
    }

    public bool IsNone()
    {
        return Is(Item.Types.NONE);
    }
    
    public bool IsSlime()
    {
        return Is(Item.Types.SLIME);
    }
    
    public bool IsBomb()
    {
        return Is(Item.Types.BOMB_HORIZONTAL) || Is(Item.Types.BOMB_VERTICAL) || Is(Item.Types.BOMB_SQUARE);
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

    public bool HasHorizontalConnection() => horizontalConnection != null && horizontalConnection.Count >= 3;
    public bool HasVerticalConnection() => verticalConnection != null && verticalConnection.Count >= 3;
    public bool HasSquareConnection() => squareConnection != null;
    public List<Tile> GetHorizontalConnection() => horizontalConnection;
    public List<Tile> GetVerticalConnection() => verticalConnection;
    public List<Tile> GetSquareConnection() => squareConnection;

    public Vector2 GetVector2()
    {
        return new Vector2((float)x, (float)y);
    }

    public static void AddListToList(List<Tile> listA, List<Tile> listB)
    {
        if (listA == null || listB == null) return;
        foreach(Tile tile in listB)
        {
            if (listA.Contains(tile)) continue;
            listA.Add(tile);
        }
    }
}
