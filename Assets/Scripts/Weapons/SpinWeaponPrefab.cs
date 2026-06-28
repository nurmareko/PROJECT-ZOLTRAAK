using UnityEngine;

public class SpinWeaponPrefab : MonoBehaviour
{
    public SpinWeapon weapon;
    private float duration;
    private Vector3 targetSize;
    private float spinDirection = 1f;
    [SerializeField] private GameObject projectile;

    void Start()
    {
        if (weapon == null)
        {
            weapon = GetComponentInParent<SpinWeapon>();
        }

        if (weapon == null)
        {
            Destroy(gameObject);
            return;
        }

        duration = weapon.stats[weapon.weaponLevel].duration;
        //Destroy(gameObject, duration);
        targetSize = Vector3.one;
        transform.localScale = Vector3.zero;
        projectile.transform.localPosition = new Vector3(0f, weapon.stats[weapon.weaponLevel].range, 0f);
        weapon.PlaySpawnSound(AudioController.Instance != null ? AudioController.Instance.spinWeaponSpawn : null);
    }

    void Update()
    {   
        // rotate
        transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z + (90 * spinDirection * Time.deltaTime * weapon.stats[weapon.weaponLevel].speed));
        // grow
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetSize, Time.deltaTime * 3);
        // shrink
        duration -= Time.deltaTime;
        if (duration <= 0){
            targetSize = Vector3.zero;
            if (transform.localScale.x == 0f){
                weapon.PlayDespawnSound(AudioController.Instance != null ? AudioController.Instance.spinWeaponDespawn : null);
                Destroy(gameObject);
            }
        }
    }

    public Vector2 GetSwirlDirection(Vector2 enemyPosition)
    {
        Vector2 center = transform.position;
        Vector2 radialDirection = enemyPosition - center;

        if (radialDirection.sqrMagnitude <= 0.001f)
        {
            radialDirection = (Vector2)projectile.transform.position - center;
        }

        radialDirection.Normalize();
        Vector2 tangentDirection = spinDirection > 0f
            ? new Vector2(-radialDirection.y, radialDirection.x)
            : new Vector2(radialDirection.y, -radialDirection.x);

        return ((tangentDirection * 0.8f) + (radialDirection * 0.35f)).normalized;
    }

    public void SetRotationOffset(float rotationOffset, float rotationDirection){
        spinDirection = Mathf.Sign(rotationDirection);
        transform.rotation = Quaternion.Euler(0f, 0f, rotationOffset);
    }
}
