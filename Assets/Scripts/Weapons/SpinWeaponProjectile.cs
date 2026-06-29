using System.Collections.Generic;
using UnityEngine;

public class SpinWeaponProjectile : MonoBehaviour
{
    private const float MinHitCooldown = 0.1f;
    private const float BaseHitCooldown = 0.5f;

    private SpinWeapon weapon;
    private SpinWeaponPrefab weaponPrefab;
    private readonly Dictionary<Enemy, float> nextHitTimes = new Dictionary<Enemy, float>();

    void Start()
    {
        weaponPrefab = GetComponentInParent<SpinWeaponPrefab>();

        if (weaponPrefab != null)
        {
            weapon = weaponPrefab.weapon;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider){
        if (collider.CompareTag("Enemy")){
            TryHitEnemy(collider);
        }
    }

    private void OnTriggerExit2D(Collider2D collider){
        if (collider.CompareTag("Enemy")){
            Enemy enemy = collider.GetComponent<Enemy>();

            if (enemy != null)
            {
                nextHitTimes.Remove(enemy);
            }
        }
    }

    private void TryHitEnemy(Collider2D collider)
    {
        if (weapon == null)
        {
            return;
        }

        Enemy enemy = collider.GetComponent<Enemy>();

        if (enemy == null)
        {
            return;
        }

        float hitCooldown = Mathf.Max(MinHitCooldown, BaseHitCooldown / Mathf.Max(weapon.stats[weapon.weaponLevel].speed, 0.1f));

        if (nextHitTimes.TryGetValue(enemy, out float nextHitTime) && Time.time < nextHitTime)
        {
            return;
        }

        nextHitTimes[enemy] = Time.time + hitCooldown;
        enemy.TakeDamage(weapon.stats[weapon.weaponLevel].damage);
        KnockEnemyOutward(collider);
    }

    private void KnockEnemyOutward(Collider2D collider)
    {
        if (weapon == null || weaponPrefab == null)
        {
            return;
        }

        Vector2 outwardDirection = weaponPrefab.GetOutwardDirection(collider.transform.position);
        float knockDistance = 0.03f + (weapon.stats[weapon.weaponLevel].speed * 0.008f);
        MoveEnemy(collider, outwardDirection, knockDistance);
    }

    private void MoveEnemy(Collider2D collider, Vector2 direction, float distance)
    {
        Rigidbody2D enemyBody = collider.attachedRigidbody;

        if (enemyBody != null)
        {
            enemyBody.MovePosition(enemyBody.position + (direction * distance));
            return;
        }

        collider.transform.position += (Vector3)(direction * distance);
    }
}
