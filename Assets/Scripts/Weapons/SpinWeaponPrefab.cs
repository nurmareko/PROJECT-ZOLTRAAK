using UnityEngine;

public class SpinWeaponPrefab : MonoBehaviour
{
    private const float BaseSpinDegreesPerSecond = 150f;
    private const float PulseFrequency = 8f;
    private const float PulseAmount = 0.16f;

    public SpinWeapon weapon;
    private float duration;
    private float startingDuration;
    private Vector3 targetSize;
    private float spinDirection = 1f;
    private float orbitRange;
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
        startingDuration = duration;
        //Destroy(gameObject, duration);
        targetSize = Vector3.one;
        transform.localScale = Vector3.zero;
        orbitRange = weapon.stats[weapon.weaponLevel].range;
        SetProjectileOrbitPosition(orbitRange);
        AudioController audioController = AudioController.Instance;
        if (audioController != null)
        {
            audioController.PlaySound(audioController.spinWeaponSpawn);
        }
    }

    void Update()
    {   
        // rotate
        float spinSpeed = BaseSpinDegreesPerSecond * spinDirection * weapon.stats[weapon.weaponLevel].speed;
        transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z + (spinSpeed * Time.deltaTime));

        float elapsed = startingDuration - duration;
        float pulse = Mathf.Sin(elapsed * PulseFrequency) * PulseAmount;
        SetProjectileOrbitPosition(orbitRange + pulse);

        // grow
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetSize, Time.deltaTime * 4);
        // shrink
        duration -= Time.deltaTime;
        if (duration <= 0){
            targetSize = Vector3.zero;
            if (transform.localScale.x == 0f){
                AudioController audioController = AudioController.Instance;
                if (audioController != null)
                {
                    audioController.PlaySound(audioController.spinWeaponDespawn);
                }
                Destroy(gameObject);
            }
        }
    }

    public Vector2 GetSwirlDirection(Vector2 enemyPosition)
    {
        Vector2 center = transform.position;
        Vector2 radialDirection = enemyPosition - center;

        if (radialDirection.sqrMagnitude <= 0.001f && projectile != null)
        {
            radialDirection = (Vector2)projectile.transform.position - center;
        }

        radialDirection = radialDirection.sqrMagnitude > 0.001f ? radialDirection.normalized : Vector2.up;
        Vector2 tangentDirection = spinDirection > 0f
            ? new Vector2(-radialDirection.y, radialDirection.x)
            : new Vector2(radialDirection.y, -radialDirection.x);

        return ((tangentDirection * 1.1f) + (radialDirection * 0.2f)).normalized;
    }

    public void SetRotationOffset(float rotationOffset, float rotationDirection){
        spinDirection = Mathf.Sign(rotationDirection);
        transform.rotation = Quaternion.Euler(0f, 0f, rotationOffset);
    }

    public Vector2 GetOutwardDirection(Vector2 enemyPosition)
    {
        Vector2 outwardDirection = enemyPosition - (Vector2)transform.position;

        if (outwardDirection.sqrMagnitude <= 0.001f && projectile != null)
        {
            outwardDirection = (Vector2)projectile.transform.position - (Vector2)transform.position;
        }

        return outwardDirection.sqrMagnitude > 0.001f ? outwardDirection.normalized : Vector2.up;
    }

    private void SetProjectileOrbitPosition(float distance)
    {
        if (projectile == null)
        {
            return;
        }

        projectile.transform.localPosition = new Vector3(0f, Mathf.Max(0.25f, distance), 0f);
    }
}
