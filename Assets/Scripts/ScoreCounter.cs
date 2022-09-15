using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ScoreCounter : MonoBehaviour
{
    public static ScoreCounter Instance { get; private set; }

    public int Score { get; private set; } = -1;
    public int TurnsLeft { get; private set; } = -1;
    public int TargetsLeft { get; private set; } = -1;
    public int Lives { get; private set; } = -1;

    public bool HasNoTurnLeft => currentLevel != null && TurnsLeft <= 0;
    public bool HasNoLivesLeft => currentLevel != null && Lives <= 0;
    public bool HasNoTargetsGoal => currentLevel != null && TargetsLeft <= 0;
    public bool HasPlayerWon => HasNoTargetsGoal;
    public bool HasPlayerLost => HasNoTurnLeft || HasNoLivesLeft;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Color scoreHighlight = Color.white;
    [Space]
    [SerializeField] private TextMeshProUGUI turnsText;
    [SerializeField] private Image turnsIcon;
    [SerializeField] private Color turnsHighlight = Color.white;
    [Space]
    [SerializeField] private TextMeshProUGUI targetsText;
    [SerializeField] private Image targetsIcon;
    [SerializeField] private Color targetsHighlight = Color.white;

    [Space]
    [SerializeField] private GameObject UITitle;
    [SerializeField] private GameObject UILevel;

    private Level currentLevel = null;

    private void Awake()
    {
        Instance = this;
        ShowTitle();
    }


    public void StartLevel(Level level)
    {
        currentLevel = level;

        Score = 0;
        scoreText.SetText(Score.ToString());

        TurnsLeft = level.turnsAmount;
        turnsText.SetText(TurnsLeft.ToString());

        TargetsLeft = level.goalAmount;
        targetsText.SetText(TargetsLeft.ToString());

        targetsIcon.sprite = ItemDatabase.GetItemSprite(level.goalType);

        Lives = 1;
        ShowLevel();
    }

    public void OnSuccessfullSwap()
    {
        TurnsLeft = Mathf.Max(0, TurnsLeft - 1);
        Sequence sequence = DOTween.Sequence();
        Animate.UpdateText(turnsText, TurnsLeft.ToString(), sequence, Animate.Options.Speed(0.5f));
        Animate.HighlightGraphic(turnsIcon, sequence, Animate.Options.Speed(0.5f));
        sequence.Play();
    }

    public void AddToScore(Item.Types type, Sequence sequence)
    {
        if (type == currentLevel.goalType)
        {
            TargetsLeft = Mathf.Max(0, TargetsLeft - 1);
            Animate.UpdateText(targetsText, TargetsLeft.ToString(), sequence, Animate.Options.Speed(0.5f));
            Animate.HighlightGraphic(targetsIcon, sequence, Animate.Options.Speed(0.5f));
        }
        Score += ItemDatabase.GetItemValue(type);
        Animate.UpdateText(scoreText, Score.ToString(), sequence);
    }

    public async Task AsyncAddToScore(Item.Types type)
    {
        Sequence sequence = DOTween.Sequence();
        AddToScore(type, sequence);
        await sequence.Play().AsyncWaitForCompletion();
    }

    public void ShowTitle()
    {
        UITitle.SetActive(true);
        UILevel.SetActive(false);
    }
    public void ShowLevel()
    {
        UITitle.SetActive(false);
        UILevel.SetActive(true);
    }

}
