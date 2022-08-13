using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents instance;

    private void Awake() {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public event Action<Projectile> onHit;
    public event Action<Weapon, Projectile> onFire;

    public void TriggerOnHit(Projectile projectile)
    {
        if (onHit != null)
        {
            onHit(projectile);
        }
    }

    public void TriggerOnFire(Weapon weapon, Projectile projectile)
    {
        if (onFire != null)
        {
            onFire(weapon, projectile);
        }
    }
}
