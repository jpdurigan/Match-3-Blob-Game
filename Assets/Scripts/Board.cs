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

    private enum Jobs
    {
        HANDLE_MATCHES,
        HANDLE_FLOATING,
        HANDLE_SLIME,
        HANDLE_BLANK,
        DONE,
    }


    [Header("Game")]
    public Row[] rows;

    public Tile[,] Tiles { get; private set; }
    [SerializeField] Vector2Int[] playerSpawn;

    public int Width => Tiles.GetLength(dimension:0);
    public int Height => Tiles.GetLength(1);

    private List<Tile> _selection = new List<Tile>();
    private bool shouldBlockSelection = false;
    
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip slimeSpawnSound;
    [SerializeField] private AudioClip slimeKillSound;
    [SerializeField] private AudioSource audioSource;

    /////////////////////////////////////////////////////
    

    //// ENGINE METHODS

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

        ScoreCounter.Instance.Score = 0;
    }

    /////////////////////////////////////////////////////
    

    //// MECHANICS
    public async void StartGame()
    {
        await SpawnPlayer();
        await HandleBlankTiles();
        await HandleGridVisual();
        ScoreCounter.Instance.Score = 0;
    }

    public async void Select(Tile tile)
    {
        if (shouldBlockSelection) return;
        if (tile.Type == Item.Types.SLIME) return;
        shouldBlockSelection = true;

        if (_selection.Contains(tile))
        {
            _selection.Remove(tile);
            await Animate.AsyncDeselect(tile);
            shouldBlockSelection = false;
            return;
        }

        if (_selection.Count == 1)
        {
            Tile selectedTile = _selection[0];
            bool isValidSelect = selectedTile.Neighbours.Contains(tile);
            if (!isValidSelect)
            {
                await Animate.AsyncWiggle(tile);
                shouldBlockSelection = false;
                return;
            }
        }

        _selection.Add(tile);
        await Animate.AsyncSelect(tile);

        if (_selection.Count < 2)
        {
            shouldBlockSelection = false;
            return;
        }

        await AsyncSwap(_selection[0], _selection[1]);

        if (HasMatches())
        {
            await HandleGrid();
        }
        else
        {
            await AsyncSwap(_selection[0], _selection[1]);
        }

        _selection.Clear();
        shouldBlockSelection = false;
    }

    private async Task HandleGrid(Jobs job = Jobs.HANDLE_MATCHES)
    {
        bool hasChangedGrid = false;
        print("// ENTERED HANDLE GRID");

        Sequence wiggleSequence = DOTween.Sequence();
        foreach(Tile tile in Tiles)
        {
            tile.OnUpdatingGrid();
            if (tile.IsSlime()) Animate.Wiggle(tile, wiggleSequence);
        }
        await wiggleSequence.Play().AsyncWaitForCompletion();

        while (job != Jobs.DONE)
        {
            switch (job)
            {
                case Jobs.HANDLE_MATCHES:
                    print("// ENTERED JOB: HANDLE_MATCHES");
                    hasChangedGrid = await HandleMatches();
                    print("// EXITED JOB: HANDLE_MATCHES");
                    job = hasChangedGrid ? Jobs.HANDLE_FLOATING : Jobs.HANDLE_SLIME;
                    break;
                case Jobs.HANDLE_FLOATING:
                    print("// ENTERED JOB: HANDLE_FLOATING");
                    hasChangedGrid = await HandleFloatingTiles();
                    print("// EXITED JOB: HANDLE_FLOATING");
                    job = hasChangedGrid ? Jobs.HANDLE_MATCHES : Jobs.HANDLE_SLIME;
                    break;
                case Jobs.HANDLE_SLIME:
                    print("// ENTERED JOB: HANDLE_SLIME");
                    hasChangedGrid = await HandleSlimeTiles();
                    print("// EXITED JOB: HANDLE_SLIME");
                    job = hasChangedGrid ? Jobs.HANDLE_FLOATING : Jobs.HANDLE_BLANK;
                    break;
                case Jobs.HANDLE_BLANK:
                    print("// ENTERED JOB: HANDLE_BLANK");
                    await HandleBlankTiles();
                    print("// EXITED JOB: HANDLE_BLANK");
                    job = Jobs.DONE;
                    break;
            }
            // await Task.Delay(1000);
        }

        print("// ENTERED JOB: HANDLE_GRID_VISUAL");
        await HandleGridVisual();
        print("// EXITED JOB: HANDLE_GRID_VISUAL");
        print("// EXITING HANDLE GRID");
    }

    private async Task<bool> HandleMatches()
    {
        bool hasChangedGrid = false;
        foreach(Tile tile in Tiles)
        {
            List<Tile> connectedTiles = tile.GetConnectedTiles();
            if (tile.IsNone() || !tile.ShouldDestroy()) continue;

            await KillTiles(connectedTiles, collectSound);
            hasChangedGrid = true;
        }
        return hasChangedGrid;
    }

    private async Task<bool> HandleFloatingTiles()
    {
        bool hasChangedGrid = false;
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
                    hasChangedGrid = true;
                }
                else if (tile.IsNone() && blankTile == null)
                {
                    blankTile = tile;
                }
            }
        }
        return hasChangedGrid;
    }

    private async Task<bool> HandleSlimeTiles()
    {
        bool hasChangedGrid = false;
        List<Tile> biggestSlime = null;
        List<List<Tile>> allSlimes = new List<List<Tile>>();

        foreach(Tile tile in Tiles)
        {
            if (!tile.IsSlime()) continue;

            List<Tile> connectedTiles = tile.GetConnectedTiles();
            if (allSlimes.Contains(connectedTiles)) continue;
            
            allSlimes.Add(connectedTiles);
            if (biggestSlime == null || connectedTiles.Count > biggestSlime.Count) biggestSlime = connectedTiles;
        }

        if (allSlimes.Count > 1)
        {
            allSlimes.Remove(biggestSlime);
            List<Tile> slimesToKill = new List<Tile>();
            foreach(List<Tile> slimeChain in allSlimes)
            {
                slimesToKill.AddRange(slimeChain);
            }
            await KillTiles(slimesToKill, slimeKillSound);
            hasChangedGrid = true;
        }

        return hasChangedGrid;
    }


    private async Task HandleBlankTiles()
    {
        Sequence inflateSequence = DOTween.Sequence();
        foreach(Tile tile in Tiles)
        {
            if (!tile.IsNone()) continue;
            tile.Type = ItemDatabase.GetRandomItem().type;
            // GARGALO
            while (tile.GetConnectedTiles().Count > 2)
            {
                tile.Type = ItemDatabase.GetRandomItem().type;
            }
            Animate.Spawn(tile, inflateSequence);
        }
        await inflateSequence.Play().AsyncWaitForCompletion();
    }

    private async Task HandleGridVisual()
    {
        List<Tile> slimeTiles = null;

        Sequence wiggleSequence = DOTween.Sequence();
        foreach(Tile tile in Tiles)
        {
            tile.UpdateVisual();
            if (tile.IsSlime())
            {
                Animate.Wiggle(tile, wiggleSequence);
                if (slimeTiles == null) slimeTiles = tile.GetConnectedTiles();
            }
        }
        await wiggleSequence.Play().AsyncWaitForCompletion();

        Tile centerSlime = GetCenterTile(slimeTiles);
        centerSlime.ShowEyes();
    }

    private async Task KillTiles(List<Tile> tiles, AudioClip sfx)
    {
        List<Tile> growthTiles = new List<Tile>();
        List<Tile> deathTiles = new List<Tile>();
        Sequence deflateSequence = DOTween.Sequence();
        foreach(Tile tile in tiles)
        {
            if (tile.Is(Item.Types.GROWTH) && tile.IsNeighbouringSlime()) growthTiles.Add(tile);
            if (tile.Is(Item.Types.DEATH) && tile.IsNeighbouringSlime()) deathTiles.Add(tile);
            Animate.Kill(tile, deflateSequence);
            ScoreCounter.Instance.Score += ItemDatabase.GetItemValue(tile.Type);
        }
        
        audioSource.PlayOneShot(sfx);
        await deflateSequence.Play().AsyncWaitForCompletion();

        if (growthTiles.Count > 0) await HandleGrowthTiles(growthTiles);
        if (deathTiles.Count > 0) await HandleDeathTiles(deathTiles);

        foreach(Tile tile in tiles)
        {
            if (growthTiles.Contains(tile)) continue;
            tile.Type = Item.Types.NONE;
        }
    }

    private async Task HandleGrowthTiles(List<Tile> tiles)
    {
        Sequence growthSequence = DOTween.Sequence();

        foreach(Tile tile in tiles)
        {
            tile.Type = Item.Types.SLIME;
            tile.OnUpdatingGrid();
            Animate.Spawn(tile, growthSequence);
        }

        audioSource.PlayOneShot(slimeSpawnSound);
        await growthSequence.Play().AsyncWaitForCompletion();
    }

    private async Task HandleDeathTiles(List<Tile> tiles)
    {
        List<Tile> slimes = new List<Tile>();
        foreach(Tile tile in tiles)
        {
            foreach(Tile neighbour in tile.Neighbours)
            {
                if (neighbour == null || !neighbour.IsSlime()) continue;
                if (!slimes.Contains(neighbour)) slimes.Add(neighbour);
            }
        }

        if (slimes.Count > 0) await KillTiles(slimes, slimeKillSound);
    }
    
    private async Task SpawnPlayer()
    {
        List<Tile> playerTiles = new List<Tile>();
        foreach(Vector2Int position in playerSpawn)
        {
            Tile tile = GetTile(position.x, position.y);
            playerTiles.Add(tile);
        }
        await HandleGrowthTiles(playerTiles);
    }

    private bool HasMatches()
    {
        bool hasMatches = false;
        foreach(Tile tile in Tiles)
        {
            hasMatches = tile.ShouldDestroy();
            if (hasMatches) break;
        }
        return hasMatches;
    }

    /////////////////////////////////////////////////////
    

    //// HELPERS

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
        return Tiles[x, y];
    }

    private Tile GetCenterTile(List<Tile> tiles)
    {
        Vector2 middlePosition = Vector2.zero;
        foreach(Tile tile in tiles)
        {
            Vector2 tilePosition = tile.GetVector2();
            middlePosition += tilePosition / (float)tiles.Count;
        }
        middlePosition.x = Mathf.Floor(middlePosition.x);
        middlePosition.y = Mathf.Floor(middlePosition.y);

        Tile centerTile = null;
        float dist = float.MaxValue;
        foreach(Tile tile in tiles)
        {
            float tileDist = (tile.GetVector2() - middlePosition).SqrMagnitude();
            if (tileDist <= Mathf.Epsilon)
            {
                centerTile = tile;
                break;
            }
            else if (tileDist < dist)
            {
                centerTile = tile;
                dist = tileDist;
            }
        }
        return centerTile;
    }

    private void SwapData(Tile tile1, Tile tile2)
    {
        // persistance variables
        Item.Types item1 = tile1.Type;
        Item.Types item2 = tile2.Type;
        Image icon1 = tile1.icon;
        Image icon2 = tile2.icon;
        Transform icon1Transform = icon1.transform;
        Transform icon2Transform = icon2.transform;
        Image eyes1 = tile1.eyes;
        Image eyes2 = tile2.eyes;

        // swap parents
        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        // swap icons
        tile1.icon = icon2;
        tile2.icon = icon1;
        tile1.eyes = eyes2;
        tile2.eyes = eyes1;

        // swap types
        tile1.Type = item2;
        tile2.Type = item1;
    }

    private async Task AsyncSwap(Tile tile1, Tile tile2, float animSpeed = 1f)
    {
        // animate movement
        await Animate.AsyncSwap(tile1, tile2, animSpeed);
        // swap data
        SwapData(tile1, tile2);
    }
}
