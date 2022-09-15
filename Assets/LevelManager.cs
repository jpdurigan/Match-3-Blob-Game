using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private List<Level> levels;
    [Space]
    [SerializeField] private GameObject boardGameObject;
    [Space]
    [SerializeField] private GameObject levelsGameObject;
    [SerializeField] private GameObject levelButtonsParent;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private float targetButtonSize = 160;

    private Level currentLevel = null;
    private int currentLevelIdx = -1;

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        foreach(Level level in levels) level.Reset();
        levels[0].isAvailable = true;
        ShowLevels();
    }

    public void Retry()
    {
        StartLevel(currentLevel);
    }

    public void Next()
    {
        Level nextLevel = levels[currentLevelIdx + 1];
        StartLevel(nextLevel);
    }

    public void LevelSelect()
    {
        currentLevel = null;
        ShowLevels();
    }

    public void StartLevel(Level level)
    {
        currentLevel = level;
        currentLevelIdx = levels.IndexOf(level);
        ShowBoard();
        Board.Instance.StartLevel(level);
    }

    public void PlayerWon()
    {
        currentLevel.isCompleted = true;
        if (HasNextLevel()) levels[currentLevelIdx + 1].isAvailable = true;
    }

    public bool HasNextLevel() => currentLevelIdx + 1 < levels.Count;

    private void CreateLevelMap()
    {
        foreach (Transform child in levelButtonsParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (var i = 0; i < levels.Count; i++)
        {
            GameObject button = Instantiate(levelButtonPrefab, Vector3.zero, Quaternion.identity, levelButtonsParent.transform);
            LevelButton levelButton = button.GetComponent<LevelButton>();
            levelButton.level = levels[i];
            levelButton.index = i+1;
        }

        RectTransform contentRT = levelButtonsParent.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, targetButtonSize * levels.Count);
    }

    private void ShowLevels()
    {
        boardGameObject.SetActive(false);
        levelsGameObject.SetActive(true);
        CreateLevelMap();
    }

    private void ShowBoard()
    {
        boardGameObject.SetActive(true);
        levelsGameObject.SetActive(false);
    }
}
