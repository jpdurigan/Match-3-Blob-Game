using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MessagePanel : MonoBehaviour
{
    public static MessagePanel Instance { get; private set; }

    [HideInInspector] public bool visible { get; private set; }

    public Message WinMessage;
    public Message LostAllLivesMessage;
    public Message TurnsEndedMessage;
    [HideInInspector] public Message EmptyMessage = new Message("", "", false, false);

    private const float FADE_BG_SHOW = 0.8f;

    [Space]
    [SerializeField] private Image messageBG;
    [SerializeField] private TextMeshProUGUI messageMainText;
    [SerializeField] private TextMeshProUGUI messageSubText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button levelSelectButton;

    private void Awake()
    {
        Instance = this;
        messageBG.color = messageBG.color * new Color(1f, 1f, 1f, FADE_BG_SHOW);
        messageMainText.SetText("");
        messageSubText.SetText("");
        visible = true;
    }


    public async Task Show()
    {
        if (visible) return;
        visible = true;
        gameObject.SetActive(true);
        Sequence fadeSequence = DOTween.Sequence();
        Animate.Fade(messageBG, fadeSequence, FADE_BG_SHOW);
        Animate.FadeIn(messageMainText, fadeSequence);
        Animate.FadeIn(messageSubText, fadeSequence);
        Animate.FadeIn(retryButton.image, fadeSequence);
        Animate.FadeIn(nextButton.image, fadeSequence);
        Animate.FadeIn(levelSelectButton.image, fadeSequence);
        await fadeSequence.Play().AsyncWaitForCompletion();
    }

    public async Task Hide()
    {
        if (!visible) return;
        visible = false;
        Sequence fadeSequence = DOTween.Sequence();
        Animate.FadeOut(messageBG, fadeSequence);
        Animate.FadeOut(messageMainText, fadeSequence);
        Animate.FadeOut(messageSubText, fadeSequence);
        Animate.FadeOut(retryButton.image, fadeSequence);
        Animate.FadeOut(nextButton.image, fadeSequence);
        Animate.FadeOut(levelSelectButton.image, fadeSequence);
        await fadeSequence.Play().AsyncWaitForCompletion();
        gameObject.SetActive(false);
    }

    public async Task ShowMessage(Message msg)
    {
        messageMainText.SetText(msg.mainText);
        messageSubText.SetText(msg.subText);
        retryButton.gameObject.SetActive(msg.showRetry);
        nextButton.gameObject.SetActive(msg.showNext && LevelManager.Instance.HasNextLevel());
        levelSelectButton.gameObject.SetActive(msg.mainText != "");
        if (!visible) await Show();
    }

    [Serializable]
    public struct Message
    {
        [TextArea(1,3)]
        public string mainText;
        [TextArea(1,3)]
        public string subText;
        public bool showRetry;
        public bool showNext;

        public Message(string main, string sub, bool retry, bool next)
        {
            this.mainText = main;
            this.subText = sub;
            this.showRetry = retry;
            this.showNext = next;
        }
    }
}
