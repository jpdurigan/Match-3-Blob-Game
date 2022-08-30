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

    public static void Spawn(Tile tile, Sequence sequence, float speed = 1f)
    {
        sequence.Join(tile.icon.transform.DOScale(SCALE_ALIVE, TWEEN_DURATION / speed));
    }

    public static void Kill(Tile tile, Sequence sequence, float speed = 1f)
    {
        sequence.Join(tile.icon.transform.DOScale(SCALE_DEAD, TWEEN_DURATION / speed));
    }

    public static void Swap(Tile tile1, Tile tile2, Sequence sequence, float speed = 1f)
    {
        Transform tile1Transform = tile1.icon.transform;
        Transform tile2Transform = tile2.icon.transform;
        sequence.Join(tile1Transform.DOMove(tile2Transform.position, TWEEN_DURATION / speed))
                .Join(tile2Transform.DOMove(tile1Transform.position, TWEEN_DURATION / speed));
    }

    public static void Select(Tile tile, Sequence sequence, float speed = 1f)
    {
        sequence.Join(tile.icon.transform.DOScale(SCALE_SELECTED, TWEEN_DURATION / speed));
    }

    public static void Deselect(Tile tile, Sequence sequence, float speed = 1f)
    {
        sequence.Join(tile.icon.transform.DOScale(SCALE_ALIVE, TWEEN_DURATION / speed));
    }



    public static async Task AsyncSwap(Tile tile1, Tile tile2, float speed = 1f)
    {
        Sequence sequence = DOTween.Sequence();
        Swap(tile1, tile2, sequence, speed);
        await sequence.Play().AsyncWaitForCompletion();
    }

    public static async Task AsyncSelect(Tile tile, float speed = 1f)
    {
        Sequence sequence = DOTween.Sequence();
        Select(tile, sequence, speed);
        await sequence.Play().AsyncWaitForCompletion();
    }

    public static async Task AsyncDeselect(Tile tile, float speed = 1f)
    {
        Sequence sequence = DOTween.Sequence();
        Deselect(tile, sequence, speed);
        await sequence.Play().AsyncWaitForCompletion();
    }
}
