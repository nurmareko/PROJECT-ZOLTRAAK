using UnityEngine;

public class AreaWeaponPrefab : MonoBehaviour
{
    public AreaWeapon weapon;
    private Vector3 targetSize;
    private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weapon = GameObject.Find("Area Weapon").GetComponent<AreaWeapon>();
        Destroy(gameObject, weapon.duration);
        targetSize = Vector3.one;
        transform.localScale = Vector3.zero;
        timer = weapon.duration;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            targetSize,
            Time.deltaTime * 5
        );

        timer -= Time.deltaTime; 
        if (timer <= 0)
        {
            targetSize = Vector3.zero;

            if (transform.localScale.x == 0f)
            {
                Destroy(gameObject);
            }
        }

    }
}
