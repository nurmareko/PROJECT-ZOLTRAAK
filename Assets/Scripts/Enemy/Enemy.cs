using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float health;
    [SerializeField] private GameObject enemyDeathEffect;
    private Vector2 direction;

    void FixedUpdate()
    {
        if (PlayerController.Instance.gameObject.activeSelf)
        {
            float playerX = PlayerController.Instance.transform.position.x;
            float enemyX = gameObject.transform.position.x;
            bool isPlayerOnRight = playerX > enemyX;
            spriteRenderer.flipX = isPlayerOnRight;

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

    public void TakeDamage(float damage)
    {
        health -= damage;

        if(health <= 0)
        {
            Destroy(gameObject);
            Instantiate(enemyDeathEffect, transform.position, transform.rotation);
        }
    }
}
