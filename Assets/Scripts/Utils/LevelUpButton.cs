using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{
    public TMP_Text weaponName;
    public TMP_Text weaponDescription;
    public Image weaponIcon;
    public Sprite fallbackIcon;

    private Action assignedUpgrade;

    public void ActivateButton(Weapon weapon)
    {
        if (weapon == null)
        {
            ActivateButton("Upgrade", "No weapon available.", null, null);
            return;
        }

        ActivateButton(
            weapon.name,
            GetWeaponDescription(weapon),
            weapon.weaponImage,
            weapon.LevelUp);
    }

    public void ActivateButton(string upgradeName, string upgradeDescription, Sprite upgradeIcon, Action upgradeAction)
    {
        if (weaponName != null)
        {
            weaponName.text = upgradeName;
        }

        if (weaponDescription != null)
        {
            weaponDescription.text = upgradeDescription;
        }

        SetIcon(upgradeIcon != null ? upgradeIcon : fallbackIcon);
        assignedUpgrade = upgradeAction;
    }

    public void SelectUpgrade()
    {
        assignedUpgrade?.Invoke();
        UIController.Instance.LevelUpPanelClose();
        AudioController.Instance.PlaySound(AudioController.Instance.selectUpgrade);
    }

    private string GetWeaponDescription(Weapon weapon)
    {
        if (weapon.stats == null || weapon.stats.Count == 0)
        {
            return "Improve this weapon.";
        }

        if (weapon.weaponLevel >= weapon.stats.Count - 1)
        {
            return "This weapon is already at max level.";
        }

        return weapon.stats[weapon.weaponLevel + 1].description;
    }

    private void SetIcon(Sprite sprite)
    {
        if (weaponIcon == null)
        {
            return;
        }

        weaponIcon.sprite = sprite;
        weaponIcon.enabled = sprite != null;
    }
}
