using UnityEngine;

public class DirectionalWeaponPrefab : MonoBehaviour
{
    public DirectionalWeapon weapon;
    private Rigidbody2D rb;
    private Vector2 direction;
    private float duration;
    private bool hasDirection;

    public void Initialize(DirectionalWeapon owner, Vector2 fireDirection)
    {
        weapon = owner;
        direction = fireDirection.normalized;
        hasDirection = true;
    }

    void Start()
    {
        if (weapon == null)
        {
            weapon = GetComponentInParent<DirectionalWeapon>();
        }

        if (weapon == null)
        {
            Destroy(gameObject);
            return;
        }

        if (!hasDirection)
        {
            direction = PlayerController.Instance.lastMoveDirection.normalized;
        }

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = Vector2.right;
        }

        duration = weapon.stats[weapon.weaponLevel].duration;
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * weapon.stats[weapon.weaponLevel].speed;
        //Destroy(gameObject, weapon.stats[weapon.weaponLevel].duration);
        AudioController audioController = AudioController.Instance;
        if (audioController != null)
        {
            audioController.PlaySound(audioController.directionalWeaponSpawn);
        }
    }


    void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0){
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 5);
            if (transform.localScale.x == 0f){
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider){
        if (collider.gameObject.CompareTag("Enemy")){
            Enemy enemy = collider.gameObject.GetComponent<Enemy>();
            enemy.TakeDamage(weapon.stats[weapon.weaponLevel].damage);
            AudioController audioController = AudioController.Instance;
            if (audioController != null)
            {
                audioController.PlaySound(audioController.directionalWeaponHit);
            }
        }
    }
}
