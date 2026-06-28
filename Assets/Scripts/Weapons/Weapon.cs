using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int weaponLevel;
    public List<WeaponStat> stats;
    public Sprite weaponImage;

    [Header("Optional Weapon SFX")]
    public AudioSource spawnSound;
    public AudioSource despawnSound;
    public AudioSource hitSound;

    public void LevelUp()
    {
        if (weaponLevel < stats.Count - 1)
        {
            weaponLevel++;
        }
    }

    public void PlaySpawnSound(AudioSource fallbackSound)
    {
        PlayWeaponSound(spawnSound, fallbackSound);
    }

    public void PlayDespawnSound(AudioSource fallbackSound)
    {
        PlayWeaponSound(despawnSound, fallbackSound);
    }

    public void PlayHitSound(AudioSource fallbackSound)
    {
        PlayWeaponSound(hitSound, fallbackSound);
    }

    private void PlayWeaponSound(AudioSource weaponSound, AudioSource fallbackSound)
    {
        if (AudioController.Instance == null)
        {
            return;
        }

        AudioController.Instance.PlaySound(weaponSound != null ? weaponSound : fallbackSound);
    }
}

[System.Serializable]
public class WeaponStat
{
    public float cooldown;
    public float duration;
    public int amount = 1;
    public float damage;
    public float range;
    public float speed;
    public string description;
}
