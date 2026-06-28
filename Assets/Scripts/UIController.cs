using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private const int RequiredLevelUpButtonCount = 3;
    private const float LevelUpPanelPulseDuration = 0.22f;
    private const float LevelUpPanelStartScale = 0.96f;
    private const float LevelUpPanelPeakScale = 1.04f;
    private const float LevelTextPulseDuration = 0.18f;
    private const float LevelTextPeakScale = 1.12f;

    public static UIController Instance;

    [Header("HUD")]
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider playerExperienceSlider;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timerText;

    [Header("Panels")]
    [SerializeField] private TMP_Text survivedTimeText;
    public GameObject gameoverPanel;
    public GameObject levelUpPanel;
    public GameObject PausePanel;

    [Header("Level Up Choices")]
    public LevelUpButton[] levelUpButtons;
    [SerializeField] private Sprite healthUpgradeIcon;
    [SerializeField] private Sprite agilityUpgradeIcon;

    private Coroutine levelUpPanelPulseCoroutine;
    private Coroutine levelTextPulseCoroutine;
    private Vector3 levelUpPanelBaseScale = Vector3.one;
    private Vector3 levelTextBaseScale = Vector3.one;
    private bool hasLevelUpPanelBaseScale;
    private bool hasLevelTextBaseScale;
    private int shownLevel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateHealthSlider()
    {
        if (playerHealthSlider == null || healthText == null || PlayerController.Instance == null)
        {
            return;
        }

        playerHealthSlider.maxValue = PlayerController.Instance.maxHealth;
        playerHealthSlider.value = PlayerController.Instance.currentHealth;
        healthText.text = "HP " + FormatValue(playerHealthSlider.value) + " / " + FormatValue(playerHealthSlider.maxValue);
    }

    public void UpdateExperienceSlider()
    {
        if (playerExperienceSlider == null || experienceText == null || PlayerController.Instance == null)
        {
            return;
        }

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
        if (levelText == null || PlayerController.Instance == null)
        {
            return;
        }

        int currentLevel = PlayerController.Instance.currentLevel;
        bool shouldPulse = shownLevel > 0 && currentLevel > shownLevel;

        levelText.text = "Level " + currentLevel;
        shownLevel = currentLevel;

        if (shouldPulse)
        {
            PlayLevelTextPulse();
        }
    }

    public void UpdateTimer(float timer)
    {
        if (timerText != null)
        {
            timerText.text = "Time " + FormatTime(timer);
        }
    }

    public void ShowGameOver(float survivedTime)
    {
        UpdateSurvivedTime(survivedTime);

        if (gameoverPanel != null)
        {
            gameoverPanel.SetActive(true);
        }
    }

    public void LevelUpPanelOpen()
    {
        if (levelUpPanel == null)
        {
            return;
        }

        levelUpPanel.SetActive(true);
        Time.timeScale = 0f;
        PlayLevelUpPanelPulse();
    }

    public void LevelUpPanelClose()
    {
        StopLevelUpPanelPulse();

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void ActivateLevelUpButtons(Weapon activeWeapon)
    {
        if (levelUpButtons == null || levelUpButtons.Length < RequiredLevelUpButtonCount || PlayerController.Instance == null)
        {
            return;
        }

        PlayerController player = PlayerController.Instance;

        levelUpButtons[0].ActivateButton(activeWeapon);
        levelUpButtons[1].ActivateButton(
            "Max Health",
            player.GetHealthUpgradeDescription(),
            healthUpgradeIcon,
            player.UpgradeHealth);
        levelUpButtons[2].ActivateButton(
            "Agility",
            player.GetAgilityUpgradeDescription(),
            agilityUpgradeIcon,
            player.UpgradeAgility);
    }

    private void PlayLevelUpPanelPulse()
    {
        RectTransform panelTransform = levelUpPanel == null
            ? null
            : levelUpPanel.GetComponent<RectTransform>();

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
        if (survivedTimeText != null)
        {
            survivedTimeText.text = "Survived Time: " + FormatTime(survivedTime);
        }
    }

    private void PlayLevelTextPulse()
    {
        RectTransform textTransform = levelText == null
            ? null
            : levelText.GetComponent<RectTransform>();

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
