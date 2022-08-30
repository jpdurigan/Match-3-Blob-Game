using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public static class Animate
{
    private const float TWEEN_DURATION = 0.25f;

    private static Vector3 SCALE_ALIVE = Vector3.one;
    private static Vector3 SCALE_DEAD = Vector3.zero;
    private static Vector3 SCALE_SELECTED = Vector3.one * 0.8f;

    public static void Spawn(Tile tile, Sequence sequence)
    {
        sequence.Join(tile.icon.transform.DOScale(SCALE_ALIVE, TWEEN_DURATION));
    }

    public static void Kill(Tile tile, Sequence sequence)
    {
        sequence.Join(tile.icon.transform.DOScale(SCALE_DEAD, TWEEN_DURATION));
    }

    public static void Swap(Tile tile1, Tile tile2, Sequence sequence)
    {
        Transform tile1Transform = tile1.icon.transform;
        Transform tile2Transform = tile2.icon.transform;
        sequence.Join(tile1Transform.DOMove(tile2Transform.position, TWEEN_DURATION))
                .Join(tile2Transform.DOMove(tile1Transform.position, TWEEN_DURATION));
    }

    public static void Select(Tile tile, Sequence sequence)
    {
        sequence.Join(tile.icon.transform.DOScale(SCALE_SELECTED, TWEEN_DURATION));
    }

    public static void Deselect(Tile tile, Sequence sequence)
    {
        sequence.Join(tile.icon.transform.DOScale(SCALE_ALIVE, TWEEN_DURATION));
    }

    public static async Task AsyncSwap(Tile tile1, Tile tile2)
    {
        Sequence sequence = DOTween.Sequence();
        Swap(tile1, tile2, sequence);
        await sequence.Play().AsyncWaitForCompletion();
    }

    public static async Task AsyncSelect(Tile tile)
    {
        Sequence sequence = DOTween.Sequence();
        Select(tile, sequence);
        await sequence.Play().AsyncWaitForCompletion();
    }

    public static async Task AsyncDeselect(Tile tile)
    {
        Sequence sequence = DOTween.Sequence();
        Deselect(tile, sequence);
        await sequence.Play().AsyncWaitForCompletion();
    }
}
