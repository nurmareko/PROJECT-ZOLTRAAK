using UnityEngine;

public class SpinWeaponProjectile : MonoBehaviour
{
    private SpinWeapon weapon;

    void Start()
    {
        SpinWeaponPrefab weaponPrefab = GetComponentInParent<SpinWeaponPrefab>();

        if (weaponPrefab != null)
        {
            weapon = weaponPrefab.weapon;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider){
        if (weapon != null && collider.gameObject.CompareTag("Enemy")){
            Enemy enemy = collider.gameObject.GetComponent<Enemy>();
            enemy.TakeDamage(weapon.stats[weapon.weaponLevel].damage);
        }
    }
}
