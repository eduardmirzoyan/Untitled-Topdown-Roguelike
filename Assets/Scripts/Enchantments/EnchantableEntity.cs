using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnchantableEntity : MonoBehaviour
{
    [SerializeField] private List<Enchantment> enchantments;
    private bool isPaused;

    private void FixedUpdate()
    {
        if (isPaused)
            return;

        // Tick every enchantment
        foreach (var enchant in enchantments)
            enchant.onTick();
    }

    public void addEnchantment(Enchantment enchantmentToAdd)
    {
        // Check if you already have this enchantment
        foreach (var enchantment in enchantments)
        {
            // If the type of the enchantment is the same as the one you want to add
            if (enchantment.GetType() == enchantmentToAdd.GetType())
            {
                // Increase stacks
                enchantment.stacks++;
                // Then finish
                return;
            }
        }

        var copy = Instantiate(enchantmentToAdd);

        // Else add the enchantment as a new one then intialize it
        enchantments.Add(copy);
        copy.intialize(gameObject);
    }

    public void removeEnchantment(Enchantment enchantmentToRemove)
    {
        foreach (var enchantment in enchantments)
        {
            // If you have the requested enchantment, uninsitalize it, remove it then break the loop
            if (enchantment.GetType() == enchantmentToRemove.GetType())
            {
                enchantment.stacks--;
                if (enchantment.stacks == 0) {
                    enchantment.unintialize();
                    enchantments.Remove(enchantment);
                }
                return;
            }
        }

        // Else print error
        print("ENCHANTMENT NOT FOUND: " + enchantmentToRemove.name);
    }

    public List<Enchantment> getEnchantments() {
        return enchantments;
    }
}
