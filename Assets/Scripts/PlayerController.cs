using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const int DefaultFirstLevelExperience = 5;
    private const float HealthDisplayScale = 10f;

    public static PlayerController Instance;

    [System.Serializable]
    private class CharacterLoadout
    {
        public string characterName = "";
        public RuntimeAnimatorController animatorController = null;
        public Sprite characterSprite = null;
        public Weapon startingWeapon = null;
    }

    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float healthUpgradeAmount = 1f;
    [SerializeField] private float agilityUpgradeAmount = 0.25f;
    [SerializeField] private int characterIndex;
    [SerializeField] private GameObject[] characterPrefabs;
    [SerializeField] private List<CharacterLoadout> characterLoadouts = new List<CharacterLoadout>();
    public Vector2 playerMoveDirection;
    public Vector2 lastMoveDirection = Vector2.down;
    public float maxHealth;
    public float currentHealth;
    public int experience;
    public int currentLevel;
    public int maxLevel;
    public List<int> playerLevels;

    public  Weapon activeWeapon;
    public int CharacterIndex => characterIndex;

    private bool immune;
    private bool skillInvulnerable;
    [SerializeField] private float immunityDuration;
    [SerializeField] private float immunityTimer;
    [SerializeField] private float immunityBlinkInterval = 0.08f;
    [SerializeField] private float immunityBlinkAlpha = 0.35f;
    private Color originalSpriteColor = Color.white;
    private Coroutine immunityBlinkCoroutine;
    private float movementOverrideTimer;
    private Vector2 movementOverrideVelocity;

    void Awake()
    {
        if (ReplaceWithSelectedCharacter())
        {
            return;
        }

        if (Instance == null)
        {
            Instance = this;
            CacheSpriteColor();
            AssignCameraTarget();
        } else
        {
            Destroy(gameObject);
        }
    }

    private void AssignCameraTarget()
    {
        CinemachineCamera cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = transform;
        }
    }

    private bool ReplaceWithSelectedCharacter()
    {
        int selectedCharacterIndex = CharacterSelection.SelectedCharacterIndex;

        if (selectedCharacterIndex == characterIndex ||
            characterPrefabs == null ||
            selectedCharacterIndex < 0 ||
            selectedCharacterIndex >= characterPrefabs.Length ||
            characterPrefabs[selectedCharacterIndex] == null)
        {
            return false;
        }

        Instantiate(characterPrefabs[selectedCharacterIndex], transform.position, transform.rotation);
        Destroy(gameObject);
        return true;
    }

    void Start()
    {
        ApplySelectedCharacter();
        EnsurePlayerLevels();

        currentHealth = maxHealth;
        UIController.Instance.UpdateHealthSlider();
        UIController.Instance.UpdateExperienceSlider();
        UIController.Instance.UpdateLevelText();
    }

    // Update is called once per frame
    void Update()
    {
        // Capture Keyboard input for movement
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        playerMoveDirection = new Vector2(inputX, inputY).normalized;

        if (playerMoveDirection != Vector2.zero)
        {
            lastMoveDirection = playerMoveDirection;
        }

        if (inputX != 0)
        {
            spriteRenderer.flipX = inputX < 0;
        }

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
        if (movementOverrideTimer > 0f)
        {
            movementOverrideTimer -= Time.fixedDeltaTime;
            rigidBody.linearVelocity = movementOverrideVelocity;
            return;
        }

        rigidBody.linearVelocity = new Vector2(
            playerMoveDirection.x * moveSpeed,
            playerMoveDirection.y * moveSpeed
            );
    }

    public void takeDamage(float damage)
    {
        if (!immune && !skillInvulnerable)
        {
            immune = true;
            immunityTimer = immunityDuration;
            StartImmunityBlink();
            currentHealth = ClampHealth(currentHealth - damage);
            GameFeelFeedback.PlayPlayerHit(transform.position);

            UIController.Instance.UpdateHealthSlider();

            if (currentHealth <= 0)
            {
                gameObject.SetActive(false);
                GameManager.Instance.GameOver();
            }
        }
    }

    public void SetSkillInvulnerable(bool isInvulnerable)
    {
        skillInvulnerable = isInvulnerable;
    }

    public void SetMovementOverride(Vector2 velocity, float duration)
    {
        movementOverrideVelocity = velocity;
        movementOverrideTimer = Mathf.Max(0f, duration);
    }

    public void TeleportTo(Vector2 position)
    {
        movementOverrideTimer = 0f;

        if (rigidBody != null)
        {
            rigidBody.position = position;
            rigidBody.linearVelocity = Vector2.zero;
        }

        transform.position = position;
    }

    void OnDisable()
    {
        RestoreSpriteColor();
    }

    public void Heal(float healAmount)
    {
        if (healAmount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth = ClampHealth(currentHealth + healAmount);
        UIController.Instance.UpdateHealthSlider();
    }

    public void GetExperience(int experienceToGet)
    {
        if (experienceToGet <= 0)
        {
            return;
        }

        EnsurePlayerLevels();

        if (IsAtMaxLevel())
        {
            experience = 0;
            UIController.Instance.UpdateExperienceSlider();
            return;
        }

        experience += experienceToGet;

        if (experience >= GetCurrentExperienceRequirement())
        {
            LevelUp();
            return;
        }

        UIController.Instance.UpdateExperienceSlider();
    }

    public void LevelUp()
    {
        EnsurePlayerLevels();

        if (IsAtMaxLevel())
        {
            experience = 0;
            UIController.Instance.UpdateExperienceSlider();
            return;
        }

        experience -= GetCurrentExperienceRequirement();
        currentLevel++;
        UIController.Instance.UpdateLevelText();

        if (IsAtMaxLevel())
        {
            experience = 0;
            UIController.Instance.UpdateExperienceSlider();
            return;
        }

        UIController.Instance.UpdateExperienceSlider();
        UIController.Instance.LevelUpPanelOpen();
        UIController.Instance.ActivateLevelUpButtons(activeWeapon);
    }

    public void UpgradeHealth()
    {
        maxHealth += healthUpgradeAmount;
        currentHealth = ClampHealth(currentHealth + healthUpgradeAmount);
        UIController.Instance.UpdateHealthSlider();
    }

    public void UpgradeAgility()
    {
        moveSpeed += agilityUpgradeAmount;
    }

    public string GetHealthUpgradeDescription()
    {
        return "Increase max health by " + FormatHealthDisplayValue(healthUpgradeAmount) +
            " and heal for the same amount.";
    }

    public string GetAgilityUpgradeDescription()
    {
        return "Increase movement speed by " + FormatUpgradeValue(agilityUpgradeAmount) + ".";
    }

    public int GetCurrentExperienceRequirement()
    {
        EnsurePlayerLevels();

        int levelIndex = Mathf.Clamp(currentLevel - 1, 0, playerLevels.Count - 1);
        return playerLevels[levelIndex];
    }

    public bool IsAtMaxLevel()
    {
        EnsurePlayerLevels();

        return currentLevel >= maxLevel;
    }

    private string FormatUpgradeValue(float value)
    {
        return value.ToString("0.##");
    }

    private string FormatHealthDisplayValue(float value)
    {
        return Mathf.RoundToInt(value * HealthDisplayScale).ToString();
    }

    private float ClampHealth(float health)
    {
        return Mathf.Clamp(health, 0f, Mathf.Max(0f, maxHealth));
    }

    private void EnsurePlayerLevels()
    {
        if (maxLevel < 1)
        {
            maxLevel = 1;
        }

        currentLevel = Mathf.Clamp(currentLevel, 1, maxLevel);

        if (playerLevels == null)
        {
            playerLevels = new List<int>();
        }

        if (playerLevels.Count == 0)
        {
            playerLevels.Add(DefaultFirstLevelExperience);
        }

        for (int i = playerLevels.Count; i < maxLevel; i++)
        {
            playerLevels.Add(Mathf.CeilToInt(playerLevels[playerLevels.Count - 1] * 1.1f + 15));
        }
    }

    private void ApplySelectedCharacter()
    {
        int selectedCharacterIndex = CharacterSelection.SelectedCharacterIndex;
        Weapon[] availableWeapons = GetComponentsInChildren<Weapon>(true);
        CharacterLoadout loadout = GetLoadout(selectedCharacterIndex);
        Weapon selectedWeapon = GetSelectedWeapon(selectedCharacterIndex, loadout, availableWeapons);

        if (loadout != null)
        {
            if (loadout.animatorController != null)
            {
                animator.runtimeAnimatorController = loadout.animatorController;
            }

            if (loadout.characterSprite != null)
            {
                spriteRenderer.sprite = loadout.characterSprite;
            }
        }

        if (selectedWeapon == null)
        {
            return;
        }

        activeWeapon = selectedWeapon;

        foreach (Weapon weapon in availableWeapons)
        {
            weapon.gameObject.SetActive(weapon == activeWeapon);
        }
    }

    private CharacterLoadout GetLoadout(int selectedCharacterIndex)
    {
        if (selectedCharacterIndex >= 0 && selectedCharacterIndex < characterLoadouts.Count)
        {
            return characterLoadouts[selectedCharacterIndex];
        }

        return null;
    }

    private Weapon GetSelectedWeapon(
        int selectedCharacterIndex,
        CharacterLoadout loadout,
        Weapon[] availableWeapons)
    {
        if (loadout != null && loadout.startingWeapon != null)
        {
            return loadout.startingWeapon;
        }

        if (availableWeapons.Length > 0)
        {
            int weaponIndex = Mathf.Clamp(selectedCharacterIndex, 0, availableWeapons.Length - 1);
            return availableWeapons[weaponIndex];
        }

        return activeWeapon;
    }

    private void StartImmunityBlink()
    {
        if (!isActiveAndEnabled || !EnsureSpriteRenderer())
        {
            return;
        }

        if (immunityBlinkCoroutine != null)
        {
            StopCoroutine(immunityBlinkCoroutine);
        }

        immunityBlinkCoroutine = StartCoroutine(ImmunityBlinkRoutine());
    }

    private IEnumerator ImmunityBlinkRoutine()
    {
        bool dimSprite = false;

        while (immune && immunityTimer > 0 && isActiveAndEnabled)
        {
            if (spriteRenderer == null)
            {
                yield break;
            }

            Color blinkColor = originalSpriteColor;
            blinkColor.a = dimSprite ? originalSpriteColor.a : immunityBlinkAlpha;
            spriteRenderer.color = blinkColor;
            dimSprite = !dimSprite;

            yield return new WaitForSeconds(immunityBlinkInterval);
        }

        RestoreSpriteColor();
        immunityBlinkCoroutine = null;
    }

    private void CacheSpriteColor()
    {
        if (EnsureSpriteRenderer())
        {
            originalSpriteColor = spriteRenderer.color;
        }
    }

    private void RestoreSpriteColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor;
        }
    }

    private bool EnsureSpriteRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        return spriteRenderer != null;
    }
}
