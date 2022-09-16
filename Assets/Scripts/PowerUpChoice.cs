using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PowerUpChoice : MonoBehaviour
{
    //uso una varibile statica da condividere con la scena principale per decidere che powerup usare
    public static int powerup;
    public Animator animator;
    public TextMeshProUGUI highScoreText;
    private int highScore;
     void Start()
    {
        Time.timeScale = 1;
        highScore = PlayerPrefs.GetInt("highscore", highScore);
        highScoreText.SetText("HIGH SCORE: \n\n" + System.Convert.ToString(highScore));
    }
    public void OnBombClick()
    {
        powerup = 0;
        StartCoroutine(LoadSceneAFterTransition());
    }

    public void OnFreezeClick()
    {
        powerup = 1;
        StartCoroutine(LoadSceneAFterTransition());
    }

    //Il cambio di scena avviene con una coroutine che triggera l'animazione di transizione
    private IEnumerator LoadSceneAFterTransition()
    {
        animator.SetBool("animateIn", true);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(1);
    }
}
