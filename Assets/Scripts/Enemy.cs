using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float moveSpeed;
    private Vector2 direction;

    // Update is called once per frame
    void Update()
    {
        float playerX = PlayerController.Instance.transform.position.x;
        float enemyX = gameObject.transform.position.x;
        bool isPlayerOnRight = playerX > enemyX;
        spriteRenderer.flipX = isPlayerOnRight;

        Vector2 playerPosn = PlayerController.Instance.transform.position;
        direction = (playerPosn - (Vector2) transform.position).normalized;
    }

    void FixedUpdate()
    {
        rigidBody.linearVelocity = new Vector2(
            direction.x * moveSpeed,
            direction.y * moveSpeed
            );
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
