using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";

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

    private void StartGameWithCharacter(int characterIndex)
    {
        CharacterSelection.SelectCharacter(characterIndex);
        SceneManager.LoadScene(gameSceneName);
    }
}
