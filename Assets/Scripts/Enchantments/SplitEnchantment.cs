using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enchantments/Split")]
public class SplitEnchantment : Enchantment
{
    [SerializeField] private int splitQuantity = 4;

    public override void intialize(GameObject gameObject)
    {
        base.intialize(gameObject);

        GameEvents.instance.onHit += AddSplitEffect;
    }

    // Basic deinitalize just nullifies current attatched gameobject
    public override void unintialize()
    {
        GameEvents.instance.onHit -= AddSplitEffect;
        base.unintialize();
    }

    private void AddSplitEffect(Projectile projectile) {
        // Check for weapon
        var weapon = projectile.getSource().GetComponent<Weapon>();
        if (weapon != null && this.weapon == weapon && !projectile.IsCopy()) {
            // Randomize offset
            var offset = Random.Range(0, 180);

            // Make 4 copys 
            for (int i = 0; i < splitQuantity; i++)
            {  
                var proj = Instantiate(projectile.gameObject, projectile.transform.position, Quaternion.Euler(0, 0,offset + 360 / splitQuantity * i)).GetComponent<Projectile>();
                proj.Copy(projectile);
                proj.setSize(0.5f);
                proj.SetRange(1.5f);
                proj.SetDamage(1);
            }
            
        }
    }
}
