using UnityEngine;

public class DirectionalWeapon : Weapon
{   
    [SerializeField] private GameObject prefab;
    private float spawnCounter;

    void Update()
    {
        spawnCounter -= Time.deltaTime;
        if (spawnCounter <= 0){
            spawnCounter = stats[weaponLevel].cooldown;
            for (int i = 0; i < stats[weaponLevel].amount; i++){
                GameObject spawnedWeapon = Instantiate(prefab, transform.position, transform.rotation, transform);
                DirectionalWeaponPrefab weaponPrefab = spawnedWeapon.GetComponent<DirectionalWeaponPrefab>();

                if (weaponPrefab != null)
                {
                    weaponPrefab.Initialize(this, GetProjectileDirection(i));
                }
            }
        }
    }

    private Vector2 GetProjectileDirection(int projectileIndex)
    {
        Vector2 lastMoveDirection = PlayerController.Instance.lastMoveDirection;

        if (lastMoveDirection.sqrMagnitude <= 0.001f)
        {
            lastMoveDirection = Vector2.right;
        }

        Vector2 forwardDirection = lastMoveDirection.normalized;

        switch (projectileIndex)
        {
            case 0:
                return forwardDirection;
            case 1:
                return -forwardDirection;
            case 2:
                return Vector2.up;
            case 3:
                return Vector2.down;
            case 4:
                return new Vector2(1f, 1f).normalized;
            case 5:
                return new Vector2(-1f, 1f).normalized;
            case 6:
                return new Vector2(1f, -1f).normalized;
            case 7:
                return new Vector2(-1f, -1f).normalized;
            default:
                float angle = (projectileIndex - 8) * 45f;
                Vector3 rotatedDirection = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
                return ((Vector2)rotatedDirection).normalized;
        }
    }
}
