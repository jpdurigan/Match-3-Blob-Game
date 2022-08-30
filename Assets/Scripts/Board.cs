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
                tile.Type = ItemDatabase.GetRandomItem().type;
                while (tile.GetConnectedTiles().Count > 2)
                {
                    tile.Type = ItemDatabase.GetRandomItem().type;
                }
            }
        }


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

        await Swap(_selection[0], _selection[1]);

        if (CanPop())
        {
            await Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);
        }

        Sequence deselectSequence = DOTween.Sequence();
        foreach(Tile selectedTile in _selection)
        {
            bool hasPopped = selectedTile.icon.transform.localScale == Vector3.zero;
            if (!hasPopped) Animate.Deselect(selectedTile, deselectSequence);
        }
        await deselectSequence.Play().AsyncWaitForCompletion();

        _selection.Clear();
    }

    public async Task Swap(Tile tile1, Tile tile2, float animSpeed = 1f)
    {
        // persistance variables
        Item.Types item1 = tile1.Type;
        Item.Types item2 = tile2.Type;
        Image icon1 = tile1.icon;
        Image icon2 = tile2.icon;
        Transform icon1Transform = icon1.transform;
        Transform icon2Transform = icon2.transform;

        // animate movement
        await Animate.AsyncSwap(tile1, tile2, animSpeed);

        // swap parents
        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        // swap icons
        tile1.icon = icon2;
        tile2.icon = icon1;

        // swap items
        tile1.Type = item2;
        tile2.Type = item1;
    }

    private bool CanPop()
    {
        bool canPop = false;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (Tiles[x, y].GetConnectedTiles().Count >= 3)
                {
                    canPop = true;
                    return canPop;
                }
            }
        }
        return canPop;
    }

    private async Task Pop()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Tile tile = Tiles[x, y];
                List<Tile> connectedTiles = tile.GetConnectedTiles();
                if (tile.IsNone() || connectedTiles.Count <= 2) continue;

                Sequence deflateSequence = DOTween.Sequence();
                foreach(Tile connectedTile in connectedTiles)
                {
                    Debug.Log($"killing {connectedTile} | type: {connectedTile.Type}");
                    Animate.Kill(connectedTile, deflateSequence);
                    ScoreCounter.Instance.Score += ItemDatabase.GetItemValue(connectedTile.Type);
                    connectedTile.Type = Item.Types.NONE;
                }

                audioSource.PlayOneShot(collectSound);
                await deflateSequence.Play().AsyncWaitForCompletion();

                // Sequence inflateSequence = DOTween.Sequence();
                // foreach(Tile connectedTile in connectedTiles)
                // {
                //     connectedTile.Item = ItemDatabase.GetRandomItem();
                //     while (tile.GetConnectedTiles().Count > 2)
                //     {
                //         tile.Item = ItemDatabase.GetRandomItem();
                //     }
                //     Animate.Spawn(connectedTile, inflateSequence);
                // }
                // await inflateSequence.Play().AsyncWaitForCompletion();

                // x = 0; y = 0;
            }
        }
        await Fall();
    }

    private async Task Fall()
    {
        for (int x = 0; x < Width; x++)
        {
            Tile blankTile = null;
            for (int y = Height - 1; y >= 0f; y--)
            {
                Tile tile = Tiles[x, y];
                if (!tile.IsNone() && blankTile != null)
                {
                    y = blankTile.y;
                    await Swap(tile, blankTile, 6);
                    blankTile = null;
                }
                else if (tile.IsNone() && blankTile == null)
                {
                    blankTile = tile;
                }
            }
        }
    }
}
