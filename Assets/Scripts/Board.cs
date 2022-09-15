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
        CHECK_WIN_LOSE_CONDITION,
        DONE,
    }


    [Header("Game")]
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject rowPrefab;
    private Row[] rows;
    private Tile[,] Tiles;

    public int Width => Tiles.GetLength(dimension: 0);
    public int Height => Tiles.GetLength(1);

    private List<Tile> _selection;
    private bool shouldBlockSelection;

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
        // ScoreCounter.Instance.Score = 0;
    }

    /////////////////////////////////////////////////////


    //// MECHANICS
    public async void StartLevel(Level level)
    {
        CreateGrid(level);
        _selection = new List<Tile>();
        shouldBlockSelection = false;
        
        await MessagePanel.Instance.ShowMessage(MessagePanel.Instance.EmptyMessage);

        ScoreCounter.Instance.StartLevel(level);

        await HandleInitialCondition(level);
        await HandleBlankTiles();
        await HandleGridVisual();

        await MessagePanel.Instance.Hide();
    }

    public async Task PlayerWon()
    {
        await MessagePanel.Instance.ShowMessage(MessagePanel.Instance.WinMessage);
    }

    public async Task PlayerLost()
    {
        if (ScoreCounter.Instance.HasNoLivesLeft) await MessagePanel.Instance.ShowMessage(MessagePanel.Instance.LostAllLivesMessage);
        else if (ScoreCounter.Instance.HasNoTurnLeft) await MessagePanel.Instance.ShowMessage(MessagePanel.Instance.TurnsEndedMessage);
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
            if (tile.IsTriggered) tile.IsTriggered = false;
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
        tile.IsTriggered = true;
        await Animate.AsyncSelect(tile);

        if (_selection.Count < 2)
        {
            shouldBlockSelection = false;
            return;
        }

        await AsyncSwap(_selection[0], _selection[1]);

        if (HasMatches())
        {
            ScoreCounter.Instance.OnSuccessfullSwap();
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
        // print("// ENTERED HANDLE GRID");

        Sequence wiggleSequence = DOTween.Sequence();
        foreach (Tile tile in Tiles)
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
                    // print("// ENTERED JOB: HANDLE_MATCHES");
                    hasChangedGrid = await HandleMatches();
                    // print("// EXITED JOB: HANDLE_MATCHES");
                    job = hasChangedGrid ? Jobs.HANDLE_FLOATING : Jobs.HANDLE_SLIME;
                    break;
                case Jobs.HANDLE_FLOATING:
                    // print("// ENTERED JOB: HANDLE_FLOATING");
                    hasChangedGrid = await HandleFloatingTiles();
                    // print("// EXITED JOB: HANDLE_FLOATING");
                    job = hasChangedGrid ? Jobs.HANDLE_MATCHES : Jobs.HANDLE_SLIME;
                    break;
                case Jobs.HANDLE_SLIME:
                    // print("// ENTERED JOB: HANDLE_SLIME");
                    hasChangedGrid = await HandleSlimeTiles();
                    // print("// EXITED JOB: HANDLE_SLIME");
                    job = hasChangedGrid ? Jobs.HANDLE_FLOATING : Jobs.CHECK_WIN_LOSE_CONDITION;
                    break;
                case Jobs.CHECK_WIN_LOSE_CONDITION:
                    if (ScoreCounter.Instance.HasPlayerWon)
                    {
                        await PlayerWon();
                        return;
                    }
                    else if (ScoreCounter.Instance.HasPlayerLost)
                    {
                        await PlayerLost();
                        return;
                    }
                    job = Jobs.HANDLE_BLANK;
                    break;
                case Jobs.HANDLE_BLANK:
                    // print("// ENTERED JOB: HANDLE_BLANK");
                    await HandleBlankTiles();
                    // print("// EXITED JOB: HANDLE_BLANK");
                    job = Jobs.DONE;
                    break;
            }
            // await Task.Delay(1000);
        }

        // print("// ENTERED JOB: HANDLE_GRID_VISUAL");
        await HandleGridVisual();
        // print("// EXITED JOB: HANDLE_GRID_VISUAL");
        // print("// EXITING HANDLE GRID");
    }

    private async Task<bool> HandleMatches()
    {
        bool hasChangedGrid = false;
        foreach (Tile tile in Tiles)
        {
            if (tile.IsNone() || !tile.ShouldDestroy()) continue;

            Structure structure = tile.GetStructure();
            await KillTiles(structure, collectSound);
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
                if (tile.IsBlock()) continue;
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

        foreach (Tile tile in Tiles)
        {
            if (!tile.IsSlime()) continue;

            List<Tile> connectedTiles = tile.GetAllConnections();
            if (allSlimes.Contains(connectedTiles)) continue;

            allSlimes.Add(connectedTiles);
            if (biggestSlime == null || connectedTiles.Count > biggestSlime.Count) biggestSlime = connectedTiles;
        }

        if (allSlimes.Count > 1)
        {
            allSlimes.Remove(biggestSlime);
            Structure slimesToKill = new Structure();
            foreach (List<Tile> slimeChain in allSlimes)
            {
                foreach (Tile slimeTile in slimeChain)
                {
                    slimesToKill.Add(slimeTile);
                }
            }
            await KillTiles(slimesToKill, slimeKillSound);
            hasChangedGrid = true;
        }

        return hasChangedGrid;
    }

    private async Task HandleBlankTiles()
    {
        Sequence inflateSequence = DOTween.Sequence();
        foreach (Tile tile in Tiles)
        {
            if (!tile.IsNone()) continue;
            tile.Type = ItemDatabase.GetRandomItem().type;
            // GARGALO
            while (tile.ShouldDestroy())
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
        foreach (Tile tile in Tiles)
        {
            tile.UpdateVisual();
            if (tile.IsSlime())
            {
                Animate.Wiggle(tile, wiggleSequence);
                if (slimeTiles == null) slimeTiles = tile.GetAllConnections();
            }
        }

        if (slimeTiles != null)
        {
            // ScoreCounter.Instance.Lives = slimeTiles.Count;
            await wiggleSequence.Play().AsyncWaitForCompletion();

            Tile centerSlime = GetCenterTile(slimeTiles);
            centerSlime.ShowEyes();
        }
        else
        {
            // await MessagePanel.Instance.ShowMessage("Game Over!");
        }
    }

    private async Task KillTiles(Structure structure, AudioClip sfx)
    {
        Sequence deflateSequence = DOTween.Sequence();
        foreach (Tile tile in structure.Tiles)
        {
            Animate.Kill(tile, deflateSequence);
            ScoreCounter.Instance.AddToScore(tile.Type, deflateSequence);
        }

        audioSource.PlayOneShot(sfx);
        await deflateSequence.Play().AsyncWaitForCompletion();

        List<Tile> excludeTiles = new List<Tile>();

        // handle creating bombs
        Item.Types bombType = structure.GetBomb();
        if (bombType != Item.Types.NONE)
        {
            Tile bombTile = await HandleGrowBomb(structure.Tiles, bombType);
            excludeTiles.Add(bombTile);
        }

        if (structure.type == Item.Types.GROWTH)
        {
            List<Tile> tilesGrown = await HandleGrowthTiles(structure);
            excludeTiles.AddRange(tilesGrown);
        }
        if (structure.type == Item.Types.DEATH) await HandleDeathTiles(structure);

        foreach (Tile tile in structure.Tiles)
        {
            if (excludeTiles.Contains(tile)) continue;
            tile.Type = Item.Types.NONE;
        }
    }

    private async Task GrowSlimeTiles(List<Tile> tilesToGrow)
    {
        Sequence growthSequence = DOTween.Sequence();

        foreach (Tile tile in tilesToGrow)
        {
            // if tile is occupied with bomb, skip it
            if (tile.IsBomb()) continue;

            tile.Type = Item.Types.SLIME;
            tile.OnUpdatingGrid();
            Animate.Spawn(tile, growthSequence);
        }

        audioSource.PlayOneShot(slimeSpawnSound);
        await growthSequence.Play().AsyncWaitForCompletion();
    }

    private async Task<Tile> HandleGrowBomb(List<Tile> tiles, Item.Types bomb)
    {
        Tile centerTile = GetCenterTile(tiles);
        centerTile.Type = bomb;
        centerTile.OnUpdatingGrid();
        await Animate.AsyncSpawn(centerTile);
        return centerTile;
    }

    private async Task<List<Tile>> HandleGrowthTiles(Structure structure)
    {
        List<Tile> tilesToGrow = SpecialItem.GetGrowthAffectedTiles(structure);
        if (tilesToGrow.Count > 0) await GrowSlimeTiles(tilesToGrow);
        return tilesToGrow;
    }

    private async Task<List<Tile>> HandleDeathTiles(Structure structure)
    {
        List<Tile> tilesToKill = SpecialItem.GetDeathAffectedTiles(structure);
        if (tilesToKill.Count > 0)
        {
            Structure slimes = new Structure();
            slimes.AddList(tilesToKill);
            await KillTiles(slimes, slimeKillSound);
        }
        return tilesToKill;
    }

    private async Task HandleInitialCondition(Level level)
    {
        Sequence growthSequence = DOTween.Sequence();

        for (int y = 0; y < level.Height; y++)
        {
            for (int x = 0; x < level.Width; x++)
            {
                Item.Types type = level.GetTile(x, y);
                if (type != Item.Types.RANDOM)
                {
                    Tile tile = Tiles[x, y];
                    tile.Type = type;
                    tile.OnUpdatingGrid();
                    Animate.Spawn(tile, growthSequence);
                }
            }
        }

        audioSource.PlayOneShot(slimeSpawnSound);
        await growthSequence.Play().AsyncWaitForCompletion();
    }

    private bool HasMatches()
    {
        bool hasMatches = false;
        foreach (Tile tile in Tiles)
        {
            hasMatches = tile.ShouldDestroy();
            if (hasMatches) break;
        }
        return hasMatches;
    }

    /////////////////////////////////////////////////////


    //// HELPERS

    private void CreateGrid(Level level)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        GetComponent<AspectRatioFitter>().aspectRatio = (float)level.Width / (float)level.Height;
        Tiles = new Tile[level.Width, level.Height];
        for (var y = 0; y < Height; y++)
        {
            GameObject currentRow = Instantiate(rowPrefab, Vector3.zero, Quaternion.identity, transform);
            for (var x = 0; x < Width; x++)
            {
                GameObject currentTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, currentRow.transform);
                Tile tile = currentTile.GetComponent<Tile>();
                tile.x = x;
                tile.y = y;
                Tiles[x, y] = tile;
            }
        }

        foreach (Tile tile in Tiles)
        {
            tile.Initialize();
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
        return Tiles[x, y];
    }

    private Tile GetCenterTile(List<Tile> tiles)
    {
        Vector2 middlePosition = Vector2.zero;
        foreach (Tile tile in tiles)
        {
            Vector2 tilePosition = tile.GetVector2();
            middlePosition += tilePosition / (float)tiles.Count;
        }
        middlePosition.x = Mathf.Floor(middlePosition.x);
        middlePosition.y = Mathf.Floor(middlePosition.y);

        Tile centerTile = null;
        float dist = float.MaxValue;
        foreach (Tile tile in tiles)
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
        bool isTriggered1 = tile1.IsTriggered;
        bool isTriggered2 = tile2.IsTriggered;

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

        // swap is triggered
        tile1.IsTriggered = isTriggered2;
        tile2.IsTriggered = isTriggered1;
    }

    private async Task AsyncSwap(Tile tile1, Tile tile2, float animSpeed = 1f)
    {
        // animate movement
        await Animate.AsyncSwap(tile1, tile2, Animate.Options.Speed(animSpeed));
        // swap data
        SwapData(tile1, tile2);
    }
}
