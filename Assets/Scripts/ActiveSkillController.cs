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
    [SerializeField] private float playerBRepelRadius = 20f;
    [SerializeField] private float playerBRepelSpeed = 14f;
    [SerializeField] private float playerBRepelDuration = 0.32f;

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
    [SerializeField] private AudioClip playerASkillSound;
    [SerializeField] private AudioClip playerBSkillSound;
    [SerializeField] private AudioClip playerCSkillSound;
    [SerializeField] private float skillSoundVolume = 0.75f;
    [SerializeField] private Color playerAParticleColor = new Color(0.55f, 0.9f, 1f, 0.9f);
    [SerializeField] private Color playerBParticleColor = new Color(0.78f, 1f, 0.62f, 0.9f);
    [SerializeField] private Color playerCParticleColor = new Color(1f, 0.62f, 0.95f, 0.9f);
    [SerializeField] private int skillParticleCount = 34;
    [SerializeField] private float skillParticleLifetime = 0.42f;

    private readonly HashSet<Enemy> lungeHitEnemies = new HashSet<Enemy>();
    private PlayerController player;
    private SpriteRenderer spriteRenderer;
    private Color originalSpriteColor = Color.white;
    private SkillType selectedSkill;
    private SkillType activeSkill;
    private float cooldownRemaining;
    private float activeRemaining;
    private float nextShieldShimmerTime;
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
                PlaySkillSound(playerASkillSound);
                GameFeelFeedback.PlayShieldBurst(transform.position, playerAPushRadius);
                CreateParticleBurst(transform.position, playerAParticleColor, playerAPushRadius, Mathf.Max(8, skillParticleCount / 2), skillParticleLifetime, 1.8f);
                PushEnemies(playerAPushRadius, playerAPushSpeed, playerAPushDuration);
                break;

            case SkillType.PlayerBRepel:
                PlaySkillSound(playerBSkillSound);
                GameFeelFeedback.PlayRepelBurst(transform.position, playerBRepelRadius);
                CreateParticleBurst(transform.position, playerBParticleColor, playerBRepelRadius, Mathf.Max(12, skillParticleCount / 2), skillParticleLifetime, 2.4f);
                RepelEnemiesInRadius(playerBRepelRadius, playerBRepelSpeed, playerBRepelDuration);
                break;

            case SkillType.PlayerCLunge:
                lungeDirection = GetLungeDirection();
                lungeHitEnemies.Clear();
                BeginTimedSkill(selectedSkill, playerCLungeDuration);
                player.SetSkillInvulnerable(playerCLungeInvulnerable);
                ApplySkillTint();
                PlaySkillSound(playerCSkillSound);
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
                if (Time.time >= nextShieldShimmerTime)
                {
                    nextShieldShimmerTime = Time.time + 0.45f;
                    GameFeelFeedback.PlayShieldShimmer(transform.position, playerAPushRadius);
                }
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

    private void RepelEnemiesInRadius(float radius, float pushSpeed, float pushDuration)
    {
        Vector2 repelCenter = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(repelCenter, radius);

        for (int i = 0; i < hits.Length; i++)
        {
            Enemy enemy = hits[i].GetComponentInParent<Enemy>();

            if (enemy == null)
            {
                continue;
            }

            Vector2 enemyPosition = enemy.transform.position;
            Vector2 pushDirection = enemyPosition - repelCenter;

            if (pushDirection.sqrMagnitude <= 0.001f)
            {
                pushDirection = Vector2.right;
            }

            enemy.ApplyExternalPush(pushDirection.normalized, pushSpeed, pushDuration);
        }
    }

    private void TeleportThroughLungePath()
    {
        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + lungeDirection * playerCLungeDistance;

        GameFeelFeedback.PlayLungeBurst(startPosition);
        GameFeelFeedback.PlayLungePath(startPosition, endPosition, playerCLungeHitRadius * 0.72f);
        HitEnemiesAroundPoint(startPosition);
        HitEnemiesAlongPath(startPosition, lungeDirection, playerCLungeDistance);
        HitEnemiesAroundPoint(endPosition);
        player.TeleportTo(endPosition);
        GameFeelFeedback.PlayLungeBurst(endPosition);
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

    private void PlaySkillSound(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(clip, transform.position, skillSoundVolume);
    }

    private void CreateParticleBurst(
        Vector2 position,
        Color particleColor,
        float radius,
        int particleCount,
        float lifetime,
        float speed)
    {
        ParticleSystem particles = CreateParticleSystem("Skill Burst", position, particleColor, lifetime, speed);
        ParticleSystem.MainModule main = particles.main;
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = Mathf.Max(0.1f, radius);
        shape.radiusThickness = 0.08f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Clamp(particleCount, 1, 128)) });

        particles.Play();
        Destroy(particles.gameObject, lifetime + 0.4f);
    }

    private void CreateLungePathEffect(Vector2 startPosition, Vector2 endPosition)
    {
        Vector2 midpoint = (startPosition + endPosition) * 0.5f;
        Vector2 direction = endPosition - startPosition;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
        {
            return;
        }

        ParticleSystem particles = CreateParticleSystem("Lunge Path", midpoint, playerCParticleColor, 0.26f, 0.35f);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        particles.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        ParticleSystem.MainModule main = particles.main;
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(distance, playerCLungeHitRadius * 1.25f, 0.1f);

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });

        particles.Play();
        Destroy(particles.gameObject, 0.7f);
    }

    private ParticleSystem CreateParticleSystem(
        string objectName,
        Vector2 position,
        Color particleColor,
        float lifetime,
        float speed)
    {
        GameObject effectObject = new GameObject(objectName);
        effectObject.transform.position = position;

        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.08f;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.55f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
        main.startColor = particleColor;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;

        ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sortingOrder = 20;

        return particles;
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
