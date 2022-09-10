// provavelmente tem um jeito mais inteligente de fazer isso
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using DG.Tweening;

public sealed class Slime : MonoBehaviour
{
    [System.Serializable]
    public struct SlimeSprite
    {
        public Sprite sprite;

        [Range(-1, 1)] public int TopLeft;
        [Range(-1, 1)] public int Top;
        [Range(-1, 1)] public int TopRight;
        [Range(-1, 1)] public int Left;
        [Range(-1, 1)] public int Right;
        [Range(-1, 1)] public int BottomLeft;
        [Range(-1, 1)] public int Bottom;
        [Range(-1, 1)] public int BottomRight;


        public bool Matches(SlimeSprite tile)
        {
            bool matches = (
                (TopLeft == 0 || TopLeft == tile.TopLeft)
                && (Top == 0 || Top == tile.Top)
                && (TopRight == 0 || TopRight == tile.TopRight)
                && (Left == 0 || Left == tile.Left)
                && (Right == 0 || Right == tile.Right)
                && (BottomLeft == 0 || BottomLeft == tile.BottomLeft)
                && (Bottom == 0 || Bottom == tile.Bottom)
                && (BottomRight == 0 || BottomRight == tile.BottomRight)
            );
            return matches;
        }
    }

    [SerializeField] private SlimeSprite[] sprites;
    [SerializeField] private Sprite defaultSprite;

    public static Slime Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Sprite GetSprite(Tile tile)
    {
        Sprite sprite = defaultSprite;
        SlimeSprite tileSprite = GetSlimeSpriteFromTile(tile);
        foreach(SlimeSprite slimeSprite in sprites)
        {
            if (slimeSprite.Matches(tileSprite))
            {
                sprite = slimeSprite.sprite;
                break;
            }
        }
        return sprite;
    }

    public Sprite GetDefaultSprite()
    {
        return defaultSprite;
    }

    private SlimeSprite GetSlimeSpriteFromTile(Tile tile)
    {
        SlimeSprite tileSprite = new SlimeSprite();
        List<Tile> tileConnections = tile.GetAllConnections();
        tileSprite.TopLeft = tileConnections.Contains(tile.TopLeft) ? 1 : -1;
        tileSprite.Top = tileConnections.Contains(tile.Top) ? 1 : -1;
        tileSprite.TopRight = tileConnections.Contains(tile.TopRight) ? 1 : -1;
        tileSprite.Left = tileConnections.Contains(tile.Left) ? 1 : -1;
        tileSprite.Right = tileConnections.Contains(tile.Right) ? 1 : -1;
        tileSprite.BottomLeft = tileConnections.Contains(tile.BottomLeft) ? 1 : -1;
        tileSprite.Bottom = tileConnections.Contains(tile.Bottom) ? 1 : -1;
        tileSprite.BottomRight = tileConnections.Contains(tile.BottomRight) ? 1 : -1;
        return tileSprite;
    }
}
