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
    [HideInInspector] public Message EmptyMessage = new Message("", "");

    private const float FADE_BG_SHOW = 0.8f;

    [Space]
    [SerializeField] private Image messageBG;
    [SerializeField] private TextMeshProUGUI messageMainText;
    [SerializeField] private TextMeshProUGUI messageSubText;

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
        await fadeSequence.Play().AsyncWaitForCompletion();
        gameObject.SetActive(false);
    }

    public async Task ShowMessage(Message msg)
    {
        messageMainText.SetText(msg.mainText);
        messageSubText.SetText(msg.subText);
        if (!visible) await Show();
    }

    [Serializable]
    public struct Message
    {
        [TextArea(1,3)]
        public string mainText;
        [TextArea(1,3)]
        public string subText;

        public Message(string main, string sub)
        {
            this.mainText = main;
            this.subText = sub;
        }
    }
}
