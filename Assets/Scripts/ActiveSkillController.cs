using System.Collections.Generic;
using UnityEngine;

public class ActiveSkillController : MonoBehaviour
{
    private enum SkillType
    {
        PlayerAShieldPush,
        PlayerBRepel,
        PlayerCLunge
    }

    [Header("Input")]
    [SerializeField] private KeyCode activationKey = KeyCode.Space;

    [Header("Player A - Shield Push")]
    [SerializeField] private string playerASkillName = "Shield";
    [SerializeField] private float playerACooldown = 15f;
    [SerializeField] private float playerADuration = 3f;
    [SerializeField] private float playerAPushRadius = 1.15f;
    [SerializeField] private float playerAPushSpeed = 6f;
    [SerializeField] private float playerAPushDuration = 0.12f;

    [Header("Player B - Repel")]
    [SerializeField] private string playerBSkillName = "Repel";
    [SerializeField] private float playerBCooldown = 19f;
    [SerializeField] private float playerBRepelRadius = 3.25f;
    [SerializeField] private float playerBRepelSpeed = 9.5f;
    [SerializeField] private float playerBRepelDuration = 0.22f;

    [Header("Player C - Lunge")]
    [SerializeField] private string playerCSkillName = "Lunge";
    [SerializeField] private float playerCCooldown = 11f;
    [SerializeField] private float playerCLungeDuration = 0.12f;
    [SerializeField] private float playerCLungeDistance = 4.5f;
    [SerializeField] private float playerCLungeHitRadius = 0.8f;
    [SerializeField] private float playerCLungeDamage = 6f;
    [SerializeField] private bool playerCLungeInvulnerable = true;

    [Header("Visual Feedback")]
    [SerializeField] private Color activeSkillTint = new Color(0.55f, 0.9f, 1f, 0.65f);

