using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{
    public TMP_Text weaponName;
    public TMP_Text weaponDescription;
    public Image weaponIcon;

    public void ActivateButton(Weapon weapon)
    {
        weaponName.text = weapon.name;
        weaponDescription.text = weapon.stats[weapon.weaponLevel].description;
        weaponIcon.sprite = weapon.weaponImage;
    }
}
