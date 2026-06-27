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
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text survivedTimeText;
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
         PlayerController player = PlayerController.Instance;
         int experienceRequirement = player.GetCurrentExperienceRequirement();

         playerExperienceSlider.maxValue = experienceRequirement;
         playerExperienceSlider.value = Mathf.Min(player.experience, experienceRequirement);
         experienceText.text = player.IsAtMaxLevel()
            ? "MAX"
            : playerExperienceSlider.value + " / " + playerExperienceSlider.maxValue;
    }

    public void UpdateLevelText()
    {
        EnsureLevelText();

        if (levelText != null)
        {
            levelText.text = "Level " + PlayerController.Instance.currentLevel;
        }
    }

    public void UpdateTimer(float timer)
    {
        timerText.text = FormatTime(timer);
    }

    public void ShowGameOver(float survivedTime)
    {
        UpdateSurvivedTime(survivedTime);
        gameoverPanel.SetActive(true);
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

    private void EnsureLevelText()
    {
        if (levelText != null || timerText == null)
        {
            return;
        }

        GameObject textObject = new GameObject("Level Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(timerText.transform.parent, false);

        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchorMin = new Vector2(0.5f, 1f);
        textTransform.anchorMax = new Vector2(0.5f, 1f);
        textTransform.anchoredPosition = new Vector2(0f, -140f);
        textTransform.sizeDelta = new Vector2(260f, 54f);
        textTransform.pivot = new Vector2(0.5f, 0.5f);

        levelText = textObject.GetComponent<TMP_Text>();
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.enableAutoSizing = true;
        levelText.fontSizeMin = 18f;
        levelText.fontSizeMax = 40f;
        levelText.raycastTarget = false;
        levelText.font = timerText.font;
        levelText.fontSharedMaterial = timerText.fontSharedMaterial;
        levelText.color = timerText.color;
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

    private void UpdateSurvivedTime(float survivedTime)
    {
        EnsureSurvivedTimeText();

        if (survivedTimeText != null)
        {
            survivedTimeText.text = "Survived: " + FormatTime(survivedTime);
        }
    }

    private void EnsureSurvivedTimeText()
    {
        if (survivedTimeText != null || gameoverPanel == null)
        {
            return;
        }

        TMP_Text[] gameOverTexts = gameoverPanel.GetComponentsInChildren<TMP_Text>(true);
        TMP_Text titleText = null;

        foreach (TMP_Text gameOverText in gameOverTexts)
        {
            if (gameOverText.name == "Survived Time Text")
            {
                survivedTimeText = gameOverText;
                return;
            }

            if (gameOverText.name == "Title")
            {
                titleText = gameOverText;
            }
        }

        GameObject textObject = new GameObject("Survived Time Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(gameoverPanel.transform, false);

        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchorMin = new Vector2(0.5f, 0.5f);
        textTransform.anchorMax = new Vector2(0.5f, 0.5f);
        textTransform.anchoredPosition = new Vector2(0f, 58f);
        textTransform.sizeDelta = new Vector2(520f, 44f);
        textTransform.pivot = new Vector2(0.5f, 0.5f);

        survivedTimeText = textObject.GetComponent<TMP_Text>();
        survivedTimeText.alignment = TextAlignmentOptions.Center;
        survivedTimeText.enableAutoSizing = true;
        survivedTimeText.fontSizeMin = 18f;
        survivedTimeText.fontSizeMax = 36f;
        survivedTimeText.raycastTarget = false;

        if (titleText != null)
        {
            survivedTimeText.font = titleText.font;
            survivedTimeText.fontSharedMaterial = titleText.fontSharedMaterial;
            survivedTimeText.color = titleText.color;
        }
    }

    private string FormatTime(float timer)
    {
        int minute = Mathf.FloorToInt(timer / 60f);
        int second = Mathf.FloorToInt(timer % 60f);

        return minute + ":" + second.ToString("00");
    }
}
