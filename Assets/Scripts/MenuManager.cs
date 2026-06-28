using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private AudioClip startGameClickSound;
    [SerializeField] private float fallbackStartGameDelay = 1.35f;

    private bool isStartingGame;

    public void NewGame()
    {
        StartGameWithCharacter(CharacterSelection.SelectedCharacterIndex);
    }

    public void NewGameAsCharacterA()
    {
        StartGameWithCharacter(0);
    }

    public void NewGameAsCharacterB()
    {
        StartGameWithCharacter(1);
    }

    public void NewGameAsCharacterC()
    {
        StartGameWithCharacter(2);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
        }
    }

    public void HideHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
    }

    private void StartGameWithCharacter(int characterIndex)
    {
        if (isStartingGame)
        {
            return;
        }

        CharacterSelection.SelectCharacter(characterIndex);
        StartCoroutine(LoadGameAfterClickSound());
    }

    private IEnumerator LoadGameAfterClickSound()
    {
        isStartingGame = true;

        float delay = startGameClickSound != null ? startGameClickSound.length : fallbackStartGameDelay;
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        SceneManager.LoadScene(gameSceneName);
    }
}
