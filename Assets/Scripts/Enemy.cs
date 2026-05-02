using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] private SpriteRenderer spriteRenderer;

    // Update is called once per frame
    void Update()
    {
        float playerX = PlayerController.Instance.transform.position.x;
        float enemyX = gameObject.transform.position.x;
        bool isPlayerOnRight = playerX > enemyX;

        spriteRenderer.flipX = isPlayerOnRight;
    }
}
