using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int Money;
    public int startMoney = 100;

    public static int Lives;
    public int startLives = 20;

    public static bool GameIsOver = false; 

    void Start()
    {
        GameIsOver = false; 
        Money = startMoney;
        Lives = startLives;
    }

    void Update()
    {
        if (GameIsOver)
            return;

        if (Lives <= 0)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        GameIsOver = true; 
        Debug.Log("GAME OVER!"); 
    }
}