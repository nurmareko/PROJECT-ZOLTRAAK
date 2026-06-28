using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float health;
    [SerializeField] private GameObject enemyDeathEffect;
    [SerializeField] private int experienceToGive;
    [SerializeField] private float pushTime;
    [SerializeField] private float damageFlashDuration = 0.08f;
    [SerializeField] private Color damageFlashColor = new Color(1f, 0.35f, 0.35f, 1f);

    private float pushCounter;
    private float externalPushTimer;
    private Vector2 externalPushVelocity;
    private Vector2 direction;
    private bool isDying;
    private Color originalSpriteColor = Color.white;
    private Coroutine damageFlashCoroutine;

    void Awake()
    {
        EnsureSpriteRenderer();

        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
        }
    }

    void OnDisable()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor;
        }
    }

    void FixedUpdate()
    {
        if (PlayerController.Instance.gameObject.activeSelf)
        {
            // Face the player
            float playerX = PlayerController.Instance.transform.position.x;
            float enemyX = gameObject.transform.position.x;
            bool isPlayerOnRight = playerX > enemyX;
            spriteRenderer.flipX = isPlayerOnRight;

            if (externalPushTimer > 0f)
            {
                externalPushTimer -= Time.fixedDeltaTime;
                rigidBody.linearVelocity = externalPushVelocity;
                return;
            }

            // Push back enemy
            if (pushCounter > 0)
            {
                pushCounter -= Time.deltaTime;
                if (moveSpeed > 0)
                {
                    moveSpeed = -moveSpeed;
                }

                if (pushCounter <= 0)
                {
                    moveSpeed = Mathf.Abs(moveSpeed);
                }
            }

            // Move toward player
            Vector2 playerPosn = PlayerController.Instance.transform.position;
            direction = (playerPosn - (Vector2) transform.position).normalized;

            rigidBody.linearVelocity = new Vector2(
                direction.x * moveSpeed,
                direction.y * moveSpeed
            );
        } else
        {
            rigidBody.linearVelocity = Vector2.zero;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.Instance.takeDamage(damage);
        }
    }

    public void ApplyExternalPush(Vector2 pushDirection, float pushSpeed, float pushDuration)
    {
        if (isDying)
        {
            return;
        }

        if (pushDirection.sqrMagnitude <= 0.001f)
        {
            pushDirection = Random.insideUnitCircle.normalized;
        }

        externalPushVelocity = pushDirection.normalized * Mathf.Max(0f, pushSpeed);
        externalPushTimer = Mathf.Max(0f, pushDuration);
    }

    public void TakeDamage(float damage)
    {
        if (isDying)
        {
            return;
        }

        health -= damage;
        DamageNumberController.Instance.CreateNumber(damage, transform.position);

        pushCounter = pushTime;
        PlayDamageFlash();

        if(health <= 0)
        {
            isDying = true;
            Destroy(gameObject);
            Instantiate(enemyDeathEffect, transform.position, transform.rotation);
            ExperiencePickup.Create(transform.position, experienceToGive);
            AudioController.Instance.PlaySound(AudioController.Instance.enemyDie);
        }
    }

    private void PlayDamageFlash()
    {
        if (!isActiveAndEnabled || !EnsureSpriteRenderer())
        {
            return;
        }

        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
        }

        damageFlashCoroutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor;
        }

        damageFlashCoroutine = null;
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
