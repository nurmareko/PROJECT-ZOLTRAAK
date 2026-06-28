using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private float survivalObjectiveSeconds = 180f;

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
        Time.timeScale = 1f;
        gameActive = true;
    }

    public void Update()
    {
        if (gameActive)
        {
            gameTime += Time.deltaTime;
            UIController.Instance.UpdateTimer(gameTime);

            if (gameTime >= survivalObjectiveSeconds)
            {
                CompleteSurvivalObjective();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                Pause();
            }
        }
    }

    public void GameOver()
    {
        if (!gameActive)
        {
            return;
        }

        gameActive = false;
        StartCoroutine(showGameOverScreen(  ));
    }

    IEnumerator showGameOverScreen()
    {
        yield return new WaitForSeconds(1.5f);
        UIController.Instance.ShowGameOver(gameTime);
        AudioController.Instance.PlaySound(AudioController.Instance.gameOver);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

     public void Pause()
    {
        if (!UIController.Instance.PausePanel.activeSelf && !UIController.Instance.gameoverPanel.activeSelf)
        {
            UIController.Instance.PausePanelOpen();
            Time.timeScale = 0f;
            AudioController.Instance.PlaySound(AudioController.Instance.pause);
        } else
        {
            UIController.Instance.PausePanelClose();
            Time.timeScale = 1f;
            AudioController.Instance.PlaySound(AudioController.Instance.unpause);
        }

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1f;
    }

    private void CompleteSurvivalObjective()
    {
        gameActive = false;
        gameTime = survivalObjectiveSeconds;
        UIController.Instance.UpdateTimer(gameTime);
        UIController.Instance.ShowVictory(gameTime, survivalObjectiveSeconds);
        Time.timeScale = 0f;
    }
}
