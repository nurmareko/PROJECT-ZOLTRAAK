using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private GameObject howToPlayPanel;

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
        CharacterSelection.SelectCharacter(characterIndex);
        SceneManager.LoadScene(gameSceneName);
    }
}
