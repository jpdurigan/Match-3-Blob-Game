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
        public static float DEFAULT_RATIO = 0.5f;

        public static Options Default = new Options(DEFAULT_DURATION, DEFAULT_DELAY, DEFAULT_RATIO);
        public static Options Wiggle = new Options(DEFAULT_DURATION, DEFAULT_DELAY, 0.2f);
        public static Options HUD = new Options(DEFAULT_DURATION, DEFAULT_DELAY, 0.35f).SpeedBy(0.5f);
        public static Options Panel = new Options(DEFAULT_DURATION, DEFAULT_DELAY, DEFAULT_RATIO).SpeedBy(0.85f);

        public float duration { get; private set; }
        public float delay { get; private set; }
        public float ratio { get; private set; } // ratio when animation has 2 states

        public float inwardsDuration => duration * ratio;
        public float outwardsDuration => duration * (1f - ratio);

        public Options(float p_duration, float p_delay, float p_ratio)
        {
            duration = p_duration;
            delay = p_delay;
            ratio = p_ratio;
        }

        public Options SpeedBy(float speed)
        {
            duration /= speed;
            delay /= speed;
            return this;
        }

        public static Options Delay(float p_delay)
        {
            return new Options(DEFAULT_DURATION, p_delay, DEFAULT_RATIO);
        }

        public static Options RandomDelay()
        {
            return RandomDelay(DEFAULT_RANDOM_DELAY);
        }
        public static Options RandomDelay(float randomness)
        {
            float randomDelay = Random.Range(0f, DEFAULT_DURATION) * randomness;
            return new Options(DEFAULT_DURATION, randomDelay, DEFAULT_RATIO);
        }

        public static Options Duration(float p_duration)
        {
            return new Options(p_duration, DEFAULT_DELAY, DEFAULT_RATIO);
        }

        public static Options Speed(float speed)
        {
            return new Options(DEFAULT_DURATION / speed, DEFAULT_DELAY, DEFAULT_RATIO);
        }

        public static Options Ratio(float p_ratio)
        {
            return new Options(DEFAULT_DURATION, DEFAULT_DELAY, p_ratio);
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
    private static Color COLOR_TEXT_HIGHLIGHT = new Color(1f, 0.94f, 0.6f, 0.25f);

    private static float FADE_IN = 1f;
    private static float FADE_OUT = 0f;

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
        sequence.Insert(options.delay,
                        tile.icon.transform.DOScale(SCALE_SELECTED, options.inwardsDuration).SetEase(Ease.OutBack))
                .Insert(options.delay + options.inwardsDuration,
                        tile.icon.transform.DOScale(SCALE_ALIVE, options.outwardsDuration).SetEase(Ease.OutBounce));
    }
    public static void Wiggle(Tile tile, Sequence sequence)
    {
        Wiggle(tile, sequence, Options.Default);
    }

    public static void Fade(Graphic graphic, Sequence sequence, float fade, Options options)
    {
        sequence.Insert(options.delay, graphic.DOFade(fade, options.duration));
    }
    public static void Fade(Graphic graphic, Sequence sequence, float fade)
    {
        Fade(graphic, sequence, fade, Options.Default);
    }
    public static void FadeIn(Graphic graphic, Sequence sequence, Options options)
    {
        Fade(graphic, sequence, FADE_IN, options);
    }
    public static void FadeIn(Graphic graphic, Sequence sequence)
    {
        Fade(graphic, sequence, FADE_IN, Options.Default);
    }
    public static void FadeOut(Graphic graphic, Sequence sequence, Options options)
    {
        Fade(graphic, sequence, FADE_OUT, options);
    }
    public static void FadeOut(Graphic graphic, Sequence sequence)
    {
        Fade(graphic, sequence, FADE_OUT, Options.Default);
    }

    public static void UpdateText(TextMeshProUGUI text, string msg, Sequence sequence, Color colorMultiply, Options options)
    {
        HighlightGraphic(text, sequence, colorMultiply, options);
        sequence.InsertCallback(options.delay + options.inwardsDuration, () => text.SetText(msg));
    }
    public static void UpdateText(TextMeshProUGUI text, string msg, Sequence sequence)
    {
        UpdateText(text, msg, sequence, COLOR_TEXT_HIGHLIGHT, Options.Default);
    }

    public static void HighlightGraphic(Graphic graphic, Sequence sequence, Color colorMultiply, Options options)
    {
        Color initialColor = graphic.color;
        Color highlightColor = initialColor * colorMultiply;
        sequence.Insert(options.delay,
                        graphic.DOColor(highlightColor, options.inwardsDuration))
                .Insert(options.delay,
                        graphic.transform.DOScale(SCALE_GRAPHIC_EXPAND, options.inwardsDuration).SetEase(Ease.InCubic))
                .Insert(options.delay + options.inwardsDuration,
                        graphic.DOColor(initialColor, options.outwardsDuration))
                .Insert(options.delay + options.inwardsDuration,
                        graphic.transform.DOScale(SCALE_GRAPHIC_NORMAL, options.outwardsDuration).SetEase(Ease.OutBack));
    }
    public static void HighlightGraphic(Graphic graphic, Sequence sequence)
    {
        HighlightGraphic(graphic, sequence, COLOR_TEXT_HIGHLIGHT, Options.Default);
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
        await AsyncWiggle(tile, Options.Wiggle);
    }
}
