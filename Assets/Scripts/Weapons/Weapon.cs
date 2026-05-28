using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int weaponLevel;
    public List<WeaponStat> stats;
    public Sprite weaponImage;
}

[System.Serializable]
public class WeaponStat
{
    public float cooldown;
    public float duration;
    public float damage;
    public float range;
    public float speed;
    public string description;
}
