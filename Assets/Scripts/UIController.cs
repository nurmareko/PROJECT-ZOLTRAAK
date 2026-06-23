using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private const int RequiredLevelUpButtonCount = 3;
    private const float LevelUpButtonSpacing = 360f;

    public static UIController Instance;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider playerExperienceSlider;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private TMP_Text timerText;
    public GameObject gameoverPanel;
    public GameObject levelUpPanel;
    public GameObject PausePanel;

    public LevelUpButton[] levelUpButtons; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
            return;
        }

        EnsureLevelUpButtons();
    }

    public void UpdateHealthSlider()
    {
        playerHealthSlider.maxValue = PlayerController.Instance.maxHealth;
        playerHealthSlider.value = PlayerController.Instance.currentHealth;
        healthText.text = playerHealthSlider.value + " / " + playerHealthSlider.maxValue;
    }

    public void UpdateExperienceSlider()
    {
         playerExperienceSlider.maxValue = PlayerController.Instance.playerLevels[PlayerController.Instance.currentLevel - 1];
         playerExperienceSlider.value = PlayerController.Instance.experience ;
         experienceText.text = playerExperienceSlider.value + " / " + playerExperienceSlider.maxValue;
    }

    public void UpdateTimer(float timer)
    {
        float minute = Mathf.FloorToInt(timer / 60f);
        float second = Mathf.FloorToInt(timer % 60f);

        timerText.text = minute + ":" + second.ToString("00");
    }

    public void LevelUpPanelOpen()
    {
        EnsureLevelUpButtons();
        levelUpPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void LevelUpPanelClose()
    {
        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ActivateLevelUpButtons(Weapon activeWeapon)
    {
        EnsureLevelUpButtons();

        if (levelUpButtons.Length < RequiredLevelUpButtonCount)
        {
            return;
        }

        PlayerController player = PlayerController.Instance;

        levelUpButtons[0].ActivateButton(activeWeapon);
        levelUpButtons[1].ActivateButton(
            "Max Health",
            player.GetHealthUpgradeDescription(),
            null,
            player.UpgradeHealth);
        levelUpButtons[2].ActivateButton(
            "Agility",
            player.GetAgilityUpgradeDescription(),
            null,
            player.UpgradeAgility);
    }

    private void EnsureLevelUpButtons()
    {
        if (levelUpButtons == null || levelUpButtons.Length == 0 || levelUpButtons[0] == null)
        {
            return;
        }

        if (levelUpButtons.Length < RequiredLevelUpButtonCount)
        {
            LevelUpButton[] expandedButtons = new LevelUpButton[RequiredLevelUpButtonCount];

            for (int i = 0; i < expandedButtons.Length; i++)
            {
                if (i < levelUpButtons.Length && levelUpButtons[i] != null)
                {
                    expandedButtons[i] = levelUpButtons[i];
                }
                else
                {
                    expandedButtons[i] = Instantiate(levelUpButtons[0], levelUpButtons[0].transform.parent);
                    expandedButtons[i].name = "Level Up Button " + (i + 1);
                }
            }

            levelUpButtons = expandedButtons;
        }

        ArrangeLevelUpButtons();
    }

    private void ArrangeLevelUpButtons()
    {
        float centerOffset = (RequiredLevelUpButtonCount - 1) * 0.5f;

        for (int i = 0; i < levelUpButtons.Length; i++)
        {
            if (levelUpButtons[i] == null)
            {
                continue;
            }

            RectTransform buttonTransform = levelUpButtons[i].GetComponent<RectTransform>();

            if (buttonTransform == null)
            {
                continue;
            }

            Vector2 position = buttonTransform.anchoredPosition;
            position.x = (i - centerOffset) * LevelUpButtonSpacing;
            buttonTransform.anchoredPosition = position;
        }
    }
}
