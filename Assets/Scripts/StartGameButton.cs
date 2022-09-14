using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    [SerializeField] Level debugLevel;
    public void StartGame() => Board.Instance.StartLevel(debugLevel);
}
