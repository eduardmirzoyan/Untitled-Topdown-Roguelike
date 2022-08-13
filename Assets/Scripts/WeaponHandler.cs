using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    // Weapon here
    [SerializeField] private Weapon weapon;

    public void usePrimaryWeapon() {
        if (weapon != null) {
            // Fire weapon
            weapon.startFiring();
        }
    }

    public void releasePrimaryWeapon() {
        if (weapon != null) {
            // Fire weapon
            weapon.stopFiring();
        }
    }

}
