using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MessagePanel : MonoBehaviour
{
    public static MessagePanel Instance { get; private set; }

    public bool visible;

    private const float FADE_SHOW = 1f;
    private const float FADE_HIDE = 0f;

    [SerializeField] private Image messageBG;
    [SerializeField] private TextMeshProUGUI message;

    private void Awake()
    {
        Instance = this;
        message.SetText("");
        visible = true;
    }


    public async Task Show()
    {
        visible = true;
        gameObject.SetActive(true);
        Sequence fadeSequence = DOTween.Sequence();
        Animate.FadeIn(messageBG, fadeSequence);
        Animate.FadeIn(message, fadeSequence);
        await fadeSequence.Play().AsyncWaitForCompletion();
    }

    public async Task Hide()
    {
        visible = false;
        Sequence fadeSequence = DOTween.Sequence();
        Animate.FadeOut(messageBG, fadeSequence);
        Animate.FadeOut(message, fadeSequence);
        await fadeSequence.Play().AsyncWaitForCompletion();
        gameObject.SetActive(false);
    }

    public async Task ShowMessage(string msg)
    {
        message.SetText(msg);
        await Show();
    }

}
