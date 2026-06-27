using UnityEngine;

public class SpinWeapon : Weapon
{
    public GameObject prefab;
    private float spawnCounter;

    void Update()
    {
        spawnCounter -= Time.deltaTime;
        if (spawnCounter <= 0){
            spawnCounter = stats[weaponLevel].cooldown;

            for (int i = 0; i < stats[weaponLevel].amount; i++){
                GameObject spawnedWeapon = Instantiate(prefab, transform.position, transform.rotation, transform);
                SpinWeaponPrefab weaponPrefab = spawnedWeapon.GetComponent<SpinWeaponPrefab>();

                if (weaponPrefab != null)
                {
                    weaponPrefab.weapon = this;
                    float rotation = 360f / stats[weaponLevel].amount * i;
                    float spinDirection = i % 2 == 0 ? 1f : -1f;
                    weaponPrefab.SetRotationOffset(rotation, spinDirection);
                }
            }

        }
    }
}
