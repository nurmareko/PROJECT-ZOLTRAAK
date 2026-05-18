using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text healthText;
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
}
