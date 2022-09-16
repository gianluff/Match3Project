using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameUI;
    public Animator animator;

    //in quanto panel appartenenti alla stessa scena mi limito a gestire la apparizione
    //del menu di pausa e di gameOver con semplic SetActive
    void Start()
    {
        pauseMenu.SetActive(false);
    }

    //Time.timeScale a 0 blocca tutti gli eventi che coinvolgono il tempo (timer e animazioni).
    //necessario quando si apre il menu di pausa
    public void OnPauseButton()
    {
        gameUI.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void OnResumeGameButton()
    {
        pauseMenu.SetActive(false);
        gameUI.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnNewGameButton()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        StartCoroutine(LoadSceneAFterTransition(1));
    }

    public void OnTitleScreenButton()
    {
        Time.timeScale = 1;
        StartCoroutine(LoadSceneAFterTransition(0));
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    //il cambio di scena avviene tramite la chiamata di una Coroutine per 
    //permettere un'animazione. Setto il parametro animateIn che fa da
    //trigger per l'animazione di transizione e aspetto due secondi per
    //l'effettivo cambio di scena
    private IEnumerator LoadSceneAFterTransition(int scene)
    {
        animator.SetBool("animateIn", true);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(scene);
    }
}
