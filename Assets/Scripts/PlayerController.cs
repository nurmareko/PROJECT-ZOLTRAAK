using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] public Vector2 playerMoveDirection;

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

    }

    void FixedUpdate()
    {
        rigidBody.linearVelocity = new Vector2(
            playerMoveDirection.x * moveSpeed,
            playerMoveDirection.y * moveSpeed
            );
    }
}
