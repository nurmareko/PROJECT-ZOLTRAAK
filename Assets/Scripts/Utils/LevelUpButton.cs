using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{
    public TMP_Text weaponName;
    public TMP_Text weaponDescription;
    public Image weaponIcon;

    private Action assignedUpgrade;

    public void ActivateButton(Weapon weapon)
    {
        if (weapon == null)
        {
            ActivateButton("Upgrade", "No weapon available.", null, null);
            return;
        }

        weaponName.text = weapon.name;
        weaponDescription.text = GetWeaponDescription(weapon);
        SetIcon(weapon.weaponImage);
        ApplyReadability();

        assignedUpgrade = weapon.LevelUp;
    }

    public void ActivateButton(string upgradeName, string upgradeDescription, Sprite upgradeIcon, Action upgradeAction)
    {
        weaponName.text = upgradeName;
        weaponDescription.text = upgradeDescription;
        SetIcon(upgradeIcon);
        ApplyReadability();

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

    private void ApplyReadability()
    {
        if (weaponName != null)
        {
            weaponName.enableAutoSizing = true;
            weaponName.fontSizeMin = 24f;
            weaponName.fontSizeMax = 42f;
            weaponName.fontStyle = FontStyles.Bold;
            weaponName.alignment = TextAlignmentOptions.Center;
            weaponName.raycastTarget = false;
        }

        if (weaponDescription != null)
        {
            weaponDescription.enableAutoSizing = true;
            weaponDescription.fontSizeMin = 18f;
            weaponDescription.fontSizeMax = 30f;
            weaponDescription.fontStyle = FontStyles.Normal;
            weaponDescription.alignment = TextAlignmentOptions.Center;
            weaponDescription.raycastTarget = false;
        }
    }
}
