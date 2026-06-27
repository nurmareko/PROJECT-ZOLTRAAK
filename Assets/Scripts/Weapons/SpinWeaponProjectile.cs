using System.Collections.Generic;
using UnityEngine;

public class SpinWeaponProjectile : MonoBehaviour
{
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

    private void OnTriggerStay2D(Collider2D collider){
        if (collider.CompareTag("Enemy")){
            TryHitEnemy(collider);
            SwirlEnemy(collider);
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

        float hitCooldown = Mathf.Max(0.08f, 0.35f / Mathf.Max(weapon.stats[weapon.weaponLevel].speed, 0.1f));

        if (nextHitTimes.TryGetValue(enemy, out float nextHitTime) && Time.time < nextHitTime)
        {
            return;
        }

        nextHitTimes[enemy] = Time.time + hitCooldown;
        enemy.TakeDamage(weapon.stats[weapon.weaponLevel].damage);
    }

    private void SwirlEnemy(Collider2D collider)
    {
        if (weapon == null || weaponPrefab == null)
        {
            return;
        }

        Vector2 swirlDirection = weaponPrefab.GetSwirlDirection(collider.transform.position);
        float shoveDistance = (0.25f + weapon.stats[weapon.weaponLevel].speed * 0.08f) * Time.fixedDeltaTime;
        Rigidbody2D enemyBody = collider.attachedRigidbody;

        if (enemyBody != null)
        {
            enemyBody.MovePosition(enemyBody.position + (swirlDirection * shoveDistance));
            return;
        }

        collider.transform.position += (Vector3)(swirlDirection * shoveDistance);
    }
}
