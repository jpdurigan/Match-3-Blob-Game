using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;

    public Row[] rows;

    public Tile[,] Tiles { get; private set; }

    public int Width => Tiles.GetLength(dimension:0);
    public int Height => Tiles.GetLength(1);

    private List<Tile> _selection = new List<Tile>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Tiles = new Tile[rows[0].tiles.Length, rows.Length];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                Tile tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;
                Tiles[x, y] = tile;
            }
        }
        foreach(Tile tile in Tiles)
        {
            tile.Initialize();
        }

        #pragma warning disable CS4014
        FillBlanks();
        #pragma warning restore CS4014
    }

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
        return Tiles[x, y];
    }

    public async void Select(Tile tile)
    {
        if (_selection.Contains(tile))
        {
            _selection.Remove(tile);
            await Animate.AsyncDeselect(tile);
            return;
        }

        if (_selection.Count == 1)
        {
            Tile selectedTile = _selection[0];
            bool isValidSelect = selectedTile.Neighbours.Contains(tile);
            if (!isValidSelect) return;
        }

        _selection.Add(tile);
        await Animate.AsyncSelect(tile);

        if (_selection.Count < 2) return;

        await AsyncSwap(_selection[0], _selection[1]);

        if (CanPop())
        {
            await Pop();
        }
        else
        {
            await AsyncSwap(_selection[0], _selection[1]);
        }

        _selection.Clear();
    }

    public async Task AsyncSwap(Tile tile1, Tile tile2, float animSpeed = 1f)
    {
        // animate movement
        await Animate.AsyncSwap(tile1, tile2, animSpeed);
        // swap data
        SwapData(tile1, tile2);
    }


    private bool CanPop()
    {
        bool canPop = false;
        foreach(Tile tile in Tiles)
        {
            canPop = tile.ShouldDestroy();
            if (canPop) break;
        }
        return canPop;
    }

    private async Task Pop()
    {
        foreach(Tile tile in Tiles)
        {
            List<Tile> connectedTiles = tile.GetConnectedTiles();
            if (tile.IsNone() || !tile.ShouldDestroy()) continue;

            Sequence deflateSequence = DOTween.Sequence();
            foreach(Tile connectedTile in connectedTiles)
            {
                Animate.Kill(connectedTile, deflateSequence);
            }
            audioSource.PlayOneShot(collectSound);
            await deflateSequence.Play().AsyncWaitForCompletion();

            foreach(Tile connectedTile in connectedTiles)
            {
                ScoreCounter.Instance.Score += ItemDatabase.GetItemValue(connectedTile.Type);
                connectedTile.Type = Item.Types.NONE;
            }
        }
        await Fall();
        await FillBlanks();
    }

    private async Task Fall()
    {
        bool hasFallenItem = false;
        for (int x = 0; x < Width; x++)
        {
            Tile blankTile = null;
            for (int y = Height - 1; y >= 0f; y--)
            {
                Tile tile = Tiles[x, y];
                if (!tile.IsNone() && blankTile != null)
                {
                    y = blankTile.y;
                    await AsyncSwap(tile, blankTile, 6);
                    blankTile = null;
                    hasFallenItem = true;
                }
                else if (tile.IsNone() && blankTile == null)
                {
                    blankTile = tile;
                }
            }
        }

        if (hasFallenItem) await Pop();
    }

    private async Task FillBlanks()
    {
        Sequence inflateSequence = DOTween.Sequence();
        foreach(Tile tile in Tiles)
        {
            if (!tile.IsNone()) continue;
            tile.Type = ItemDatabase.GetRandomItem().type;
            while (tile.GetConnectedTiles().Count > 2)
            {
                tile.Type = ItemDatabase.GetRandomItem().type;
            }
            Animate.Spawn(tile, inflateSequence);
        }
        await inflateSequence.Play().AsyncWaitForCompletion();
    }


    public void SwapData(Tile tile1, Tile tile2)
    {
        // persistance variables
        Item.Types item1 = tile1.Type;
        Item.Types item2 = tile2.Type;
        Image icon1 = tile1.icon;
        Image icon2 = tile2.icon;
        Transform icon1Transform = icon1.transform;
        Transform icon2Transform = icon2.transform;

        // swap parents
        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        // swap icons
        tile1.icon = icon2;
        tile2.icon = icon1;

        // swap types
        tile1.Type = item2;
        tile2.Type = item1;
    }
}
