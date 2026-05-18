using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            Pause();
        }
    }

    public void GameOver()
    {
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
        if (UIController.Instance.PausePanel.activeSelf == false)
        {
            UIController.Instance.PausePanel.SetActive(true);
        } else
        {
            UIController.Instance.PausePanel.SetActive(false);
        }
    }
}