    private readonly HashSet<Enemy> lungeHitEnemies = new HashSet<Enemy>();
    private PlayerController player;
    private SpriteRenderer spriteRenderer;
    private Color originalSpriteColor = Color.white;
    private SkillType selectedSkill;
    private SkillType activeSkill;
    private float cooldownRemaining;
    private float activeRemaining;
    private Vector2 lungeDirection;
    private bool hasActiveSkill;
    private bool hasOriginalSpriteColor;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
            hasOriginalSpriteColor = true;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            player = PlayerController.Instance;
        }

        selectedSkill = GetSelectedSkill();
        UpdateSkillHud();
    }

    private void Update()
    {
        if (player == null || !player.gameObject.activeSelf)
        {
            return;
        }

        float deltaTime = Time.deltaTime;

        if (cooldownRemaining > 0f)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);
        }

        if (hasActiveSkill)
        {
            activeRemaining = Mathf.Max(0f, activeRemaining - deltaTime);
            TickActiveSkill();

            if (activeRemaining <= 0f)
            {
                EndActiveSkill();
            }
        }

        if (Time.timeScale > 0f && cooldownRemaining <= 0f && !hasActiveSkill && Input.GetKeyDown(activationKey))
        {
            ActivateSkill();
        }

        UpdateSkillHud();
    }

    private SkillType GetSelectedSkill()
    {
        int characterIndex = player == null
            ? CharacterSelection.SelectedCharacterIndex
            : player.CharacterIndex;

        if (characterIndex == 1)
        {
            return SkillType.PlayerBRepel;
        }

        if (characterIndex == 2)
        {
            return SkillType.PlayerCLunge;
        }

        return SkillType.PlayerAShieldPush;
    }

    private void ActivateSkill()
    {
        selectedSkill = GetSelectedSkill();
        cooldownRemaining = GetCooldown(selectedSkill);

        switch (selectedSkill)
        {
            case SkillType.PlayerAShieldPush:
                BeginTimedSkill(selectedSkill, playerADuration);
                player.SetSkillInvulnerable(true);
                ApplySkillTint();
                PushEnemies(playerAPushRadius, playerAPushSpeed, playerAPushDuration);
                break;

            case SkillType.PlayerBRepel:
                PushEnemies(playerBRepelRadius, playerBRepelSpeed, playerBRepelDuration);
                break;

            case SkillType.PlayerCLunge:
                lungeDirection = GetLungeDirection();
                lungeHitEnemies.Clear();
                BeginTimedSkill(selectedSkill, playerCLungeDuration);
                player.SetSkillInvulnerable(playerCLungeInvulnerable);
                ApplySkillTint();
                TeleportThroughLungePath();
                break;
        }
    }

    private void BeginTimedSkill(SkillType skillType, float duration)
    {
        activeSkill = skillType;
        activeRemaining = Mathf.Max(0f, duration);
        hasActiveSkill = activeRemaining > 0f;
    }

    private void TickActiveSkill()
    {
        switch (activeSkill)
        {
            case SkillType.PlayerAShieldPush:
                PushEnemies(playerAPushRadius, playerAPushSpeed, playerAPushDuration);
                break;

            case SkillType.PlayerCLunge:
                break;
        }
    }

    private void EndActiveSkill()
    {
        if (activeSkill == SkillType.PlayerAShieldPush || activeSkill == SkillType.PlayerCLunge)
        {
            player.SetSkillInvulnerable(false);
        }

        hasActiveSkill = false;
        activeRemaining = 0f;
        RestoreSkillTint();
        lungeHitEnemies.Clear();
    }

    private void PushEnemies(float radius, float pushSpeed, float pushDuration)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        for (int i = 0; i < hits.Length; i++)
        {
            Enemy enemy = hits[i].GetComponentInParent<Enemy>();

            if (enemy == null)
            {
                continue;
            }

            Vector2 pushDirection = enemy.transform.position - transform.position;
            enemy.ApplyExternalPush(pushDirection, pushSpeed, pushDuration);
        }
    }

    private void TeleportThroughLungePath()
    {
        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + lungeDirection * playerCLungeDistance;

        HitEnemiesAroundPoint(startPosition);
        HitEnemiesAlongPath(startPosition, lungeDirection, playerCLungeDistance);
        HitEnemiesAroundPoint(endPosition);
        player.TeleportTo(endPosition);
    }

    private void HitEnemiesAlongPath(Vector2 startPosition, Vector2 direction, float distance)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(startPosition, playerCLungeHitRadius, direction, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            TryDamageLungeEnemy(hits[i].collider);
        }
    }

    private void HitEnemiesAroundPoint(Vector2 point)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, playerCLungeHitRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            TryDamageLungeEnemy(hits[i]);
        }
    }

    private void TryDamageLungeEnemy(Collider2D hitCollider)
    {
        if (hitCollider == null)
        {
            return;
        }

        Enemy enemy = hitCollider.GetComponentInParent<Enemy>();

        if (enemy == null || lungeHitEnemies.Contains(enemy))
        {
            return;
        }

        lungeHitEnemies.Add(enemy);
        enemy.TakeDamage(playerCLungeDamage);
    }

    private Vector2 GetLungeDirection()
    {
        if (player.playerMoveDirection.sqrMagnitude > 0.001f)
        {
            return player.playerMoveDirection.normalized;
        }

        if (player.lastMoveDirection.sqrMagnitude > 0.001f)
        {
            return player.lastMoveDirection.normalized;
        }

        return Vector2.down;
    }

    private float GetCooldown(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.PlayerBRepel:
                return playerBCooldown;
            case SkillType.PlayerCLunge:
                return playerCCooldown;
            default:
                return playerACooldown;
        }
    }

    private string GetSkillName()
    {
        switch (selectedSkill)
        {
            case SkillType.PlayerBRepel:
                return playerBSkillName;
            case SkillType.PlayerCLunge:
                return playerCSkillName;
            default:
                return playerASkillName;
        }
    }

    private void UpdateSkillHud()
    {
        if (UIController.Instance == null)
        {
            return;
        }

        UIController.Instance.UpdateSkillCooldown(GetSkillName(), cooldownRemaining, activeRemaining, hasActiveSkill);
    }

    private void ApplySkillTint()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (!hasOriginalSpriteColor)
        {
            originalSpriteColor = spriteRenderer.color;
            hasOriginalSpriteColor = true;
        }

        spriteRenderer.color = activeSkillTint;
    }

    private void RestoreSkillTint()
    {
        if (spriteRenderer != null && hasOriginalSpriteColor)
        {
            spriteRenderer.color = originalSpriteColor;
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.SetSkillInvulnerable(false);
        }

        RestoreSkillTint();
    }
}
