using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private const int RequiredLevelUpButtonCount = 3;
    private const float LevelUpButtonSpacing = 360f;
    private const float LevelUpPanelPulseDuration = 0.22f;
    private const float LevelUpPanelStartScale = 0.96f;
    private const float LevelUpPanelPeakScale = 1.04f;
    private const float LevelTextPulseDuration = 0.18f;
    private const float LevelTextPeakScale = 1.12f;

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

    private Coroutine levelUpPanelPulseCoroutine;
    private Coroutine levelTextPulseCoroutine;
    private Vector3 levelUpPanelBaseScale = Vector3.one;
    private Vector3 levelTextBaseScale = Vector3.one;
    private bool hasLevelUpPanelBaseScale;
    private bool hasLevelTextBaseScale;
    private int shownLevel;

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
        ApplyReadabilityDefaults();
    }

    public void UpdateHealthSlider()
    {
        playerHealthSlider.maxValue = PlayerController.Instance.maxHealth;
        playerHealthSlider.value = PlayerController.Instance.currentHealth;
        healthText.text = "HP " + FormatValue(playerHealthSlider.value) + " / " + FormatValue(playerHealthSlider.maxValue);
    }

    public void UpdateExperienceSlider()
    {
         PlayerController player = PlayerController.Instance;
         int experienceRequirement = player.GetCurrentExperienceRequirement();

         playerExperienceSlider.maxValue = experienceRequirement;
         playerExperienceSlider.value = Mathf.Min(player.experience, experienceRequirement);
         experienceText.text = player.IsAtMaxLevel()
            ? "XP MAX"
            : "XP " + FormatValue(playerExperienceSlider.value) + " / " + FormatValue(playerExperienceSlider.maxValue);
    }

    public void UpdateLevelText()
    {
        EnsureLevelText();

        if (levelText != null)
        {
            int currentLevel = PlayerController.Instance.currentLevel;
            bool shouldPulse = shownLevel > 0 && currentLevel > shownLevel;

            levelText.text = "Level " + currentLevel;
            shownLevel = currentLevel;

            if (shouldPulse)
            {
                PlayLevelTextPulse();
            }
        }
    }

    public void UpdateTimer(float timer)
    {
        timerText.text = "Time " + FormatTime(timer);
    }

    public void ShowGameOver(float survivedTime)
    {
        ApplyGameOverReadability();
        UpdateSurvivedTime(survivedTime);
        gameoverPanel.SetActive(true);
    }

    public void LevelUpPanelOpen()
    {
        EnsureLevelUpButtons();
        levelUpPanel.SetActive(true);
        Time.timeScale = 0f;
        PlayLevelUpPanelPulse();
    }

    public void LevelUpPanelClose()
    {
        StopLevelUpPanelPulse();
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
        StyleHudText(levelText, 20f, 42f);
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

    private void PlayLevelUpPanelPulse()
    {
        if (levelUpPanel == null)
        {
            return;
        }

        RectTransform panelTransform = levelUpPanel.GetComponent<RectTransform>();

        if (panelTransform == null)
        {
            return;
        }

        if (!hasLevelUpPanelBaseScale)
        {
            levelUpPanelBaseScale = panelTransform.localScale;
            hasLevelUpPanelBaseScale = true;
        }

        StopLevelUpPanelPulse();
        levelUpPanelPulseCoroutine = StartCoroutine(PulseLevelUpPanel(panelTransform));
    }

    private void StopLevelUpPanelPulse()
    {
        if (levelUpPanelPulseCoroutine != null)
        {
            StopCoroutine(levelUpPanelPulseCoroutine);
            levelUpPanelPulseCoroutine = null;
        }

        if (hasLevelUpPanelBaseScale && levelUpPanel != null)
        {
            levelUpPanel.transform.localScale = levelUpPanelBaseScale;
        }
    }

    private IEnumerator PulseLevelUpPanel(RectTransform panelTransform)
    {
        float elapsed = 0f;
        Vector3 startScale = levelUpPanelBaseScale * LevelUpPanelStartScale;
        Vector3 peakScale = levelUpPanelBaseScale * LevelUpPanelPeakScale;

        while (elapsed < LevelUpPanelPulseDuration)
        {
            float t = elapsed / LevelUpPanelPulseDuration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            panelTransform.localScale = t < 0.5f
                ? Vector3.Lerp(startScale, peakScale, eased * 2f)
                : Vector3.Lerp(peakScale, levelUpPanelBaseScale, (eased - 0.5f) * 2f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        panelTransform.localScale = levelUpPanelBaseScale;
        levelUpPanelPulseCoroutine = null;
    }

    private void UpdateSurvivedTime(float survivedTime)
    {
        EnsureSurvivedTimeText();

        if (survivedTimeText != null)
        {
            survivedTimeText.text = "Survived Time: " + FormatTime(survivedTime);
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
        survivedTimeText.fontSizeMin = 22f;
        survivedTimeText.fontSizeMax = 42f;
        survivedTimeText.fontStyle = FontStyles.Bold;
        survivedTimeText.raycastTarget = false;

        if (titleText != null)
        {
            survivedTimeText.font = titleText.font;
            survivedTimeText.fontSharedMaterial = titleText.fontSharedMaterial;
            survivedTimeText.color = titleText.color;
        }
    }

    private void ApplyReadabilityDefaults()
    {
        StyleHudText(healthText, 18f, 38f);
        StyleHudText(experienceText, 18f, 38f);
        StyleHudText(timerText, 30f, 92f);

        EnsureLevelText();
        StyleHudText(levelText, 20f, 42f);
        ApplyPanelReadability(levelUpPanel);
        ApplyGameOverReadability();
    }

    private void ApplyPanelReadability(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        TMP_Text[] panelTexts = panel.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text panelText in panelTexts)
        {
            if (panelText.name == "Title")
            {
                StylePanelText(panelText, 36f, 96f, FontStyles.Bold);
            }
            else if (panelText.name == "Subtitle")
            {
                StylePanelText(panelText, 22f, 52f, FontStyles.Bold);
            }
        }
    }

    private void ApplyGameOverReadability()
    {
        if (gameoverPanel == null)
        {
            return;
        }

        ApplyPanelReadability(gameoverPanel);

        TMP_Text[] gameOverTexts = gameoverPanel.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text gameOverText in gameOverTexts)
        {
            if (gameOverText.transform.parent != null && gameOverText.transform.parent.name == "Restart Button")
            {
                gameOverText.text = "Restart Run";
                StyleButtonText(gameOverText);
            }
        }
    }

    private void StyleHudText(TMP_Text text, float minSize, float maxSize)
    {
        if (text == null)
        {
            return;
        }

        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    private void StylePanelText(TMP_Text text, float minSize, float maxSize, FontStyles style)
    {
        if (text == null)
        {
            return;
        }

        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    private void StyleButtonText(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        text.enableAutoSizing = true;
        text.fontSizeMin = 22f;
        text.fontSizeMax = 38f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
    }

    private void PlayLevelTextPulse()
    {
        if (levelText == null)
        {
            return;
        }

        RectTransform textTransform = levelText.GetComponent<RectTransform>();

        if (textTransform == null)
        {
            return;
        }

        if (!hasLevelTextBaseScale)
        {
            levelTextBaseScale = textTransform.localScale;
            hasLevelTextBaseScale = true;
        }

        if (levelTextPulseCoroutine != null)
        {
            StopCoroutine(levelTextPulseCoroutine);
        }

        levelTextPulseCoroutine = StartCoroutine(PulseLevelText(textTransform));
    }

    private IEnumerator PulseLevelText(RectTransform textTransform)
    {
        float elapsed = 0f;
        Vector3 peakScale = levelTextBaseScale * LevelTextPeakScale;

        while (elapsed < LevelTextPulseDuration)
        {
            float t = elapsed / LevelTextPulseDuration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            textTransform.localScale = t < 0.5f
                ? Vector3.Lerp(levelTextBaseScale, peakScale, eased * 2f)
                : Vector3.Lerp(peakScale, levelTextBaseScale, (eased - 0.5f) * 2f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        textTransform.localScale = levelTextBaseScale;
        levelTextPulseCoroutine = null;
    }

    private string FormatValue(float value)
    {
        return Mathf.Approximately(value, Mathf.Round(value))
            ? Mathf.RoundToInt(value).ToString()
            : value.ToString("0.#");
    }

    private string FormatTime(float timer)
    {
        int minute = Mathf.FloorToInt(timer / 60f);
        int second = Mathf.FloorToInt(timer % 60f);

        return minute + ":" + second.ToString("00");
    }
}
