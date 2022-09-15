using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public static class Animate
{
    private const float TWEEN_DURATION = 0.25f;
    private const float SPAWN_RANDOMNESS = 0.8f;

    private static Vector3 SCALE_ALIVE = Vector3.one;
    private static Vector3 SCALE_DEAD = Vector3.zero;
    private static Vector3 SCALE_SELECTED = Vector3.one * 0.8f;

    private static Color COLOR_ALIVE = Color.white;
    private static Color COLOR_DEAD = new Color(1, 1, 1, 0);
    private static Color COLOR_TEXT_NORMAL = Color.white;
    private static Color COLOR_TEXT_HIGHLIGHT = new Color(1f, 0.825f, 0f, 0.25f);

    private static float FADE_IN = 1f;
    private static float FADE_OUT = 0f;

    private static float SCALE_Y_NORMAL = 1f;
    private static float SCALE_Y_SQUISHY = 0.76f;

    public static void Spawn(Tile tile, Sequence sequence, float speed = 1f)
    {
        float randomDelay = Random.Range(0f, TWEEN_DURATION) * SPAWN_RANDOMNESS / speed;
        sequence.Insert(0f, tile.icon.transform.DOScale(SCALE_DEAD, 0f))
                .Insert(0f, tile.icon.DOColor(COLOR_ALIVE, TWEEN_DURATION / speed))
                .Insert(randomDelay, tile.icon.transform.DOScale(SCALE_ALIVE, TWEEN_DURATION / speed));
    }

    public static void Kill(Tile tile, Sequence sequence, float speed = 1f)
    {
        sequence.Insert(0f, tile.icon.transform.DOScale(SCALE_DEAD, TWEEN_DURATION / speed))
                .Insert(0f, tile.icon.DOColor(COLOR_DEAD, TWEEN_DURATION / speed))
                .InsertCallback(TWEEN_DURATION / speed, () => tile.eyes.enabled = false);
    }

    public static void Swap(Tile tile1, Tile tile2, Sequence sequence, float speed = 1f)
    {
        Transform tile1Transform = tile1.icon.transform;
        Transform tile2Transform = tile2.icon.transform;
        sequence.Insert(0f, tile1Transform.DOMove(tile2Transform.position, TWEEN_DURATION / speed))
                .Insert(0f, tile2Transform.DOMove(tile1Transform.position, TWEEN_DURATION / speed));
    }

    public static void Select(Tile tile, Sequence sequence, float speed = 1f)
    {
        sequence.Insert(0f, tile.icon.transform.DOScale(SCALE_SELECTED, TWEEN_DURATION / speed));
    }

    public static void Deselect(Tile tile, Sequence sequence, float speed = 1f)
    {
        sequence.Insert(0f, tile.icon.transform.DOScale(SCALE_ALIVE, TWEEN_DURATION / speed));
    }

    public static void Wiggle(Tile tile, Sequence sequence, float speed = 1f)
    {
        float tweenDuration = TWEEN_DURATION / speed;
        float ratio = 0.2f;
        sequence.Insert(0f, tile.icon.transform.DOScale(SCALE_SELECTED, tweenDuration * ratio).SetEase(Ease.OutBack))
                .Insert(tweenDuration * ratio, tile.icon.transform.DOScale(SCALE_ALIVE, tweenDuration * (1f - ratio)).SetEase(Ease.OutBounce));
    }

    public static void FadeIn(Graphic graphic, Sequence sequence, float speed = 1f)
    {
        sequence.Insert(0f, graphic.DOFade(FADE_IN, TWEEN_DURATION / speed));
    }

    public static void FadeOut(Graphic graphic, Sequence sequence, float speed = 1f)
    {
        sequence.Insert(0f, graphic.DOFade(FADE_OUT, TWEEN_DURATION / speed));
    }

    public static void UpdateText(TextMeshProUGUI text, string msg, Sequence sequence, float speed = 1f)
    {
        Color initialColor = text.color;
        Color highlightColor = initialColor * COLOR_TEXT_HIGHLIGHT;
        float tweenDuration = TWEEN_DURATION / (2 * speed);
        sequence.Insert(0f, text.DOColor(highlightColor, tweenDuration))
                .Insert(0f, text.transform.DOScaleY(SCALE_Y_SQUISHY, tweenDuration).SetEase(Ease.InCubic))
                .InsertCallback(tweenDuration, () => text.SetText(msg))
                .Insert(tweenDuration, text.DOColor(initialColor, tweenDuration))
                .Insert(tweenDuration, text.transform.DOScaleY(SCALE_Y_NORMAL, tweenDuration).SetEase(Ease.OutBack));
    }



    public static async Task AsyncSwap(Tile tile1, Tile tile2, float speed = 1f)
    {
        Sequence sequence = DOTween.Sequence();
        Swap(tile1, tile2, sequence, speed);
        Deselect(tile1, sequence, speed);
        Deselect(tile2, sequence, speed);
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

    public static async Task AsyncSpawn(Tile tile, float speed = 1f)
    {
        Sequence sequence = DOTween.Sequence();
        Spawn(tile, sequence, speed);
        await sequence.Play().AsyncWaitForCompletion();
    }

    public static async Task AsyncWiggle(Tile tile, float speed = 1f)
    {
        Sequence sequence = DOTween.Sequence();
        Wiggle(tile, sequence, speed);
        await sequence.Play().AsyncWaitForCompletion();
    }

    public static async Task AsyncUpdateText(TextMeshProUGUI text, string msg, float speed = 1f)
    {
        Sequence sequence = DOTween.Sequence();
        UpdateText(text, msg, sequence, speed);
        await sequence.Play().AsyncWaitForCompletion();
    }
}
