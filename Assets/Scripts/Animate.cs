using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public static class Animate
{
    public class Options
    {
        public static float DEFAULT_DURATION = 0.25f;
        public static float DEFAULT_SPEED = 1f;
        public static float DEFAULT_DELAY = 0f;
        public static float DEFAULT_RANDOM_DELAY = 0.8f;

        public static Options Default = new Options(DEFAULT_DURATION, DEFAULT_DELAY);

        public float duration { get; private set; }
        public float delay { get; private set; }

        public Options(float p_duration, float p_delay)
        {
            duration = p_duration;
            delay = p_delay;
        }

        public static Options Delay(float p_delay)
        {
            return new Options(DEFAULT_DURATION, p_delay);
        }

        public static Options RandomDelay()
        {
            return RandomDelay(DEFAULT_RANDOM_DELAY);
        }
        public static Options RandomDelay(float randomness)
        {
            float randomDelay = Random.Range(0f, DEFAULT_DURATION) * randomness;
            return new Options(DEFAULT_DURATION, randomDelay);
        }

        public static Options Duration(float p_duration)
        {
            return new Options(p_duration, DEFAULT_DELAY);
        }

        public static Options Speed(float speed)
        {
            return new Options(DEFAULT_DURATION / speed, DEFAULT_DELAY);
        }
    }


    private static Vector3 SCALE_ALIVE = Vector3.one;
    private static Vector3 SCALE_DEAD = Vector3.zero;
    private static Vector3 SCALE_SELECTED = Vector3.one * 0.8f;
    private static Vector3 SCALE_GRAPHIC_NORMAL = Vector3.one;
    private static Vector3 SCALE_GRAPHIC_EXPAND = Vector3.one * 1.8f;

    private static Color COLOR_ALIVE = Color.white;
    private static Color COLOR_DEAD = new Color(1, 1, 1, 0);
    private static Color COLOR_TEXT_NORMAL = Color.white;
    private static Color COLOR_TEXT_HIGHLIGHT = new Color(1f, 0.825f, 0f, 0.25f);

    private static float FADE_IN = 1f;
    private static float FADE_OUT = 0f;

    private static float SCALE_Y_NORMAL = 1f;
    private static float SCALE_Y_SQUISHY = 0.76f;

    public static void Spawn(Tile tile, Sequence sequence, Options options)
    {
        sequence.Insert(0f, tile.icon.transform.DOScale(SCALE_DEAD, 0f))
                .Insert(0f, tile.icon.DOColor(COLOR_ALIVE, options.duration))
                .Insert(options.delay, tile.icon.transform.DOScale(SCALE_ALIVE, options.duration));
    }
    public static void Spawn(Tile tile, Sequence sequence)
    {
        Spawn(tile, sequence, Options.Default);
    }

    public static void Kill(Tile tile, Sequence sequence, Options options)
    {
        sequence.Insert(options.delay, tile.icon.transform.DOScale(SCALE_DEAD, options.duration))
                .Insert(options.delay, tile.icon.DOColor(COLOR_DEAD, options.duration))
                .InsertCallback(options.delay + options.duration, () => tile.eyes.enabled = false);
    }
    public static void Kill(Tile tile, Sequence sequence)
    {
        Kill(tile, sequence, Options.Default);
    }

    public static void Swap(Tile tile1, Tile tile2, Sequence sequence, Options options)
    {
        Transform tile1Transform = tile1.icon.transform;
        Transform tile2Transform = tile2.icon.transform;
        sequence.Insert(options.delay, tile1Transform.DOMove(tile2Transform.position, options.duration))
                .Insert(options.delay, tile2Transform.DOMove(tile1Transform.position, options.duration));
    }
    public static void Swap(Tile tile1, Tile tile2, Sequence sequence)
    {
        Swap(tile1, tile2, sequence, Options.Default);
    }

    public static void Select(Tile tile, Sequence sequence, Options options)
    {
        sequence.Insert(options.delay, tile.icon.transform.DOScale(SCALE_SELECTED, options.duration));
    }
    public static void Select(Tile tile, Sequence sequence)
    {
        Select(tile, sequence, Options.Default);
    }

    public static void Deselect(Tile tile, Sequence sequence, Options options)
    {
        sequence.Insert(options.delay, tile.icon.transform.DOScale(SCALE_ALIVE, options.duration));
    }
    public static void Deselect(Tile tile, Sequence sequence)
    {
        Deselect(tile, sequence, Options.Default);
    }

    public static void Wiggle(Tile tile, Sequence sequence, Options options)
    {
        float ratio = 0.2f;
        float inwardsDuration = options.duration * ratio;
        float outwardsDuration = options.duration * (1f - ratio);
        sequence.Insert(options.delay, tile.icon.transform.DOScale(SCALE_SELECTED, inwardsDuration).SetEase(Ease.OutBack))
                .Insert(options.delay + inwardsDuration, tile.icon.transform.DOScale(SCALE_ALIVE, outwardsDuration).SetEase(Ease.OutBounce));
    }
    public static void Wiggle(Tile tile, Sequence sequence)
    {
        Wiggle(tile, sequence, Options.Default);
    }

    public static void FadeIn(Graphic graphic, Sequence sequence, Options options)
    {
        sequence.Insert(options.delay, graphic.DOFade(FADE_IN, options.duration));
    }
    public static void FadeIn(Graphic graphic, Sequence sequence)
    {
        FadeIn(graphic, sequence, Options.Default);
    }

    public static void FadeOut(Graphic graphic, Sequence sequence, Options options)
    {
        sequence.Insert(options.delay, graphic.DOFade(FADE_OUT, options.duration));
    }
    public static void FadeOut(Graphic graphic, Sequence sequence)
    {
        FadeOut(graphic, sequence, Options.Default);
    }

    public static void UpdateText(TextMeshProUGUI text, string msg, Sequence sequence, Options options)
    {
        Color initialColor = text.color;
        Color highlightColor = initialColor * COLOR_TEXT_HIGHLIGHT;
        float ratio = 0.35f;
        float inwardsDuration = options.duration * ratio;
        float outwardsDuration = options.duration * (1f - ratio);
        sequence.Insert(options.delay, text.DOColor(highlightColor, inwardsDuration))
                .Insert(options.delay, text.transform.DOScale(SCALE_GRAPHIC_EXPAND, inwardsDuration).SetEase(Ease.InCubic))
                .InsertCallback(options.delay + inwardsDuration, () => text.SetText(msg))
                .Insert(options.delay + inwardsDuration, text.DOColor(initialColor, outwardsDuration))
                .Insert(options.delay + inwardsDuration, text.transform.DOScale(SCALE_GRAPHIC_NORMAL, outwardsDuration).SetEase(Ease.OutBack));
    }
    public static void UpdateText(TextMeshProUGUI text, string msg, Sequence sequence)
    {
        UpdateText(text, msg, sequence, Options.Default);
    }

    public static void HighlightGraphic(Graphic graphic, Sequence sequence, Options options)
    {
        Color initialColor = graphic.color;
        Color highlightColor = initialColor * COLOR_TEXT_HIGHLIGHT;
        float ratio = 0.35f;
        float inwardsDuration = options.duration * ratio;
        float outwardsDuration = options.duration * (1f - ratio);
        sequence.Insert(options.delay, graphic.DOColor(highlightColor, inwardsDuration))
                .Insert(options.delay, graphic.transform.DOScale(SCALE_GRAPHIC_EXPAND, inwardsDuration).SetEase(Ease.InCubic))
                .Insert(options.delay + inwardsDuration, graphic.DOColor(initialColor, outwardsDuration))
                .Insert(options.delay + inwardsDuration, graphic.transform.DOScale(SCALE_GRAPHIC_NORMAL, outwardsDuration).SetEase(Ease.OutBack));
    }
    public static void HighlightGraphic(Graphic graphic, Sequence sequence)
    {
        HighlightGraphic(graphic, sequence, Options.Default);
    }


    public static async Task AsyncSwap(Tile tile1, Tile tile2, Options options)
    {
        Sequence sequence = DOTween.Sequence();
        Swap(tile1, tile2, sequence, options);
        Deselect(tile1, sequence, options);
        Deselect(tile2, sequence, options);
        await sequence.Play().AsyncWaitForCompletion();
    }
    public static async Task AsyncSwap(Tile tile1, Tile tile2)
    {
        await AsyncSwap(tile1, tile2, Options.Default);
    }

    public static async Task AsyncSelect(Tile tile, Options options)
    {
        Sequence sequence = DOTween.Sequence();
        Select(tile, sequence, options);
        await sequence.Play().AsyncWaitForCompletion();
    }
    public static async Task AsyncSelect(Tile tile)
    {
        await AsyncSelect(tile, Options.Default);
    }

    public static async Task AsyncDeselect(Tile tile, Options options)
    {
        Sequence sequence = DOTween.Sequence();
        Deselect(tile, sequence, options);
        await sequence.Play().AsyncWaitForCompletion();
    }
    public static async Task AsyncDeselect(Tile tile)
    {
        await AsyncDeselect(tile, Options.Default);
    }

    public static async Task AsyncSpawn(Tile tile, Options options)
    {
        Sequence sequence = DOTween.Sequence();
        Spawn(tile, sequence, options);
        await sequence.Play().AsyncWaitForCompletion();
    }
    public static async Task AsyncSpawn(Tile tile)
    {
        await AsyncSpawn(tile, Options.Default);
    }

    public static async Task AsyncWiggle(Tile tile, Options options)
    {
        Sequence sequence = DOTween.Sequence();
        Wiggle(tile, sequence, options);
        await sequence.Play().AsyncWaitForCompletion();
    }
    public static async Task AsyncWiggle(Tile tile)
    {
        await AsyncWiggle(tile, Options.Default);
    }
}
