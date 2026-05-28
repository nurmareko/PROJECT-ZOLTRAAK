using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider playerExperienceSlider;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private TMP_Text timerText;
    public GameObject gameoverPanel;
    public GameObject PausePanel;

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

    public void UpdateTimer (float timer)
    {
        float minute = Mathf.FloorToInt(timer / 60f);
        float second = Mathf.FloorToInt(timer % 60f);

        timerText.text = minute + ":" + second.ToString("00");
    }
}
