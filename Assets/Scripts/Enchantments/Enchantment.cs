using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enchantment : ScriptableObject
{
    [SerializeField] public string enchantmentName;
    [SerializeField] public int stacks = 1;
    protected GameObject entity;
    protected Weapon weapon;

    [TextArea(15, 20)]
    [SerializeField] public string description;

    // Basic intialize just takes in the gameobject it is attached to
    public virtual void intialize(GameObject gameObject)
    {
        entity = gameObject;
        weapon = gameObject.GetComponent<Weapon>();
        if (weapon == null) {
            throw new System.Exception("WEAPON NOT FOUND");
        }
    }

    public virtual void onTick()
    {
        // Do nothing on tick originally
    }

    // Basic deinitalize just nullifies current attatched gameobject
    public virtual void unintialize()
    {
        entity = null;
    }
}