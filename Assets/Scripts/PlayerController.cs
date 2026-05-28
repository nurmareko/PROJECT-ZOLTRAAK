using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 2.5f;
    public Vector2 playerMoveDirection;
    public float maxHealth;
    public float currentHealth;
    public int experience;
    public int currentLevel;
    public int maxLevel;
    public List<int> playerLevels;

    private bool immune;
    [SerializeField] private float immunityDuration;
    [SerializeField] private float immunityTimer;

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

    void Start()
    {
        for (int i = playerLevels.Count; i < maxLevel; i++)
        {
            playerLevels.Add(Mathf.CeilToInt(playerLevels[playerLevels.Count - 1] * 1.1f + 15));
        }

        currentHealth = maxHealth;
        UIController.Instance.UpdateHealthSlider();
        UIController.Instance.UpdateExperienceSlider();
    }

    // Update is called once per frame
    void Update()
    {
        // Capture Keyboard input for movement
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        playerMoveDirection = new Vector2(inputX, inputY).normalized;

        // Trigger animation state
        animator.SetFloat("Move X", inputX);
        animator.SetFloat("Move Y", inputY);
        animator.SetBool("Is Moving", playerMoveDirection != Vector2.zero);

        if (immunityTimer > 0)
        {
            immunityTimer -= Time.deltaTime;
        } else
        {
            immune = false;
        }
    }

    void FixedUpdate()
    {
        rigidBody.linearVelocity = new Vector2(
            playerMoveDirection.x * moveSpeed,
            playerMoveDirection.y * moveSpeed
            );
    }

    public void takeDamage(float damage)
    {
        if (!immune)
        {
            immune = true;
            immunityTimer = immunityDuration;
            currentHealth -= damage;

            UIController.Instance.UpdateHealthSlider();

            if (currentHealth <= 0)
            {
                gameObject.SetActive(false);
                GameManager.Instance.GameOver();
            }
        }
    }

    public void GetExperience(int experienceToGet)
    {
        experience += experienceToGet;
        UIController.Instance.UpdateExperienceSlider();
    }
}
