using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Game : MonoBehaviour
{
    public Griglia grid;
    public int score;
    private int currentScore;
    private int highScore;
    public int timeInseconds;
    private float timer;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject gameUI;
    
    //In apertura prelevo l'highScore salvato
    void Start()
    {
        Time.timeScale = 1;
        gameOverMenu.SetActive(false);
        highScore = PlayerPrefs.GetInt("highscore", highScore);
        highScoreText.SetText(System.Convert.ToString(highScore));
    }

    //Se il tempo non è freezato faccio scorrere un timer e mostro nella UI
    //il tempo rimanente
    void Update()
    {
        if (!Griglia.stopTimer)
        {
            timer += Time.deltaTime;
            if (timeInseconds - timer <= 0)
                GameEnd();
            int minutes = (timeInseconds - (int)timer) / 60;
            int seconds = (timeInseconds - (int)timer) % 60;
            if (seconds >= 10)
                timeText.SetText("0" + System.Convert.ToString(minutes) + ":" + System.Convert.ToString(seconds));
            else
                timeText.SetText("0" + System.Convert.ToString(minutes) + ":0" + System.Convert.ToString(seconds));
        }
    }

    //quando il gioco finisce nasondo la gameUI e mostro la schermata di gameOver. Mostro il punteggio ottenuto e quello più alto
    //non setto timeScale a 0 perché pregiudicherei la transizione tra scene
    public void GameEnd()
    {
        grid.GameOver();
        gameUI.SetActive(false);
        gameOverMenu.SetActive(true);
        gameOverMenu.transform.Find("HighScore").GetComponent<TextMeshProUGUI>().text = System.Convert.ToString(highScore);
        gameOverMenu.transform.Find("YourScore").GetComponent<TextMeshProUGUI>().text = System.Convert.ToString(currentScore);
    }

    //quando il pezzo viene eliminato aggiorno il punteggio e se necessario anche il punteggio migliore
    public void OnPieceCleared(GamePiece piece)
    {
        currentScore = currentScore + piece.score;
        scoreText.SetText(System.Convert.ToString(currentScore));
        if (currentScore > highScore)
        {
            highScore = currentScore;
            highScoreText.SetText(System.Convert.ToString(currentScore));
            PlayerPrefs.SetInt("highscore", highScore);
            PlayerPrefs.Save();
        }
    }
}
