using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [Header("Colors")]
    public Color availableLevelColorBG = Color.white;
    public Color availableLevelColorText = Color.white;
    [Space]
    public Color unavailableLevelColorBG = Color.white;
    public Color unavailableLevelColorText = Color.white;
    [Space]
    public Color completeLevelColorBG = Color.white;
    public Color completeLevelColorText = Color.white;

    public Level level
    {
        get
        {
            return _level;
        }

        set
        {
            _level = value;
            if (_level.isCompleted)
            {
                button.interactable = true;
                buttonBG.color = completeLevelColorBG;
                indexLabel.color = completeLevelColorText;
            }
            else if (_level.isAvailable)
            {
                button.interactable = true;
                buttonBG.color = availableLevelColorBG;
                indexLabel.color = availableLevelColorText;
            }
            else
            {
                button.interactable = false;
                buttonBG.color = unavailableLevelColorBG;
                indexLabel.color = unavailableLevelColorText;
            }
        }
    }
    private Level _level;
    public int index
    {
        get
        {
            return _index;
        }

        set
        {
            _index = value;
            indexLabel.SetText(_index.ToString());
        }
    }
    private int _index;

    [Space]
    [Header("Game Objects")]
    [SerializeField] private Button button;
    [SerializeField] private Image buttonBG;
    [SerializeField] private TextMeshProUGUI indexLabel;

    public void OnPressed()
    {
        Board.Instance.StartLevel(level);
    }
}
