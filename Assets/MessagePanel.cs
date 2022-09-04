using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessagePanel : MonoBehaviour
{
    public static MessagePanel Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI message;

    private void Awake()
    {
        Instance = this;
        ShowMessage("");
    }


    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ShowMessage(string msg)
    {
        message.SetText(msg);
        Show();
    }

}
