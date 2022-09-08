using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreCounter : MonoBehaviour
{
    public static ScoreCounter Instance { get; private set; }

    private int _score = -1;
    public int Score
    {
        get => _score;
        set
        {
            if (_score == value) return;
            _score = value;
            scoreText.SetText($"{_score:D8}");
        }
    }

    private int _lives = -1;
    public int Lives
    {
        get => _lives;
        set
        {
            if (_lives == value) return;
            _lives = value;
            livesText.SetText($"{_lives:D2}");
        }
    }

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;

    private void Awake()
    {
        Instance = this;
    }
}
