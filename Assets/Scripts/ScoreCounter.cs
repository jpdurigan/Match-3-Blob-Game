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
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Image livesIcon;
    [SerializeField] private Color livesHighlight = Color.white;
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

        Lives = level.initialLives;
        livesText.SetText(Lives.ToString());

        TargetsLeft = level.goalAmount;
        targetsText.SetText(TargetsLeft.ToString());
        targetsIcon.sprite = ItemDatabase.GetItemSprite(level.goalType);

        ShowLevel();
    }

    public void OnSuccessfullSwap()
    {
        TurnsLeft = Mathf.Max(0, TurnsLeft - 1);
        Sequence sequence = DOTween.Sequence();
        Animate.UpdateText(turnsText, TurnsLeft.ToString(), sequence, turnsHighlight, Animate.Options.HUD);
        Animate.HighlightGraphic(turnsIcon, sequence, turnsHighlight, Animate.Options.HUD);
        sequence.Play();
    }

    public void AddToScore(Item.Types type, Sequence sequence)
    {
        if (type == currentLevel.goalType)
        {
            TargetsLeft = Mathf.Max(0, TargetsLeft - 1);
            Animate.UpdateText(targetsText, TargetsLeft.ToString(), sequence, targetsHighlight, Animate.Options.HUD);
            Animate.HighlightGraphic(targetsIcon, sequence, targetsHighlight, Animate.Options.HUD);
        }
        Score += ItemDatabase.GetItemValue(type);
        Animate.UpdateText(scoreText, Score.ToString(), sequence);
    }

    public void AddLife(Sequence sequence)
    {
        Lives++;
        Animate.UpdateText(livesText, Lives.ToString(), sequence, livesHighlight, Animate.Options.HUD);
        Animate.HighlightGraphic(livesIcon, sequence, livesHighlight, Animate.Options.HUD);
    }

    public void TakeLife(Sequence sequence)
    {
        Lives--;
        Animate.UpdateText(livesText, Lives.ToString(), sequence, livesHighlight, Animate.Options.HUD);
        Animate.HighlightGraphic(livesIcon, sequence, livesHighlight, Animate.Options.HUD);
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
