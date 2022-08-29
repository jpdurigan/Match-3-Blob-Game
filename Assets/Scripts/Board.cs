using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    private const float TweenDuration = 0.25f;

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
                tile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
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
        if (_selection.Contains(tile)) return;

        _selection.Add(tile);

        if (_selection.Count < 2) return;

        await Swap(_selection[0], _selection[1]);

        if (CanPop()) Pop();
        else await Swap(_selection[0], _selection[1]);

        _selection.Clear();
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        // persistance variables
        Item item1 = tile1.Item;
        Item item2 = tile2.Item;
        Image icon1 = tile1.icon;
        Image icon2 = tile2.icon;
        Transform icon1Transform = icon1.transform;
        Transform icon2Transform = icon2.transform;

        // animate movement
        Sequence sequence = DOTween.Sequence();
        sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration))
                .Join(icon2Transform.DOMove(icon1Transform.position, TweenDuration));

        await sequence.Play()
                      .AsyncWaitForCompletion();

        // swap parents
        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        // swap icons
        tile1.icon = icon2;
        tile2.icon = icon1;

        // swap items
        tile1.Item = item2;
        tile2.Item = item1;
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

    private async void Pop()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Tile tile = Tiles[x, y];
                List<Tile> connectedTiles = tile.GetConnectedTiles();
                if (connectedTiles.Count <= 2) continue;

                Sequence deflateSequence = DOTween.Sequence();
                foreach(Tile connectedTile in connectedTiles)
                {
                    deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration));
                    ScoreCounter.Instance.Score += connectedTile.Item.value;
                }

                audioSource.PlayOneShot(collectSound);
                await deflateSequence.Play().AsyncWaitForCompletion();

                Sequence inflateSequence = DOTween.Sequence();
                foreach(Tile connectedTile in connectedTiles)
                {
                    connectedTile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
                    inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
                }
                await inflateSequence.Play().AsyncWaitForCompletion();
            }
        }
    }
}
