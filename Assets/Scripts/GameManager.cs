using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public float gameTime;
    private bool gameActive; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        gameActive = true;
    }

    public void Update()
    {
        if (gameActive)
        {
            gameTime += Time.deltaTime;
            UIController.Instance.UpdateTimer(gameTime);

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                Pause();
            }   
        }
    }

    public void GameOver()
    {
        gameActive = false;
        StartCoroutine(showGameOverScreen(  ));
    }

    IEnumerator showGameOverScreen()
    {
        yield return new WaitForSeconds(1.5f);
        UIController.Instance.gameoverPanel.SetActive(true); 
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game"); 
    }

     public void Pause()
    {
        if (!UIController.Instance.PausePanel.activeSelf && !UIController.Instance.gameoverPanel.activeSelf)
        {
            UIController.Instance.PausePanel.SetActive(true);
            Time.timeScale = 0f;
        } else
        {
            UIController.Instance.PausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
        
    }

    public void QuitGame()
    {
        Application.Quit(); 
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
