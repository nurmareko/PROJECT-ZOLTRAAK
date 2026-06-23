using UnityEngine;

public static class CharacterSelection
{
    private const string SelectedCharacterKey = "SelectedCharacterIndex";
    public const int CharacterCount = 3;

    public static int SelectedCharacterIndex
    {
        get
        {
            int selectedCharacterIndex = PlayerPrefs.GetInt(SelectedCharacterKey, 0);
            return Mathf.Clamp(selectedCharacterIndex, 0, CharacterCount - 1);
        }
    }

    public static void SelectCharacter(int characterIndex)
    {
        int selectedCharacterIndex = Mathf.Clamp(characterIndex, 0, CharacterCount - 1);
        PlayerPrefs.SetInt(SelectedCharacterKey, selectedCharacterIndex);
        PlayerPrefs.Save();
    }
}
