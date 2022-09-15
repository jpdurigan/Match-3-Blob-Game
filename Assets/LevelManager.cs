using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Level[] levels;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private GameObject levelButtonsParent;
    [SerializeField] private float targetButtonSize = 250;
    
    private void Start()
    {
        CreateLevelMap();
    }

    private void CreateLevelMap()
    {
        foreach (Transform child in levelButtonsParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (var i = 0; i < levels.Length; i++)
        {
            GameObject button = Instantiate(levelButtonPrefab, Vector3.zero, Quaternion.identity, levelButtonsParent.transform);
            LevelButton levelButton = button.GetComponent<LevelButton>();
            levelButton.level = levels[i];
            levelButton.index = i+1;
        }

        RectTransform contentRT = levelButtonsParent.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, targetButtonSize * levels.Length);
    }
}
