using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class ScoreCounter : MonoBehaviour
{
    public static ScoreCounter Instance { get; private set; }

    public int Score { get; private set; } = -1;
    // public string ScoreFormatted => $"{Score:D8}";
    public int TurnsLeft { get; private set; } = -1;
    // public string TurnsLeftFormatted => $"{TurnsLeft:D2}";
    public int TargetsLeft { get; private set; } = -1;
    public int Lives { get; private set; } = -1;
    // public string TargetsLeftFormatted => $"{TargetsLeft:D2}";
    // public int Lives
    // {
    //     get => _lives;
    //     set
    //     {
    //         if (_lives == value) return;
    //         _lives = value;
    //         // livesText.SetText($"{_lives:D2}");
    //         Animate.AsyncUpdateText(livesText, $"{_lives:D2}");
    //     }
    // }

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI turnsText;
    [SerializeField] private TextMeshProUGUI targetsText;

    private Level currentLevel = null;

    private void Awake()
    {
        Instance = this;
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

        Lives = 1;
    }

    public void AddToScore(Item.Types type, Sequence sequence)
    {
        if (type == currentLevel.goalType)
        {
            TargetsLeft--;
            Animate.UpdateText(targetsText, TargetsLeft.ToString(), sequence);
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

}
